using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Token;
using MassTransit;
using System.Threading.Tasks;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;
using LT.DigitalOffice.Models.Broker.Responses.Auth;

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
                _tokenEngine.Create(request.UserId, TokenType.Access, out double accessTokenLifeTime),
                _tokenEngine.Create(request.UserId, TokenType.Refresh, out double refreshTokenLifeTime),
                accessTokenLifeTime,
                refreshTokenLifeTime);
        }
    }
}
