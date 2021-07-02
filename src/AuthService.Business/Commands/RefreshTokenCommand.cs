using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Business.Helpers.Token.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.AuthService.Business.Commands
{
    public class RefreshTokenCommand : IRefreshTokenCommand
    {
        private readonly ITokenEngine _tokenEngine;
        private readonly ITokenValidator _tokenValidator;

        public RefreshTokenCommand(
            ITokenEngine tokenEngine,
            ITokenValidator tokenValidator)
        {
            _tokenEngine = tokenEngine;
            _tokenValidator = tokenValidator;
        }

        public OperationResultResponse<string> Execute(string token)
        {
            return new OperationResultResponse<string>
            {
                Status = OperationResultStatusType.FullSuccess,
                Body = _tokenEngine.Create(_tokenValidator.Validate(token))
            };
        }
    }
}
