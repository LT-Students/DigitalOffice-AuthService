using LT.DigitalOffice.Kernel.Configurations;

namespace LT.DigitalOffice.AuthService.Models.Dto.Configurations
{
    public class RabbitMqConfig : BaseRabbitMqConfig
    {
        public string GetUserCredentialsEndpoint { get; set; }
        public string GetTokenEndpoint { get; set; }
    }
}
