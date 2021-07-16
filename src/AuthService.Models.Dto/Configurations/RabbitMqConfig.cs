using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Models.Broker.Requests.User;

namespace LT.DigitalOffice.AuthService.Models.Dto.Configurations
{
    public class RabbitMqConfig : BaseRabbitMqConfig
    {
        [AutoInjectRequest(typeof(IGetUserCredentialsRequest))]
        public string GetUserCredentialsEndpoint { get; set; }
        public string GetTokenEndpoint { get; set; }
    }
}
