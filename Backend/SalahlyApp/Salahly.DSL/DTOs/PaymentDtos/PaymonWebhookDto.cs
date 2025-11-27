using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.PaymentDtos
{
    public class PaymobWebhookDto
    {
        [JsonPropertyName("obj")]
        public PaymobWebhookObject Obj { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        // Extracted properties from Obj for convenience
        public int TransactionId => Obj?.Id ?? 0;
        public int OrderId => Obj?.Order?.Id ?? 0;
        public bool Success => Obj?.Success ?? false;
        public int AmountCents => Obj?.AmountCents ?? 0;
        public string Currency => Obj?.Currency ?? "";
        public bool ErrorOccurred => Obj?.ErrorOccurred ?? false;
        public bool HasParentTransaction => Obj?.HasParentTransaction ?? false;
        public int IntegrationId => Obj?.IntegrationId ?? 0;
        public bool IsAuth => Obj?.IsAuth ?? false;
        public bool IsCapture => Obj?.IsCapture ?? false;
        public bool IsRefunded => Obj?.IsRefunded ?? false;
        public bool IsStandalonePayment => Obj?.IsStandalonePayment ?? false;
        public bool IsVoided => Obj?.IsVoided ?? false;
        public int Owner => Obj?.Owner ?? 0;
        public bool Pending => Obj?.Pending ?? false;
        public string SourceDataPan => Obj?.SourceData?.Pan ?? "";
        public string SourceDataSubType => Obj?.SourceData?.SubType ?? "";
        public string SourceDataType => Obj?.SourceData?.Type ?? "";
        public string CreatedAt => Obj?.CreatedAt ?? "";
        public string? Hmac => Obj?.Hmac; // ✅ Made nullable
    }

    public class PaymobWebhookObject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("error_occured")]
        public bool ErrorOccurred { get; set; }

        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("integration_id")]
        public int IntegrationId { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("owner")]
        public int Owner { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobSourceData SourceData { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("order")]
        public PaymobOrder Order { get; set; }

        [JsonPropertyName("hmac")]
        public string? Hmac { get; set; } // ✅ Made nullable
    }

    public class PaymobSourceData
    {
        [JsonPropertyName("pan")]
        public string Pan { get; set; }

        [JsonPropertyName("sub_type")]
        public string SubType { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class PaymobOrder
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string MerchantOrderId { get; set; }
    }
}