using MediatR;

namespace WaterZen.Telegram.Application
{
    internal class ShowerSession : IRequest<bool>
    {
        public Guid Id { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public List<Tuple<DateTime, double>> FlowRates { get; set; } = new List<Tuple<DateTime, double>>();
        public List<Tuple<DateTime, double>> Temperatures { get; set; } = new List<Tuple<DateTime, double>>();

        public ShowerSession()
        {
            Id = Guid.NewGuid();
        }

        public void StartSession()
        {
            StartDate = DateTime.Now;
        }

        public void EndSession()
        {
            EndDate = DateTime.Now;
        }

        public void AddFlowRate(double rate)
        {
            FlowRates.Add(new Tuple<DateTime, double>(DateTime.Now, rate));
        }

        public void AddTemperature(double temperature)
        {
            Temperatures.Add(new Tuple<DateTime, double>(DateTime.Now, temperature));
        }
    }
}
