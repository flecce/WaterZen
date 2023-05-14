using Microsoft.Extensions.Configuration;
using System.Collections;
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

            using (Stream tempStream = new MemoryStream(ChartHelper.CreateGraph(session.Temperatures.Select(x => x.Item2).ToArray())))
            using (Stream flowRateStream = new MemoryStream(ChartHelper.CreateGraph(session.FlowRates.Select(x => x.Item2).ToArray())))
            {
                foreach (var chatId in _chatIds)
                {
                    await _botClient.SendTextMessageAsync(chatId, GetMessage(session), parseMode: ParseMode.Markdown);
                    
                    await _botClient.SendPhotoAsync(chatId, InputFile.FromStream(tempStream));
                    await _botClient.SendPhotoAsync(chatId, InputFile.FromStream(flowRateStream));
                }
            }
        }

        private string GetMessage(ShowerSession session)
        {
            return $"" +
                $"- Hai consumato {session.FlowRates.Sum(x => x.Item2).ToString()} litri d'acqua" +
                $"- Hai emesso {(session.EndDate - session.StartDate).TotalMinutes * 50} grammi di CO2" +
                $"- Ti è costato {((session.EndDate - session.StartDate).TotalMinutes) * 0.0025} euro";
        }
    }
}
