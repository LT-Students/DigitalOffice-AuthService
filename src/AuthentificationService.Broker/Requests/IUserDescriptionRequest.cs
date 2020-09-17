using System;

namespace LT.DigitalOffice.AuthentificationService.Broker.Requests
{
    public interface IUserDescriptionRequest
    {
        Guid Id { get; }
        string Email { get; }
        string FirstName { get; }
        string LastName { get; }
        string MiddleName { get; }
    }
}
