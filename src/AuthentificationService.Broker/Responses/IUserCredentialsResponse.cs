using System;

namespace LT.DigitalOffice.Broker.Responses
{
    /// <summary>
    /// The model is a binding the response internal model of sender for RabbitMQ.
    /// </summary>
    public interface IUserCredentialsResponse
    {
        Guid UserId { get; }
        string PasswordHash { get; }
    }
}