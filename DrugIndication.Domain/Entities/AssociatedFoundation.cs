namespace DrugIndication.Domain.Entities
{
    public class AssociatedFoundation
    {
        public int ProgramID { get; set; }
        public string ProgramName { get; set; } = string.Empty;
        public List<string> FoundationFundLevels { get; set; } = new();
        public List<string> TherapAreas { get; set; } = new();
        public List<string> Drugs { get; set; } = new();
        public List<IndicationMapping> Indications { get; set; } = new();
    }

}
