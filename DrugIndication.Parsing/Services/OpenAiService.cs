using DrugIndication.Domain.Config;
using DrugIndication.Domain.Entities;
using DrugIndication.Domain.Interfaces;
using DrugIndication.Domain.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DrugIndication.Parsing.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAiService(IOptions<OpenAiOptions> options)
        {
            _apiKey = options.Value.ApiKey;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.openai.com/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<(string? Code, string? Description)> GetStandardizedDiagnosisAsync(string indicationText)
        {
            var prompt = $@"
                        Given the following medical condition: {indicationText}, return the best matching ICD-10 code and its official description.
                        Return only a JSON object in this format:
                        {{{{""code"": ""L20.9"", ""description"": ""Atopic dermatitis, unspecified""}}}}.
                        If the condition cannot be mapped, return: {{{{""code"": null, ""description"": ""UNKNOWN""}}}}";

            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a medical assistant helping map free-text indications to ICD-10 codes and descriptions." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(request);
            var response = await _httpClient.PostAsync("v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return (null, null);

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            try
            {
                using var parsed = JsonDocument.Parse(content);
                var root = parsed.RootElement;
                var code = root.GetProperty("code").GetString();
                var description = root.GetProperty("description").GetString();
                return (code, description);
            }
            catch
            {
                return (null, null);
            }
        }        

        public async Task<List<EligibilityRequirement>> ExtractKeyValuePairsAsync(string eligibilityText)
        {
            var prompt = $@"
                        You are a helpful AI assistant that transforms eligibility requirement text from patient assistance programs into structured JSON.
                        Given the following eligibility text, extract a list of objects with 'Name' and 'Value' representing the structured requirements.
                        Respond with ONLY a JSON array. Do not explain anything.

                        Text:
                        {eligibilityText}

                        JSON Output Example:
                        [
                          {{ ""Name"": ""us_residency"", ""Value"": ""true"" }},
                          {{ ""Name"": ""insurance_coverage"", ""Value"": ""true"" }},
                          {{ ""Name"": ""minimum_age"", ""Value"": ""18"" }},
                        ]";

            var result = await AskOpenAiAsync(prompt);
            if (string.IsNullOrWhiteSpace(result)) return new();

            try
            {
                return JsonSerializer.Deserialize<List<EligibilityRequirement>>(result) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<List<Benefit>> ExtractBenefitsAsync(string programDetails)
        {
            var prompt = $@"
                        You are an AI that transforms unstructured text from copay card benefit descriptions into structured JSON.
                        Given the following text, extract a list of objects with 'Name' and 'Value' representing patient benefits or program features.
                        Respond ONLY with a JSON array. Do not explain anything.

                        Text:
                        {programDetails}

                        JSON Output Example:
                        [
                          {{ ""Name"": ""max_annual_savings"", ""Value"": ""13000.00"" }},
                          {{ ""Name"": ""min_out_of_pocket"", ""Value"": ""0.00"" }}
                        ]";

            var result = await AskOpenAiAsync(prompt);
            if (string.IsNullOrWhiteSpace(result)) return new();

            try
            {
                return JsonSerializer.Deserialize<List<Benefit>>(result) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<DateTime?> NormalizeExpirationDateAsync(string rawText)
        {
            var prompt = $"""
                        Given this expiration date string: "{rawText}", return a single ISO-8601 formatted date string (e.g., 2025-12-31).
                        If the text is ambiguous (e.g. 'end of calendar year'), assume December 31 of the current or next year.
                         You should return only the string date, like 2025-12-31, no tags or response format is required.
                        If it's unknown, return null.
                        """;

            var result = await AskOpenAiAsync(prompt);
            if (DateTime.TryParse(result, out var date))
                return date;

            return null;
        }

        public async Task<List<Benefit>> ExtractBenefitsFromAllSourcesAsync(RawProgramInput input)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Extract all structured benefits from the following fields:");
            sb.AppendLine($"ProgramDetails: {input.ProgramDetails}");
            sb.AppendLine($"AnnualMax: {input.AnnualMax}");
            sb.AppendLine($"MaximumBenefit: {input.MaximumBenefit}");
            sb.AppendLine($"AddRenewalDetails: {input.AddRenewalDetails}");
            sb.AppendLine($"CouponVehicle: {input.CouponVehicle}");
            sb.AppendLine($"ActivationReq: {input.ActivationReq}");
            sb.AppendLine($"ActivationMethod: {input.ActivationMethod}");
            sb.AppendLine($"FreeTrialOffer: {input.FreeTrialOffer}");
            sb.AppendLine($"OfferRenewable: {input.OfferRenewable}");
            sb.AppendLine("Respond ONLY with a JSON array. Do not explain anything. JSON Output Example:");
            sb.AppendLine("[{\"Name\": \"max_annual_savings\", \"Value\": \"13000.00\"}]");

            var prompt = sb.ToString();
            var result = await AskOpenAiAsync(prompt);
            if (string.IsNullOrWhiteSpace(result)) return new();

            try
            {
                return JsonSerializer.Deserialize<List<Benefit>>(result) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<List<Dictionary<string, string>>> ExtractDetailedProgramInfoAsync(string programDetailsText)
        {
            var prompt = $@"
            Analyze the following patient support program details and return structured categories in JSON format with keys like eligibility, program, renewal, income, etc.
            Respond ONLY with a JSON array. Do not explain anything.

            Input:
            {programDetailsText}

            JSON Output Example:
                        [
                          {{{{ """"eligibility"""": """"...""""}}}},
                          {{{{ """"program"""": """"..."""",}}}},
                          {{{{ """"renewal"""": """"..."""",}}}},
                          {{{{ """"income"""": """"..."""",}}}}
                        ]""
            ";

            var json = await AskOpenAiAsync(prompt);
            if (string.IsNullOrWhiteSpace(json)) return new();

            try
            {
                return JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json) ?? new();
            }
            catch
            {
                return new();
            }
        }

        private async Task<string?> AskOpenAiAsync(string prompt)
        {
            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are an assistant extracting structured healthcare information in JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.2
            };

            var json = JsonSerializer.Serialize(request);
            var response = await _httpClient.PostAsync("v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()?.Trim();
        }
    }
}
