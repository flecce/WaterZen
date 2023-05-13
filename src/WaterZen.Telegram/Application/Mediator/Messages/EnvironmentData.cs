using MediatR;
using System.Runtime.Serialization;

namespace WaterZen.Telegram.Application.Mediator.Messages
{
    [DataContract]
    public class EnvironmentData : IRequest<bool>
    {
        [DataMember]
        public decimal Temperature { get; set; }
        [DataMember]
        public decimal Humidity { get; set; }
    }
}
