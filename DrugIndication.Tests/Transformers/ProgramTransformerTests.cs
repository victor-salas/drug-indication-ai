using DrugIndication.Domain.Entities;
using DrugIndication.Domain.Interfaces;
using DrugIndication.Domain.Models;
using DrugIndication.Parsing.Interfaces;
using DrugIndication.Parsing.Transformers;
using FluentAssertions;
using Moq;

namespace DrugIndication.Tests.Transformers;

public class ProgramTransformerTests
{
    [Fact]
    public async Task TransformAsync_Should_MapAllFieldsCorrectly()
    {
        // Arrange
        var mockMapping = new Mock<IIcd10MappingService>();
        var mockAi = new Mock<IOpenAiService>();

        var input = new RawProgramInput
        {
            ProgramID = 1,
            ProgramName = "Test Program",
            Drugs = new() { "DrugX" },
            TherapeuticAreas = new() { "Asthma" },
            EligibilityDetails = "Must be 18+",
            ProgramDetails = "Up to $1,000 in coverage.",
            ExpirationDate = "Expires at end of calendar year",
            ProgramUrl = "http://example.com",
            AssociatedFoundations = new List<RawAssociatedFoundation>
            {
                new()
                {
                    ProgramID = 999,
                    ProgramName = "Test Foundation",
                    TherapAreas = new() { "Asthma" },
                    Drugs = new() { "DrugY" },
                    FoundationFundLevels = new() { "Low" }
                }
            }
        };

        // ICD-10 mock for both TherapeuticAreas and AssociatedFoundations
        mockMapping.Setup(m => m.MapToIcd10Async("Asthma"))
            .ReturnsAsync((Code: "J45", Description: "Asthma", Source: "ai"));

        mockAi.Setup(a => a.ExtractKeyValuePairsAsync("Must be 18+"))
            .ReturnsAsync(new List<EligibilityRequirement>
            {
                new() { Name = "age", Value = "18+" }
            });

        mockAi.Setup(a => a.ExtractBenefitsFromAllSourcesAsync(input))
            .ReturnsAsync(new List<Benefit>
            {
                new() { Name = "max_annual_savings", Value = "1000" }
            });

        mockAi.Setup(a => a.ExtractDetailedProgramInfoAsync("Up to $1,000 in coverage."))
            .ReturnsAsync(new List<Dictionary<string, string>>
            {
                new() { { "program", "Up to $1,000 in coverage." } }
            });

        mockAi.Setup(a => a.NormalizeExpirationDateAsync("Expires at end of calendar year"))
            .ReturnsAsync(new DateTime(2025, 12, 31));

        var transformer = new ProgramTransformer(mockMapping.Object, mockAi.Object);

        // Act
        var result = await transformer.TransformAsync(input);

        // Assert
        result.ProgramID.Should().Be(1);
        result.ProgramName.Should().Be("Test Program");
        result.Indications.Should().ContainSingle(i => i.Icd10Code == "J45");
        result.Requirements.Should().ContainSingle(r => r.Name == "age");
        result.Benefits.Should().ContainSingle(b => b.Name == "max_annual_savings");
        result.ExpirationDate.Should().Be(new DateTime(2025, 12, 31));
        result.Details.Should().ContainSingle().And.Contain(d => d.ContainsKey("program"));
        result.AssociatedFoundations.Should().ContainSingle();
        result.AssociatedFoundations[0].Indications.Should().ContainSingle(i => i.Icd10Code == "J45");
    }
}
