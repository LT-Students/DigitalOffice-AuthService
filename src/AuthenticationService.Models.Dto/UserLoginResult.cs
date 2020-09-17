using System;

namespace LT.DigitalOffice.AuthenticationService.Models.Dto
{
    public class UserLoginResult
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}