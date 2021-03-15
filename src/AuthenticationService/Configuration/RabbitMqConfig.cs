using LT.DigitalOffice.Kernel.Broker;

namespace LT.DigitalOffice.AuthenticationService.Configuration
{
    public class RabbitMqConfig : BaseRabbitMqOptions
    {
        public string UserServiceCredentialsUrl { get; set; }
        public string AuthenticationServiceValidationEndpoint { get; set; }
    }
}
