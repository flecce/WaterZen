using MediatR;
using WaterZen.Telegram.Application.Mediator.Messages;
using WaterZen.Telegram.Application.Services.Interfaces;

namespace WaterZen.Telegram.Application.Mediator.Handler
{
    internal class EnvironmentDataHandler : IRequestHandler<EnvironmentData, bool>
    {
        private readonly IBotService _botService;

        public EnvironmentDataHandler(IBotService botService)
        {
            _botService = botService;
        }

        public async Task<bool> Handle(EnvironmentData request, CancellationToken cancellationToken)
        {
            await _botService.SendText(request.Temperature.ToString());
            return true;
        }
    }
}
