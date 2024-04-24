using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyTGBot
{
    class Program
    {
        private static ITelegramBotClient botClient;
        private static ReceiverOptions receiverOptions;
        static void Main() 
        {
            var botClient = new TelegramBotClient("7134898383:AAEgZbfY1mfCne8gFiSO6Ha1jdt_LeZaod0");
            
            using var cts = new CancellationTokenSource();

            receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message,
                                         UpdateType.CallbackQuery }//Обрабатывает нажатие на кнопки}
            };
            botClient.StartReceiving(updateHandler, ErrorHandler, receiverOptions, cts.Token);


            Console.ReadLine();
        }

        /// <summary>
        /// Обработка возникших исключений
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static async Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Обработка сообщений от пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task updateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            try
            {
                switch (update.Type) //Тип полученного update'а
                {   
                    case UpdateType.Message: //Если сообщение
                    {
                        
                            var message = update.Message;
                            var ID = message.Chat.Id;

                        
                            switch (message.Type) //Обработка полученных сообщений
                            {

                            
                                case MessageType.Text:
                                {
                                
                                        if (message.Text.Equals(@"/start"))
                                        {   
                                            var inlineKeyboard = new InlineKeyboardMarkup(
                                                new List<InlineKeyboardButton[]>() {
                                                new InlineKeyboardButton[] {
                                                    InlineKeyboardButton.WithCallbackData("Пройти тест", "button1") },
                                                new InlineKeyboardButton[] {
                                                    InlineKeyboardButton.WithCallbackData("Настроить самостаятельно", "buttun2")}});

                                            await botClient.SendTextMessageAsync(ID, "МемоLove приветствует тебя, заблудший сранник!\n\n" +
                                                "Моё призвание - поиск годных мемов со всех уголков света.\n Я поделюсь с тобой результатами своего неустанного труда, но вначале ты должен помочь мне:\n\n" +
                                                "Расскажи мне какие мемы тебе нравятся и я смогу подобрать для тебя подходящие:", replyMarkup: inlineKeyboard);
                                        }

                                        break;
                                }
                            }

                            break;   
                    }
                    case UpdateType.CallbackQuery: //Если нажатие на кнопку
                    {
                            var callbackQuery = update.CallbackQuery;
                            var chat = callbackQuery.Message.Chat;

                            switch (callbackQuery.Data)
                            {
                                case "buttun1":
                                    {
                                        break;
                                    }
                                case "buttun2":
                                    {
                                        break;
                                    }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex) 
            { Console.WriteLine(ex.Message); }
        }
    }

}