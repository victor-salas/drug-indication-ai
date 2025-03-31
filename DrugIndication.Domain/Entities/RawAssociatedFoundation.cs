namespace DrugIndication.Domain.Models
{
    public class RawAssociatedFoundation
    {
        public int ProgramID { get; set; }

        public string ProgramName { get; set; } = string.Empty;

        public List<string> FoundationFundLevels { get; set; } = new();

        public List<string> TherapAreas { get; set; } = new();

        public List<string> Drugs { get; set; } = new();
    }
}
