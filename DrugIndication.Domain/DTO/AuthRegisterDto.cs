namespace DrugIndication.Domain.DTO
{
    public class AuthRegisterDto
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "user"; // default: user
    }
}
