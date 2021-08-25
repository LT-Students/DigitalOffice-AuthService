using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using System;
using System.Threading.Tasks;
using LT.DigitalOffice.AuthService.Models.Dto.Enums;

namespace LT.DigitalOffice.AuthService.Broker.Consumers
{
  public class CheckTokenConsumer : IConsumer<ICheckTokenRequest>
  {
    private readonly ITokenValidator _tokenValidator;

    public CheckTokenConsumer(ITokenValidator tokenValidator)
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
      return _tokenValidator.Validate(request.Token, TokenType.Access);
    }
  }
}
