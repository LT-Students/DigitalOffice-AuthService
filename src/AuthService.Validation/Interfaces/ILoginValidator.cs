using FluentValidation;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;

namespace LT.DigitalOffice.AuthService.Validation.Interfaces
{
    public interface ILoginValidator : IValidator<LoginRequest>
    {
    }
}
