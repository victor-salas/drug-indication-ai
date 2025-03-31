using DrugIndication.Domain.Entities;
using DrugIndication.Domain.Models;

namespace DrugIndication.API.Models
{
    public class CoPayProgramDto
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public List<string> Drugs { get; set; }
        public List<string> TherapeuticAreas { get; set; }
        public List<string> CoverageEligibilities { get; set; }
        public string ProgramType { get; set; }
        public List<RequirementDto> Requirements { get; set; }
        public List<RequirementDto> Benefits { get; set; }
        public List<FormDto> Forms { get; set; }
        public FundingDto Funding { get; set; }
        public ProgramDetailsDto Details { get; set; }
        public string ProgramUrl { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<IndicationMapping> Indications { get; set; } = new();
        public List<AssociatedFoundation> AssociatedFoundations { get; set; } = new();

    }
}
