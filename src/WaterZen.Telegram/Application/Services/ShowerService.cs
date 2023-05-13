using MediatR;
using WaterZen.Telegram.Application.Mediator.Messages;

namespace WaterZen.Telegram.Application.Services
{
    internal class ShowerService
    {
        private readonly IMediator _mediator;
        public bool IsSessionActive { get; private set; }
        public ShowerSession? CurrentShowerSession { get; private set; }

        public ShowerService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void StartSession()
        {
            if (IsSessionActive)
            {
                return;
            }

            CurrentShowerSession = new ShowerSession();
            CurrentShowerSession.StartSession();
            IsSessionActive = true;
        }

        public void AddFlowRate(decimal flowRate)
        {
            CurrentShowerSession?.AddFlowRate(flowRate);
        }

        public void AddTemperature(decimal temperature)
        {
            CurrentShowerSession?.AddTemperature(temperature);
        }

        public void CheckClosingSession()
        {
            if (IsSessionActive && CurrentShowerSession?.FlowRates.Count > 0)
            {
                var lastDateTime = CurrentShowerSession?.FlowRates.Last().Item1;
                if (lastDateTime != null && DateTime.Now.Subtract(lastDateTime.Value) > new TimeSpan(0, 0, 10))
                {
                    IsSessionActive = false;
                    CurrentShowerSession?.EndSession();

                    _mediator.Send(new WaterData
                    {
                        Temperature = CurrentShowerSession.Temperatures.First().Item2,
                        FlowRate = CurrentShowerSession.FlowRates.First().Item2
                    });
                }
            }
        }
    }
}
