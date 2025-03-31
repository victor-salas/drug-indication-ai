namespace DrugIndication.Domain.Config
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = "DrugApi";
        public string Audience { get; set; } = "DrugUsers";
        public int ExpirationMinutes { get; set; } = 60;
    }
}
