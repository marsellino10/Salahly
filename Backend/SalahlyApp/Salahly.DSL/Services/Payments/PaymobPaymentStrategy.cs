using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DSL.DTOs.PaymentDtos;
using Salahly.DSL.Interfaces.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Salahly.DSL.Services.Payments
{
    public class PaymobPaymentStrategy : IPaymentStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymobPaymentStrategy> _logger;

        // Paymob Configuration
        private readonly string _apiKey;
        private readonly string _integrationId;
        private readonly string _iframeId;
        private readonly string _hmacSecret;
        private readonly string _baseUrl;

        public PaymobPaymentStrategy(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PaymobPaymentStrategy> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Load config from appsettings.json
            _apiKey = _configuration["Paymob:ApiKey"];
            _integrationId = _configuration["Paymob:IntegrationId"];
            _iframeId = _configuration["Paymob:IframeId"];
            _hmacSecret = _configuration["Paymob:HmacSecret"];
            _baseUrl = _configuration["Paymob:BaseUrl"] ?? "https://accept.paymob.com/api";
        }

        public string GetProviderName() => "Paymob";

        /// <summary>
        /// Initialize Payment with Paymob
        /// </summary>
        public async Task<PaymentInitializationResult> InitializeAsync(
            PaymentInitializationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Initializing Paymob payment for Booking: {request.BookingId}");

                // Step 1: Authentication - Get Auth Token
                var authToken = await GetAuthTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(authToken))
                {
                    return new PaymentInitializationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to authenticate with Paymob"
                    };
                }

                // Step 2: Create Order
                var orderId = await CreateOrderAsync(authToken, request, cancellationToken);
                if (string.IsNullOrEmpty(orderId))
                {
                    return new PaymentInitializationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to create order with Paymob"
                    };
                }

                // Step 3: Generate Payment Key
                var paymentKey = await GeneratePaymentKeyAsync(
                    authToken,
                    orderId,
                    request,
                    cancellationToken);

                if (string.IsNullOrEmpty(paymentKey))
                {
                    return new PaymentInitializationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to generate payment key"
                    };
                }

                // Step 4: Generate Payment Link
                var paymentLink = $"{_baseUrl}/acceptance/iframes/{_iframeId}?payment_token={paymentKey}";

                _logger.LogInformation($"Paymob payment initialized. Order: {orderId}, Payment Key: {paymentKey}");

                return new PaymentInitializationResult
                {
                    IsSuccess = true,
                    PaymentLink = paymentLink,
                    PaymentToken = paymentKey,
                    TransactionId = orderId,
                    MetaData = new Dictionary<string, object>
                    {
                        { "AuthToken", authToken },
                        { "OrderId", orderId }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Paymob payment");
                return new PaymentInitializationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Payment initialization failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Verify Payment from Paymob
        /// </summary>
        public async Task<PaymentVerificationResult> VerifyAsync(
            string transactionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Verifying Paymob transaction: {transactionId}");

                // Get Auth Token
                var authToken = await GetAuthTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(authToken))
                {
                    return new PaymentVerificationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to authenticate with Paymob"
                    };
                }

                // Call Paymob API to get transaction details
                var url = $"{_baseUrl}/acceptance/transactions/{transactionId}";
                _httpClient.DefaultRequestHeaders.Clear();

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentVerificationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to verify payment with Paymob"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var transaction = JsonSerializer.Deserialize<PaymobTransactionResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var isSuccess = transaction?.Success == true;

                return new PaymentVerificationResult
                {
                    IsSuccess = true,
                    IsPaymentConfirmed = isSuccess,
                    PaidAmount = transaction?.Amount / 100m ?? 0, // Paymob uses cents
                    TransactionId = transactionId,
                    PaymentDate = DateTime.UtcNow,
                    ErrorMessage = isSuccess ? null : "Payment not successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying Paymob transaction: {transactionId}");
                return new PaymentVerificationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Verification failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Process Refund via Paymob
        /// </summary>
        public async Task<RefundResult> RefundAsync(
            RefundRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Processing Paymob refund for transaction: {request.OriginalTransactionId}");

                // Get Auth Token
                var authToken = await GetAuthTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(authToken))
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to authenticate with Paymob"
                    };
                }

                // Create refund request
                var refundPayload = new
                {
                    auth_token = authToken,
                    transaction_id = request.OriginalTransactionId,
                    amount_cents = (int)(request.RefundAmount * 100) // Convert to cents
                };

                var url = $"{_baseUrl}/acceptance/void_refund/refund";
                var jsonContent = JsonSerializer.Serialize(refundPayload);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError($"Paymob refund failed: {errorContent}");

                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to process refund with Paymob"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var refundResponse = JsonSerializer.Deserialize<PaymobRefundResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation($"Paymob refund successful: {refundResponse?.Id}");

                return new RefundResult
                {
                    IsSuccess = true,
                    RefundTransactionId = refundResponse?.Id.ToString() ?? request.OriginalTransactionId,
                    RefundAmount = request.RefundAmount,
                    RefundDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Paymob refund");
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Refund failed: {ex.Message}"
                };
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Step 1: Get Authentication Token
        /// </summary>
        private async Task<string?> GetAuthTokenAsync(CancellationToken cancellationToken)
        {
            try
            {
                var payload = new { api_key = _apiKey };
                var url = $"{_baseUrl}/auth/tokens";

                var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<PaymobAuthResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Paymob auth token");
                return null;
            }
        }

        /// <summary>
        /// Step 2: Create Order
        /// </summary>
        private async Task<string?> CreateOrderAsync(
            string authToken,
            PaymentInitializationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var payload = new
                {
                    auth_token = authToken,
                    delivery_needed = "false",
                    amount_cents = (int)(request.Amount * 100), // Convert to cents
                    currency = "EGP",
                    merchant_order_id = $"{request.BookingId}-{request.PaymentId}",
                    items = Array.Empty<object>()
                };

                var url = $"{_baseUrl}/ecommerce/orders";
                var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<PaymobOrderResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Paymob order");
                return null;
            }
        }

        /// <summary>
        /// Step 3: Generate Payment Key
        /// </summary>
        private async Task<string?> GeneratePaymentKeyAsync(
    string authToken,
    string orderId,
    PaymentInitializationRequest request,
    CancellationToken cancellationToken)
        {
            try
            {
                // Handle null values
                var customerName = request.CustomerName ?? "Test User";
                var nameParts = customerName.Split(' ');
                var firstName = nameParts.Length > 0 ? nameParts[0] : "Test";
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "User";

                var payload = new
                {
                    auth_token = authToken,
                    amount_cents = (int)(request.Amount * 100),
                    expiration = 3600, // 1 hour
                    order_id = orderId,
                    billing_data = new
                    {
                        apartment = "NA",
                        email = request.CustomerEmail ?? "test@example.com", // ✅ Default value
                        floor = "NA",
                        first_name = firstName,
                        street = request.CustomerAddress ?? "NA",
                        building = "NA",
                        phone_number = request.CustomerPhone ?? "+201000000000", // ✅ Default value
                        shipping_method = "NA",
                        postal_code = "00000", 
                        city = "Cairo", 
                        country = "EG",
                        last_name = lastName,
                        state = "Cairo"
                    },
                    currency = "EGP",
                    integration_id = int.Parse(_integrationId)
                };

                var url = $"{_baseUrl}/acceptance/payment_keys";
                var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError($"Paymob Payment Key Error: {errorContent}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<PaymobPaymentKeyResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Paymob payment key");
                return null;
            }
        }

        #endregion

        #region Paymob Response Models

        private class PaymobAuthResponse
        {
            public string Token { get; set; }
        }

        private class PaymobOrderResponse
        {
            public int Id { get; set; }
        }

        private class PaymobPaymentKeyResponse
        {
            public string Token { get; set; }
        }

        private class PaymobTransactionResponse
        {
            public bool Success { get; set; }
            public int Amount { get; set; }
        }

        private class PaymobRefundResponse
        {
            public int Id { get; set; }
        }

        #endregion
    }
}
