namespace WaterZenSimulator.Services.Interface
{
    public interface IMQTTService
    {
        Task SendWaterData(WaterData data);
        Task StartAsync();
    }
}