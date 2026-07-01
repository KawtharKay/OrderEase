namespace Application.Common.Settings
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; } = default!;
        public string BaseUrl { get; set; } = "https://api.paystack.co";
    }
}