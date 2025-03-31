using DrugIndication.Domain.Entities;
using DrugIndication.Domain.Interfaces;
using DrugIndication.Domain.Models;
using DrugIndication.Parsing.Interfaces;
using DrugIndication.Parsing.Services;

namespace DrugIndication.Parsing.Transformers
{
    public class ProgramTransformer
    {
        private readonly IIcd10MappingService _mappingService;
        private readonly IOpenAiService _aiService;

        public ProgramTransformer(IIcd10MappingService mappingService, IOpenAiService aiService)
        {
            _mappingService = mappingService;
            _aiService = aiService;
        }

        public async Task<ProgramDto> TransformAsync(RawProgramInput input)
        {
            var dto = new ProgramDto
            {
                ProgramID = input.ProgramID,
                ProgramName = input.ProgramName,
                Drugs = input.Drugs ?? new(),
                ProgramUrl = input.ProgramUrl
            };

            // Map indications from TherapeuticAreas
            if (input.TherapeuticAreas != null)
            {
                foreach (var text in input.TherapeuticAreas)
                {
                    var result = await _mappingService.MapToIcd10Async(text);
                    dto.Indications.Add(new IndicationMapping
                    {
                        Description = text,
                        Icd10Code = result.Code,
                        Icd10Description = result.Description,
                        MappingSource = result.Source
                    });
                }
            }

            // Use AI to extract requirements from EligibilityDetails
            if (!string.IsNullOrWhiteSpace(input.EligibilityDetails))
            {
                var raw = await _aiService.ExtractKeyValuePairsAsync(input.EligibilityDetails);
                dto.Requirements.AddRange(raw);
            }

            // Use AI to extract benefits
            if (!string.IsNullOrWhiteSpace(input.ProgramDetails))
            {
                var raw = await _aiService.ExtractBenefitsFromAllSourcesAsync(input);
                dto.Benefits.AddRange(raw);
            }

            // Formar ProgramDetails
            if (!string.IsNullOrWhiteSpace(input.ProgramDetails))
            {
                dto.Details = await _aiService.ExtractDetailedProgramInfoAsync(input.ProgramDetails);
            }

            // Normalize expiration date
            dto.ExpirationDate = await _aiService.NormalizeExpirationDateAsync(input.ExpirationDate);

            // Process associated foundations
            if (input.AssociatedFoundations != null)
            {
                foreach (var af in input.AssociatedFoundations)
                {
                    var foundation = new AssociatedFoundation
                    {
                        ProgramID = af.ProgramID,
                        ProgramName = af.ProgramName,
                        FoundationFundLevels = af.FoundationFundLevels ?? new(),
                        TherapAreas = af.TherapAreas ?? new(),
                        Drugs = af.Drugs ?? new()
                    };

                    foreach (var area in af.TherapAreas)
                    {
                        var result = await _mappingService.MapToIcd10Async(area);
                        foundation.Indications.Add(new IndicationMapping
                        {
                            Description = area,
                            Icd10Code = result.Code,
                            Icd10Description = result.Description,
                            MappingSource = result.Source
                        });
                    }

                    dto.AssociatedFoundations.Add(foundation);
                }
            }

            return dto;
        }
    }
}
