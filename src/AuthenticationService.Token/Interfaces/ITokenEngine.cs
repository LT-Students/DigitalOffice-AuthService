using System;

namespace LT.DigitalOffice.AuthenticationService.Token.Interfaces
{
    public interface ITokenEngine
    {
        /// <summary>
        /// Create new token based on user login.
        /// </summary>
        string Create(Guid userId);
    }
}
