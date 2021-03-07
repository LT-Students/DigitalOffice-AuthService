using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Broker.Consumers
{
    public class JwtConsumer : IConsumer<ICheckTokenRequest>
    {
        private readonly ITokenValidator _tokenValidator;

        public JwtConsumer([FromServices] ITokenValidator jwtValidator)
        {
            _tokenValidator = jwtValidator;
        }

        public async Task Consume(ConsumeContext<ICheckTokenRequest> context)
        {
            var response = OperationResultWrapper.CreateResponse(GetValidationResultJwt, context.Message);

            await context.RespondAsync<IOperationResult<Guid>>(response);
        }

        private Guid GetValidationResultJwt(ICheckTokenRequest request)
        {
            return _tokenValidator.Validate(request.Token);
        }
    }
}