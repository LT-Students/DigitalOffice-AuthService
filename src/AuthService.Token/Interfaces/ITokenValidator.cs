using System;

namespace LT.DigitalOffice.AuthService.Token.Interfaces
{
    /// <summary>
    /// Represents interface for user token validator.
    /// </summary>
    public interface ITokenValidator
    {
        /// <summary>
        /// Validate user token.
        /// </summary>
        /// <param name="token">User token.</param>
        Guid Validate(string token);
    }
}
