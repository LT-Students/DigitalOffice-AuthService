using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Models.Broker.Requests.User;

namespace LT.DigitalOffice.AuthService.Models.Dto.Configurations
{
  public class RabbitMqConfig : BaseRabbitMqConfig
  {
    public string Username { get; set; }
    public string Password { get; set; }

    [AutoInjectRequest(typeof(IGetUserCredentialsRequest))]
    public string GetUserCredentialsEndpoint { get; set; }
    public string GetTokenEndpoint { get; set; }
  }
}
