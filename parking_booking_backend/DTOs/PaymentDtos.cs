namespace parking_booking_backend.DTOs;

public sealed record PaymentWebhookRequest(string Code, PaymentWebhookData Data);

public sealed record PaymentWebhookData(decimal Amount, string Description, string TransactionDateTime);
