using System;

namespace LT.DigitalOffice.AuthService.Models.Dto.Responses
{
    public record LoginResult
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}