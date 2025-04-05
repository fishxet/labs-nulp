using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;

namespace TelegramBotExperiments
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("6481115850:AAEeDJ-_76UJTwn3609YWqlYon3CI-zlfvA");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Пошёл нахуй нищеёб");
                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Слышь, старт то пропиши, я хуй отвечу так-то");
                }
                
                
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
        

        static void Main(string[] argss)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}