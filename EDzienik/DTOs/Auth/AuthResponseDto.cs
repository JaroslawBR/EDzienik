namespace EDzienik.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

    }
}

        

