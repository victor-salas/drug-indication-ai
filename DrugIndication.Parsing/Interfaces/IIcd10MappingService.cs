namespace DrugIndication.Parsing.Interfaces
{
    public interface IIcd10MappingService
    {
        Task<(string Code, string Description, string Source)> MapToIcd10Async(string text);
    }
}
