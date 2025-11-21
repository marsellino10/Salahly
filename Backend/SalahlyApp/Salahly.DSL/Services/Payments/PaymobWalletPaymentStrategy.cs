using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class PaymobWalletPaymentStrategy : IPaymentStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymobWalletPaymentStrategy> _logger;

        // Paymob Configuration for Wallets
        private readonly string _apiKey;
        private readonly string _walletIntegrationId; // Different from card integration
        private readonly string _iframeId;
        private readonly string _hmacSecret;
        private readonly string _baseUrl;

        public PaymobWalletPaymentStrategy(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PaymobWalletPaymentStrategy> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Load config from appsettings.json - use different integration ID for wallets
            _apiKey = _configuration["Paymob:ApiKey"];
            _walletIntegrationId = _configuration["Paymob:WalletIntegrationId"]; // Separate config
            _iframeId = _configuration["Paymob:WalletIframeId"]; // Could be same or different
            _hmacSecret = _configuration["Paymob:HmacSecret"];
            _baseUrl = _configuration["Paymob:BaseUrl"] ?? "https://accept.paymob.com/api";
        }

        public string GetProviderName() => "Paymob_Wallet";

        public async Task<PaymentInitializationResult> InitializeAsync(
            PaymentInitializationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Initializing Paymob Wallet payment for Booking: {request.BookingId}");

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

                // Step 3: Generate Payment Key (with wallet integration ID)
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
                        ErrorMessage = "Failed to generate payment key for wallet"
                    };
                }

                // Step 4: Generate Payment Link for Wallets
                var paymentLink = $"{_baseUrl}/acceptance/iframes/{_iframeId}?payment_token={paymentKey}";

                _logger.LogInformation($"Paymob Wallet payment initialized. Order: {orderId}");

                return new PaymentInitializationResult
                {
                    IsSuccess = true,
                    PaymentLink = paymentLink,
                    PaymentToken = paymentKey,
                    TransactionId = orderId,

                    MetaData = new Dictionary<string, object>
                    {
                        { "AuthToken", authToken },
                        { "OrderId", orderId },
                        { "PaymentMethod", "Wallet" },
                        { "IntegrationType", "Wallet" }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Paymob Wallet payment");
                return new PaymentInitializationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Wallet payment initialization failed: {ex.Message}"
                };
            }
        }

        public async Task<PaymentVerificationResult> VerifyAsync(
            string transactionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Verifying Paymob Wallet transaction: {transactionId}");

                // Reuse the same verification logic as card payments
                var authToken = await GetAuthTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(authToken))
                {
                    return new PaymentVerificationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to authenticate with Paymob"
                    };
                }

                var url = $"{_baseUrl}/acceptance/transactions/{transactionId}";
                _httpClient.DefaultRequestHeaders.Clear();

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentVerificationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to verify wallet payment with Paymob"
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
                    PaidAmount = transaction?.Amount / 100m ?? 0,
                    TransactionId = transactionId,
                    PaymentDate = DateTime.UtcNow,
                    ErrorMessage = isSuccess ? null : "Wallet payment not successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying Paymob Wallet transaction: {transactionId}");
                return new PaymentVerificationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Wallet verification failed: {ex.Message}"
                };
            }
        }

        public async Task<RefundResult> RefundAsync(
            RefundRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Processing Paymob Wallet refund for transaction: {request.OriginalTransactionId}");

                // Use same refund logic as card payments
                var authToken = await GetAuthTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(authToken))
                {
                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to authenticate with Paymob"
                    };
                }

                var refundPayload = new
                {
                    auth_token = authToken,
                    transaction_id = request.OriginalTransactionId,
                    amount_cents = (int)(request.RefundAmount * 100)
                };

                var url = $"{_baseUrl}/acceptance/void_refund/refund";
                var jsonContent = JsonSerializer.Serialize(refundPayload);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError($"Paymob Wallet refund failed: {errorContent}");

                    return new RefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Failed to process wallet refund with Paymob"
                    };
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var refundResponse = JsonSerializer.Deserialize<PaymobRefundResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation($"Paymob Wallet refund successful: {refundResponse?.Id}");

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
                _logger.LogError(ex, "Error processing Paymob Wallet refund");
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Wallet refund failed: {ex.Message}"
                };
            }
        }

        #region Private Helper Methods

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
                _logger.LogError(ex, "Error getting Paymob auth token for wallet");
                return null;
            }
        }

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
                    amount_cents = (int)(request.Amount * 100),
                    currency = "EGP",
                    merchant_order_id = $"WALLET_{request.BookingId}",
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
                _logger.LogError(ex, "Error creating Paymob order for wallet");
                return null;
            }
        }

        private async Task<string?> GeneratePaymentKeyAsync(
            string authToken,
            string orderId,
            PaymentInitializationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var customerName = request.CustomerName ?? "Test User";
                var nameParts = customerName.Split(' ');
                var firstName = nameParts.Length > 0 ? nameParts[0] : "Test";
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "User";

                var payload = new
                {
                    auth_token = authToken,
                    amount_cents = (int)(request.Amount * 100),
                    expiration = 3600,
                    order_id = orderId,
                    billing_data = new
                    {
                        apartment = "NA",
                        email = request.CustomerEmail ?? "test@example.com",
                        floor = "NA",
                        first_name = firstName,
                        street = request.CustomerAddress ?? "NA",
                        building = "NA",
                        phone_number = request.CustomerPhone ?? "+201000000000",
                        shipping_method = "NA",
                        postal_code = "00000",
                        city = "Cairo",
                        country = "EG",
                        last_name = lastName,
                        state = "Cairo"
                    },
                    currency = "EGP",
                    integration_id = int.Parse(_walletIntegrationId) // Use wallet-specific integration
                };

                var url = $"{_baseUrl}/acceptance/payment_keys";
                var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError($"❌ Paymob Wallet Payment Key Error: {errorContent}");
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
                _logger.LogError(ex, "Error generating Paymob wallet payment key");
                return null;
            }
        }

        #endregion

        #region Paymob Response Models (Reuse from card or define here)

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
