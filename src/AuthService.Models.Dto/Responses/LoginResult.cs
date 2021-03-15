using System;

namespace LT.DigitalOffice.AuthService.Models.Dto.Responses
{
    public class LoginResult
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}