using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WaterZen.Telegram.Application.Helpers;
using WaterZen.Telegram.Application.Services.Interfaces;

namespace WaterZen.Telegram.Application.Services.Impl
{
    internal class TelegramBotService : IBotService
    {
        private IConfiguration _configuration;
        private TelegramBotClient? _botClient;
        private long[] _chatIds;

        public TelegramBotService(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _configuration = configuration;
            _chatIds = configuration.GetValue<string>("Telegram:ChatId")?.Split(",").Select(x => Convert.ToInt64(x.Trim())).ToArray();
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            _initClient();

            if (_botClient == null)
            {
                throw new Exception("BotClient cannot be null");
            }

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );

            var me = await _botClient.GetMeAsync();
            Console.WriteLine(me.FirstName);
        }

        private void _initClient()
        {
            var accessToken = _configuration.GetValue<string>("Telegram:AccessToken") ?? throw new ArgumentNullException("Telegram:AccessToken");
            _botClient = new TelegramBotClient(accessToken);
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            if (messageText.StartsWith("/mychatid"))
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Your chat ID:\n" + chatId,
                    cancellationToken: cancellationToken);
            }
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public async Task SendMessage(ShowerSession session)
        {
            if (_botClient == null)
            {
                return;
            }

            using (Stream tempStream = new MemoryStream(ChartHelper.CreateGraph(session.Temperatures.Select(x => x.Item2).ToArray(), "Temperature")))
            using (Stream flowRateStream = new MemoryStream(ChartHelper.CreateGraph(session.FlowRates.Select(x => x.Item2).ToArray(), "Flow Rate")))
            {
                foreach (var chatId in _chatIds)
                {
                    await _botClient.SendTextMessageAsync(
                        chatId,
                        GetMessage(session),
                        parseMode: ParseMode.Markdown);

                    await _botClient.SendPhotoAsync(chatId, InputFile.FromStream(tempStream));
                    await _botClient.SendPhotoAsync(chatId, InputFile.FromStream(flowRateStream));

                    tempStream.Seek(0, SeekOrigin.Begin);
                    flowRateStream.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        private string GetMessage(ShowerSession session)
        {
            var dollar = char.ConvertFromUtf32(0x1F4B8);
            var candy = char.ConvertFromUtf32(0x1F36C);
            var water = char.ConvertFromUtf32(0x1F6B0);
            var co2 = char.ConvertFromUtf32(0x1F4A8);

            return $"" +
                $"La tua doccia è durata {TimeSpan.FromMinutes((session.EndDate - session.StartDate).TotalMinutes).ToString(@"hh\:mm\:ss")} minuti:\r\n" +
                $"- {water} Hai consumato {(int)session.FlowRates.Sum(x => x.Item2)} litri d'acqua\r\n" +
                $"- {co2} Hai emesso {(int)(session.EndDate - session.StartDate).TotalSeconds} grammi di CO2\r\n" +
                $"- {candy} La doccia ti è costata {Math.Round(((session.EndDate - session.StartDate).TotalSeconds) * 0.0001 / 0.10, 1)} goleador";
                //$"- {dollar} Ti è costato {Math.Round(((session.EndDate - session.StartDate).TotalMinutes) * 0.0025, 4)} euro\r\n";
        }
    }
}
