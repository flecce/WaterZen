using MediatR;
using System.Runtime.Serialization;

namespace WaterZen.Telegram.Application.Mediator.Messages
{
    [DataContract]
    public class WaterData : IRequest<bool>
    {
        [DataMember]
        public decimal Temperature { get; set; }
        [DataMember]
        public decimal FlowRate { get; set; }
    }
}
