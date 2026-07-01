using Application.Common.Settings;
using Application.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services
{
    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _settings;

        public PaystackService(HttpClient httpClient, IOptions<PaystackSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
        }

        public async Task<PaystackInitResponse> InitializeTransactionAsync(
            string email, decimal amount, string reference)
        {
            var payload = new
            {
                email,
                amount = (int)(amount * 100), // Paystack expects amount in kobo
                reference
            };

            var response = await _httpClient.PostAsJsonAsync("/transaction/initialize", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Paystack initialization failed: {content}");

            var result = JsonSerializer.Deserialize<PaystackInitApiResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new PaystackInitResponse(
                result!.Status,
                result.Data.AuthorizationUrl,
                result.Data.Reference);
        }

        public async Task<PaystackVerifyResponse> VerifyTransactionAsync(string reference)
        {
            var response = await _httpClient.GetAsync($"/transaction/verify/{reference}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Paystack verification failed: {content}");

            var result = JsonSerializer.Deserialize<PaystackVerifyApiResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new PaystackVerifyResponse(
                result!.Status,
                result.Data.Status,
                result.Data.Amount / 100m, // convert back from kobo
                result.Data.Customer.Email);
        }

        // Internal classes for deserializing Paystack's raw API response
        private class PaystackInitApiResponse
        {
            public bool Status { get; set; }
            public PaystackInitData Data { get; set; } = default!;
        }

        private class PaystackInitData
        {
            [JsonPropertyName("authorization_url")]
            public string AuthorizationUrl { get; set; } = default!;
            public string Reference { get; set; } = default!;
        }

        private class PaystackVerifyApiResponse
        {
            public bool Status { get; set; }
            public PaystackVerifyData Data { get; set; } = default!;
        }

        private class PaystackVerifyData
        {
            public string Status { get; set; } = default!;
            public decimal Amount { get; set; }
            public PaystackCustomer Customer { get; set; } = default!;
        }

        private class PaystackCustomer
        {
            public string Email { get; set; } = default!;
        }
    }
}