using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using Newtonsoft.Json;

namespace telegramBot
{
    internal class Program
    {
        private static string token = "5361129191:AAHjGPTKHedN0G6k94JJr95jFh49QRmBSJg";
        private static TelegramBotClient botClient = new TelegramBotClient(token);

        private static void ReplyMarkup(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            Tickets.routeBack = message.Text;

            async void RouteChoice(string choice)
            {
                await botClient.SendTextMessageAsync(message.Chat, choice,
                    replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
                Tickets.routeBack = message.Text;
            }

            if (message.Text == "Да")
            {
                RouteChoice("Введите дату вылета и прилета через дефис (01.01.2022-10.02.2022)");
            }
            else if (message.Text == "Нет")
            {
                RouteChoice("Введите дату вылета");
            }
        }

        private static async void GetRouteDate(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            Tickets.routeDate = message.Text;

            await botClient.SendTextMessageAsync(message.Chat, text: "Олично, запрос обрабатывается!", cancellationToken: cancellationToken);
            Tickets tickets = new Tickets(@"C:\Users\emmag\source\repos\telegramBot\cities.json");

            TicketsData[] result = TicketsData.FromJson(tickets.ResponseData["data"].ToString());
            Console.WriteLine(result.Length);

            foreach (TicketsData data in result)
            {
                Console.WriteLine(data.ReturnAt);
                string htmlText = $"<a>{Tickets.route}</a>\n" +
                            $"<a>Туда: {data.DepartureAt.ToString("dd.MM.yyyy")}</a>\n" +
                            $"<a>Обратно: {(data.ReturnAt.Year < DateTime.Now.Year ? "-" : data.ReturnAt.ToString("dd.MM.yyyy"))}</a>\n" +
                            $"<a>Цена: {data.Price} руб.</a>\n" +
                            $"<a href=\"https://www.aviasales.ru/{data.Link}\">Источник</a>\n";

                await botClient.SendTextMessageAsync(chatId: message.Chat, text: htmlText, parseMode: ParseMode.Html);
            }

        }

        private static async void GetRoute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            Tickets.route = message.Text.Trim();

            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new[] { new KeyboardButton[]{"Да", "Нет"} }) { ResizeKeyboard = true };

            await botClient.SendTextMessageAsync(message.Chat, "Маршрут туда и обратно?",
                replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
            Tickets.route = message.Text;
        }
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                Message message = update.Message;

                if (message.Text == "/start")
                {

                    Tickets.route = null;
                    Tickets.routeDate = null;
                    Tickets.routeBack = null;

                    await botClient.SendTextMessageAsync(message.Chat, "Привет! Я твой помошник для поиска дешевых авиабилетов!" +
                                                                        "Введите маршрут через пробел(Москва/Владивосток)");
                    return;
                }

                if (Tickets.route == null)
                {
                    GetRoute(botClient, message, cancellationToken);
                    return;
                }

                if (Tickets.routeBack == null)
                {
                    ReplyMarkup(botClient, message, cancellationToken);
                    return;
                }

                if (Tickets.routeDate == null)
                {
                    GetRouteDate(botClient, message, cancellationToken);
                    return;
                }

                Console.WriteLine(Tickets.routeBack);
                Console.WriteLine(Tickets.route);
            }

        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Бот запущен: {botClient.GetMeAsync().Result.FirstName}");

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.ReadLine();
        }
    }
}
