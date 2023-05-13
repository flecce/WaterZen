using MediatR;
using WaterZen.Telegram.Application.Services.Interfaces;

namespace WaterZen.Telegram.Application.Mediator.Handler
{
    internal class EnvironmentDataHandler : IRequestHandler<ShowerSession, bool>
    {
        private readonly IBotService _botService;

        public EnvironmentDataHandler(IBotService botService)
        {
            _botService = botService;
        }

        public async Task<bool> Handle(ShowerSession request, CancellationToken cancellationToken)
        {
            await _botService.SendMessage(request);

            return true;
        }
    }
}
