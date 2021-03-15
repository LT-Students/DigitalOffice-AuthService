using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthService.Broker.Consumers
{
  public class TokenConsumer : IConsumer<ICheckTokenRequest>
    {
        private readonly ITokenValidator _tokenValidator;

        public TokenConsumer(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public async Task Consume(ConsumeContext<ICheckTokenRequest> context)
        {
            var response = OperationResultWrapper.CreateResponse(GetValidationResult, context.Message);

            await context.RespondAsync<IOperationResult<Guid>>(response);
        }

        private Guid GetValidationResult(ICheckTokenRequest request)
        {
            return _tokenValidator.Validate(request.Token);
        }
    }
}