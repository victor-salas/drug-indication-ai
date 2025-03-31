namespace DrugIndication.Domain.Entities
{
    public class IndicationMapping
    {
        public string Description { get; set; } = string.Empty;
        public string? Icd10Code { get; set; }
        public string? Icd10Description { get; set; }
        public string MappingSource { get; set; } = "unmappable";
    }

}
