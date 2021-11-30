using System;
using System.Threading.Tasks;
using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Business.Helpers;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.AuthService.Validation.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.User;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.AuthService.Business.Commands
{
  public class LoginCommand : ILoginCommand
  {
    private readonly ITokenEngine _tokenEngine;
    private readonly ILoginValidator _validator;
    private readonly IRequestClient<IGetUserCredentialsRequest> _requestClient;
    private readonly ILogger<LoginCommand> _logger;
    private readonly HttpContext _httpContext;

    public LoginCommand(
      ITokenEngine tokenEngine,
      ILoginValidator validator,
      IRequestClient<IGetUserCredentialsRequest> requestClient,
      IHttpContextAccessor httpContextAccessor,
      ILogger<LoginCommand> logger)
    {
      _tokenEngine = tokenEngine;
      _validator = validator;
      _requestClient = requestClient;
      _logger = logger;
      _httpContext = httpContextAccessor.HttpContext;
    }

    public async Task<LoginResult> Execute(LoginRequest request)
    {
      request.LoginData = request.LoginData.Trim();

      _logger.LogInformation(
        "User login request for LoginData: '{loginData}' from IP: '{requestIP}'.",
        request.LoginData,
        _httpContext.Connection.RemoteIpAddress);

      _validator.ValidateAndThrowCustom(request);

      IGetUserCredentialsResponse userCredentials = await GetUserCredentials(request.LoginData);

      if (userCredentials == null)
      {
        throw new NotFoundException(
          "User was not found, please check your credentials and try again. In case this error occurred again contact DO support team by email 'spartak.ryabtsev@lanit-tercom.com'.");
      }

      VerifyPasswordHash(userCredentials, request.Password);

      var result = new LoginResult
      {
        UserId = userCredentials.UserId,
        AccessToken = _tokenEngine.Create(userCredentials.UserId, TokenType.Access, out double accessTokenLifeTime),
        RefreshToken = _tokenEngine.Create(userCredentials.UserId, TokenType.Refresh, out double refreshTokenLifeTime),
        AccessTokenExpiresIn = accessTokenLifeTime,
        RefreshTokenExpiresIn = refreshTokenLifeTime
      };

      _logger.LogInformation(
        "User was successfully logged in with LoginData: '{loginData}' from IP: {requestIP}",
        request.LoginData,
        _httpContext.Connection.RemoteIpAddress);

      return result;
    }

    private async Task<IGetUserCredentialsResponse> GetUserCredentials(string loginData)
    {
      IGetUserCredentialsResponse result = null;

      try
      {
        var brokerResponse = await _requestClient.GetResponse<IOperationResult<IGetUserCredentialsResponse>>(
          IGetUserCredentialsRequest.CreateObj(loginData));

        if (!brokerResponse.Message.IsSuccess)
        {
          _logger.LogWarning("Can't get user credentials for LoginData: '{loginData}'", loginData);
        }
        else
        {
          result = brokerResponse.Message.Body;
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(
          exc,
          "Exception was caught while receiving user credentials for LoginData: {loginData}",
          loginData);
      }

      return result;
    }

    private void VerifyPasswordHash(IGetUserCredentialsResponse savedUserCredentials, string requestPassword)
    {
      string requestPasswordHash = PasswordHelper.GetPasswordHash(
        savedUserCredentials.UserLogin,
        savedUserCredentials.Salt,
        requestPassword);

      if (!string.Equals(savedUserCredentials.PasswordHash, requestPasswordHash))
      {
        throw new ForbiddenException("Wrong user credentials.");
      }
    }
  }
}
