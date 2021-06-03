using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Model.Broker.Requests.Token;
using LT.DigitalOffice.Models.Broker.Requests.User;

namespace LT.DigitalOffice.AuthService.Models.Dto.Configurations
{
    public class RabbitMqConfig : BaseRabbitMqConfig
    {
        [AutoInjectRequest(typeof(IGetUserCredentialsRequest))]
        public string GetUserCredentialsEndpoint { get; set; }
        [AutoInjectRequest(typeof(IGetTokenRequest))]
        public string GetTokenEndpoint { get; set; }
    }
}
