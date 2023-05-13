namespace WaterZen.Telegram.Application.Services.Interfaces
{
    internal interface IMQTTService
    {
        Task Listen(CancellationToken cancellationToken);
    }
}
