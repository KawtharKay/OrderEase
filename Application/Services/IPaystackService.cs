namespace Application.Services
{
    public interface IPaystackService
    {
        Task<PaystackInitResponse> InitializeTransactionAsync(string email, decimal amount, string reference);
        Task<PaystackVerifyResponse> VerifyTransactionAsync(string reference);
    }

    public record PaystackInitResponse(bool Status, string AuthorizationUrl, string Reference);
    public record PaystackVerifyResponse(bool Status, string ReferenceStatus, decimal AmountPaid, string CustomerEmail);
}