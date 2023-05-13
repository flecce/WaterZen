namespace WaterZen.Telegram.Application
{
    internal class ShowerSession
    {
        public Guid Id { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public List<Tuple<DateTime, decimal>> FlowRates { get; set; } = new List<Tuple<DateTime, decimal>>();
        public List<Tuple<DateTime, decimal>> Temperatures { get; set; } = new List<Tuple<DateTime, decimal>>();

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

        public void AddFlowRate(decimal rate)
        {
            FlowRates.Add(new Tuple<DateTime, decimal>(DateTime.Now, rate));
        }

        public void AddTemperature(decimal temperature)
        {
            Temperatures.Add(new Tuple<DateTime, decimal>(DateTime.Now, temperature));
        }
    }
}
