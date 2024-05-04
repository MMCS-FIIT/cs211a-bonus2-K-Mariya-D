using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using AngleSharp;
using AngleSharp.Dom;
using static System.Net.WebRequestMethods;
using System.Net.WebSockets;
using MyBot;

namespace MyTGBot
{
    class Program
    {
        static void Main() 
        {
            var botClient = new TelegramBotClient("7134898383:AAEgZbfY1mfCne8gFiSO6Ha1jdt_LeZaod0");
            
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message,
                                         UpdateType.CallbackQuery }//Обрабатывает нажатие на кнопки
            };

            var bot = new Bot(botClient, cts, receiverOptions);
            
            bot.Start();

            Console.ReadLine();
        }
    }

}