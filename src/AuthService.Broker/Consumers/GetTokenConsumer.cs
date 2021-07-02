using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Token;
using MassTransit;
using System.Threading.Tasks;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;

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

            await context.RespondAsync<IOperationResult<(string accessToken, string refreshToken)>>(response);
        }

        private (string accessToken, string refreshToken) GetTokenResult(IGetTokenRequest request)
        {
            return (
                _tokenEngine.Create(request.UserId, TokenType.Access),
                _tokenEngine.Create(request.UserId, TokenType.Refresh)
            );
        }
    }
}
