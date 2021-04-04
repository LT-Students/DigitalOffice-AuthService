using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using System.Threading.Tasks;

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

            await context.RespondAsync<IOperationResult<string>>(response);
        }

        private string GetTokenResult(IGetTokenRequest request)
        {
            return _tokenEngine.Create(request.UserId);
        }
    }
}
