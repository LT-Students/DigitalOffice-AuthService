using LT.DigitalOffice.Kernel.Broker;

namespace LT.DigitalOffice.AuthService.Configuration
{
    public class RabbitMqConfig : BaseRabbitMqOptions
    {
        public string GetUserCredentialsEndpoint { get; set; }
    }
}
