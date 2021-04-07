using LT.DigitalOffice.AuthService.Models.Dto.Configurations;
using LT.DigitalOffice.Kernel.Attributes;
using System;

namespace LT.DigitalOffice.Broker.Requests
{
    [AutoInjectRequest(nameof(RabbitMqConfig.GetTokenEndpoint))]
    public interface IGetTokenRequest
    {
        Guid UserId { get; }

        static object CreateObj(Guid userId)
        {
            return new
            {
                UserId = userId
            };
        }
    }
}
