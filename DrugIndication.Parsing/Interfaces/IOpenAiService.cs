using DrugIndication.Domain.Entities;
using DrugIndication.Domain.Models;

namespace DrugIndication.Domain.Interfaces;

public interface IOpenAiService
{
    Task<(string? Code, string? Description)> GetStandardizedDiagnosisAsync(string indicationText);
    Task<List<EligibilityRequirement>> ExtractKeyValuePairsAsync(string eligibilityText);
    Task<List<Benefit>> ExtractBenefitsAsync(string programDetails);
    Task<List<Benefit>> ExtractBenefitsFromAllSourcesAsync(RawProgramInput input);
    Task<DateTime?> NormalizeExpirationDateAsync(string rawText);
    Task<List<Dictionary<string, string>>> ExtractDetailedProgramInfoAsync(string programDetailsText);
}
