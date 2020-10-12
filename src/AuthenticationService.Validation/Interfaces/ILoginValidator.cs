using FluentValidation;
using LT.DigitalOffice.AuthenticationService.Models.Dto.Requests;

namespace LT.DigitalOffice.AuthenticationService.Validation.Interfaces
{
    public interface ILoginValidator : IValidator<LoginRequest>
    {
    }
}
