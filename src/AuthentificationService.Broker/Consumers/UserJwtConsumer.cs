using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Broker.Consumers
{
    public class UserJwtConsumer : IConsumer<IUserJwtRequest>
    {
        private readonly IJwtValidator jwtValidator;

        public UserJwtConsumer([FromServices] IJwtValidator jwtValidator)
        {
            this.jwtValidator = jwtValidator;
        }

        public async Task Consume(ConsumeContext<IUserJwtRequest> context)
        {
            var response = OperationResultWrapper.CreateResponse(GetValidationResultJwt, context.Message);

            await context.RespondAsync<IOperationResult<bool>>(response);
        }

        private bool GetValidationResultJwt(IUserJwtRequest request)
        {
            jwtValidator.ValidateJwt(request.UserJwt);

            return true;
        }
    }
}