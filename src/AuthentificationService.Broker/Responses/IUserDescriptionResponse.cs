using System;

namespace LT.DigitalOffice.AuthentificationService.Broker.Responses
{
    public interface IUserDescriptionResponse
    {
        Guid Id { get; }
        string Email { get; }
        string FirstName { get; }
        string LastName { get; }
        string MiddleName { get; }
    }
}
