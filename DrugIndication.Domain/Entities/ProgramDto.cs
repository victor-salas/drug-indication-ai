using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DrugIndication.Domain.Entities
{
    public class ProgramDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int ProgramID { get; set; }
        public required string ProgramName { get; set; }
        public List<string>? Drugs { get; set; }
        public List<string>? TherapeuticAreas { get; set; }
        public List<string>? CoverageEligibilities { get; set; }
        public string? ProgramType { get; set; }
        public List<EligibilityRequirement>? Requirements { get; set; } = new();
        public List<Benefit>? Benefits { get; set; } = new();
        public List<ProgramForm>? Forms { get; set; }
        public FundingInfo? Funding { get; set; }
        public List<Dictionary<string, string>>? Details { get; set; }
        public string? ProgramUrl { get; set; }
        public List<IndicationMapping>? Indications { get; set; } = new();
        public List<AssociatedFoundation>? AssociatedFoundations { get; set; } = new();
        public DateTime? ExpirationDate { get; set; }
    }
}
