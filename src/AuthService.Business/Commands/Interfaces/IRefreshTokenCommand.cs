using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;

namespace LT.DigitalOffice.AuthService.Business.Commands.Interfaces
{
    [AutoInject]
    public interface IRefreshTokenCommand
    {
        OperationResultResponse<string> Execute(string token);
    }
}
