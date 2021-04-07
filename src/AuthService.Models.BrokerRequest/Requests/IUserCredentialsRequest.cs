using LT.DigitalOffice.AuthService.Models.Dto.Configurations;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.Broker.Requests
{
    /// <summary>
    /// The model is a binding the request internal model of consumer for RabbitMQ.
    /// </summary>
    [AutoInjectRequest(nameof(RabbitMqConfig.GetUserCredentialsEndpoint))]
    public interface IUserCredentialsRequest
    {
        string LoginData { get; }

        static object CreateObj(string loginData)
        {
            return new
            {
                LoginData = loginData
            };
        }
    }
}
