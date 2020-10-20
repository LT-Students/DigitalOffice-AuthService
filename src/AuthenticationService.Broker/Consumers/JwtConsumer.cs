using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Broker.Consumers
{
    public class JwtConsumer : IConsumer<IUserJwtRequest>
    {
        private readonly IJwtValidator _jwtValidator;

        public JwtConsumer([FromServices] IJwtValidator jwtValidator)
        {
            _jwtValidator = jwtValidator;
        }

        public async Task Consume(ConsumeContext<IUserJwtRequest> context)
        {
            var response = OperationResultWrapper.CreateResponse(GetValidationResultJwt, context.Message);

            await context.RespondAsync<IOperationResult<bool>>(response);
        }

        private bool GetValidationResultJwt(IUserJwtRequest request)
        {
            _jwtValidator.ValidateAndThrow(request.UserJwt);

            return true;
        }
    }
}