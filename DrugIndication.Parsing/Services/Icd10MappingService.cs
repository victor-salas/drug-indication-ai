using DrugIndication.Domain.Interfaces;
using DrugIndication.Parsing.Interfaces;

namespace DrugIndication.Parsing.Services
{
    public class Icd10MappingService : IIcd10MappingService
    {
        private readonly IOpenAiService _aiService;
        private readonly Icd10Service _localService;

        public Icd10MappingService(IOpenAiService aiService, Icd10Service localService)
        {
            _aiService = aiService;
            _localService = localService;
        }

        /// <summary>
        /// Attempts to map a free-text medical indication to an ICD-10 code.
        /// Uses OpenAI first, then falls back to local search.
        /// </summary>
        public async Task<(string? Code, string? Description, string Source)> MapToIcd10Async(string indication)
        {
            // Step 1: Ask OpenAI to simplify the indication
            var aiSuggestedDescription = await _aiService.GetStandardizedDiagnosisAsync(indication);

            if (!string.IsNullOrWhiteSpace(aiSuggestedDescription.Code) && aiSuggestedDescription.Description.ToUpper() != "UNKNOWN")
            {
                return (aiSuggestedDescription.Code, aiSuggestedDescription.Description, "ai");
            }

            // Step 2: Fallback to local fuzzy match
            var localMatch = _localService.FindClosestMatch(indication);
            if (localMatch != null)
            {
                return (localMatch.Code, localMatch.Description, "local");
            }

            // Step 3: No match found
            return (null, null, "unmappable");
        }
    }
}
