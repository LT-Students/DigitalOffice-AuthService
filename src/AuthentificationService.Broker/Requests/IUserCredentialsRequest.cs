namespace LT.DigitalOffice.Broker.Requests
{
    /// <summary>
    /// The model is a binding the request internal model of consumer for RabbitMQ.
    /// </summary>
    public interface IUserCredentialsRequest
    {
        string Email { get; }
    }
}
