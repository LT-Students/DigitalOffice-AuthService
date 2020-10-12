using System;

namespace LT.DigitalOffice.AuthenticationService.Models.Dto.Responses
{
    public class LoginResult
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}