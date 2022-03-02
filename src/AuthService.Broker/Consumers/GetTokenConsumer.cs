using System.Threading.Tasks;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Auth;
using LT.DigitalOffice.Models.Broker.Responses.Auth;
using MassTransit;

namespace LT.DigitalOffice.AuthService.Broker.Consumers
{
  public class GetTokenConsumer : IConsumer<IGetTokenRequest>
  {
    private readonly ITokenEngine _tokenEngine;

    public GetTokenConsumer(ITokenEngine tokenEngine)
    {
      _tokenEngine = tokenEngine;
    }

    public async Task Consume(ConsumeContext<IGetTokenRequest> context)
    {
      var response = OperationResultWrapper.CreateResponse(GetTokenResult, context.Message);

      await context.RespondAsync<IOperationResult<IGetTokenResponse>>(response);
    }

    private object GetTokenResult(IGetTokenRequest request)
    {
      return IGetTokenResponse.CreateObj(
        _tokenEngine.Create(request.UserId, TokenType.Access, out double accessTokenExpiresIn),
        _tokenEngine.Create(request.UserId, TokenType.Refresh, out double refreshTokenExpiresIn),
        accessTokenExpiresIn,
        refreshTokenExpiresIn);
    }
  }
}
