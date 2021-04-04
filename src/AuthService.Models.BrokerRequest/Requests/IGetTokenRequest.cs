using System;

namespace LT.DigitalOffice.Broker.Requests
{
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
