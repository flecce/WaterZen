namespace WaterZen.Telegram.Application.Services.Interfaces
{
    internal interface IBotService
    {
        Task Start(CancellationToken cancellationToken);
        Task SendText(string text);
    }
}
