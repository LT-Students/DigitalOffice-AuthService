using System;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;

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
    /// <param name="tokenType">Is access or refresh token</param>
    Guid Validate(string token, TokenType tokenType);
  }
}
