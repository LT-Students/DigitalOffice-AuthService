using System;

namespace LT.DigitalOffice.AuthentificationService.Models.Dto
{
    public class UserLoginResult
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}