using AngleSharp.Dom;
using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace MyBot
{
    /// <summary>
    /// Класс телеграм бота
    /// </summary>
    class Bot
    {
        private Dictionary<long, List<string>> users; //хранит списки с мемами отдельно для каждого пользователя
        public TelegramBotClient BotClient { get; }
        public CancellationTokenSource Cts { get; }
        public ReceiverOptions ReceiverOptions { get; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="BotClient"></param>
        /// <param name="Cts"></param>
        /// <param name="receiverOptions"></param>
        public Bot(TelegramBotClient BotClient, CancellationTokenSource Cts, ReceiverOptions receiverOptions)
        {
            this.users = new Dictionary<long, List<string>>();
            this.BotClient = BotClient;
            this.Cts = Cts;
            this.ReceiverOptions = receiverOptions;
        }
        public void Start() => BotClient.StartReceiving(updateHandler, ErrorHandler, ReceiverOptions, Cts.Token);

        /// <summary>
        /// Парсит поданные сайты и записывает все найденные ссылки в словарь 
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        private async Task Parsing(string[] urls, long key)
        {
            var config = Configuration.Default.WithDefaultLoader();
            using var context = BrowsingContext.New(config);

            var docs = new List<Task<IDocument>>();
            
            //Получение кода всех страниц и запись ссылок в список
            foreach (var url in urls)
            {
                using var doc = await context.OpenAsync(url);
                var mems = doc.Images.Select(el => el.GetAttribute("src"));

                if (!users.ContainsKey(key))
                    users.Add(key, mems.ToList());
                else 
                    users[key].AddRange(mems);
            }   
        }
        /// <summary>
        /// Создаёт инлайн клавиатуру с 2 кнопками
        /// </summary>
        /// <param name="txt1"></param>
        /// <param name="txt2"></param>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns></returns>
        private InlineKeyboardMarkup CreateInline(string txt1, string txt2, string data1, string data2)
        {
            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>
            {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData(txt1, data1),
                        InlineKeyboardButton.WithCallbackData(txt2, data2)
                    }
            });

        }

        /// <summary>
        /// Обработка возникших исключений
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken token)
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
        private async Task updateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            try
            {
                switch (update.Type) //Тип полученного update'а
                {
                    case UpdateType.Message: //Если сообщение
                        {

                            var message = update.Message;
                            var ID = message.Chat.Id;
   
                            if ( message.Type.Equals(MessageType.Text) && message.Text.ToLower().Equals(@"/start"))
                            {
                                if (users.ContainsKey(ID))
                                    users[ID].Clear();

                                var inlineKeyboard = new InlineKeyboardMarkup(
                                        InlineKeyboardButton.WithCallbackData("Пройти тест", "button1"));

                                await botClient.SendTextMessageAsync(ID, "МемоLove приветствует тебя, заблудший сранник!\n\n" +
                                    "Моё призвание - поиск годных мемов со всех уголков света.\n Я поделюсь с тобой результатами своего неустанного труда, но вначале ты должен помочь мне:\n\n" +
                                    "Расскажи мне какие мемы тебе нравятся и я смогу подобрать для тебя подходящие:", replyMarkup: inlineKeyboard);
                            }
                            else //Ответ на случайное сообщение 
                            {
                                var difrentPhotos = new string[] { "https://a.d-cd.net/8abae02s-960.jpg",
                                                "https://i.pinimg.com/736x/18/7c/0e/187c0edc4c62719e313b6e57d6fca48a.jpg",
                                                "https://www.meme-arsenal.com/memes/e95300acce058bfa3ee18d8e41999ba2.jpg",
                                                "https://i.pinimg.com/564x/8b/2a/34/8b2a34739089fe01481e098a4eb0d517.jpg",
                                                "https://memchik.ru//images/memes/6048acaeb1c7e30db84acd75.jpg",
                                                "https://memchik.ru//images/memes/5f874b60b1c7e36130153ce0.jpg"};
                                Random r = new Random();
                                await botClient.SendPhotoAsync(ID, InputFile.FromUri(difrentPhotos[r.Next(0, difrentPhotos.Length - 1)]), caption: "Даже не знаю, что на это сказать...");
                            }

                            break;
                        }
                    case UpdateType.CallbackQuery: //Если нажатие на кнопку
                        {
                            var callbackQuery = update.CallbackQuery; //информация о кнопке
                            var chat = callbackQuery.Message.Chat; //информация о чате

                            var call = callbackQuery.Data;

                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                            if (call.Equals("button1")) // Вопрос теста 1: Коты
                            {
                                var inline = CreateInline("ДА!", "НЕТ!", "button2:Yes", "button2:No");

                                await botClient.SendPhotoAsync(chat.Id, InputFile.FromUri("https://cs12.pikabu.ru/post_img/big/2022/04/11/12/1649708608135887396.jpg"), caption: "Нравятся ли тебе мемы с котами? (Признавайся, они всем нравятся)", replyMarkup: inline);
                            }
                            else if (call.StartsWith("button2")) //Вопрос теста 2: Иисус
                            {
                                var urls = new string[] { "https://ru.pinterest.com/ane4kaklimova7/%D0%BA%D0%BE%D1%88%D0%B0%D1%87%D1%8C%D0%B8-%D0%BC%D0%B5%D0%BC%D1%8B/", "https://ru.pinterest.com/WhateverCouldItMean/%D0%BC%D0%B5%D0%BC%D1%8B-%D1%81-%D0%BA%D0%BE%D1%82%D0%B0%D0%BC%D0%B8/", }; //мемы с котами

                                var inline = CreateInline("да ;)", "Это богохульство!", "button3:Yes", "button3:No");

                                await botClient.SendPhotoAsync(chat.Id, InputFile.FromUri("https://i.pinimg.com/236x/94/9f/e0/949fe03d837b2cec05040f81056eec0a.jpg"), caption: "Что думаешь на счёт... христианских мемов? (Нажимая 'да' ты выбираешь ад!" +
                                            "\nМемоLove не несёт ответственности за испорченную карму и оскорбления чувств верующих)", replyMarkup: inline);

                                if (call.Equals("button2:Yes"))
                                    Parsing(urls, chat.Id);
                            }
                            else if (call.StartsWith("button3")) //Вопрос теста 3: Чёрный юмор
                            {
                                var urls = new string[] { "https://ru.pinterest.com/lnmyaso/%D0%B8%D0%B8%D1%81%D1%83%D1%81/", "https://ru.pinterest.com/nyatinamari/%D1%85%D1%80%D0%B8%D1%81%D1%82%D0%B8%D0%B0%D0%BD%D1%81%D0%BA%D0%B8%D0%B5-%D0%BC%D0%B5%D0%BC%D1%8B/", "https://ru.pinterest.com/polinahergiani413/%D1%85%D1%80%D0%B8%D1%81%D1%82%D0%B8%D0%B0%D0%BD%D1%81%D0%BA%D0%B8%D0%B5-%D0%BC%D0%B5%D0%BC%D1%8B/" }; //христианские мемы (не для религиозных)

                                var inline = CreateInline("Yesss", "Нет ^~^", "button4:Yes", "button4:No");

                                await botClient.SendPhotoAsync(chat.Id, InputFile.FromUri("https://i.pinimg.com/564x/ef/e2/9f/efe29f7bbd804044ef45b20b9d4bb350.jpg"), caption: "Чёрный(не толерантный) юмор, юмор для ценителей. Присоеденишься к этой братии?", replyMarkup: inline);

                                if (call.Equals("button3:Yes"))
                                    Parsing(urls, chat.Id);
                            }
                            else if (call.StartsWith("button4")) //Вопрос теста 4: Волки
                            {
                                var urls = new string[] { "https://ru.pinterest.com/kostpotapow5/%D1%87%D1%91%D1%80%D0%BD%D1%8B%D0%B9-%D1%8E%D0%BC%D0%BE%D1%80/", "https://ru.pinterest.com/iceescp/%D1%87%D0%B5%D1%80%D0%BD%D1%8B%D0%B9-%D1%8E%D0%BC%D0%BE%D1%80/" }; //чёрный юмор

                                var inline = CreateInline("ДА!!!", "НЕТ!", "button5:Yes", "button5:No");

                                await botClient.SendPhotoAsync(chat.Id, InputFile.FromUri("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRWKqv8tCEIlk1jThK_2ZQrbJQQDVFWSxx9aGipsOMuIg&s"), caption: "И следующий вопрос теста: Мемы с волками. Поймет не каждый, но тот, кто поймёт, тот поймёт. Что скажешь?", replyMarkup: inline);

                                if (call.Equals("button4:Yes"))
                                    Parsing(urls, chat.Id);
                            }
                            else if (call.StartsWith("button5")) //Вопрос теста 5: Общая тематика
                            {
                                var urls = new string[] { "https://ru.pinterest.com/Alek_Ext/%D0%BC%D0%B5%D0%BC%D1%8B-%D1%81-%D0%B2%D0%BE%D0%BB%D0%BA%D0%B0%D0%BC%D0%B8/", "https://ru.pinterest.com/vladick2007/%D0%B2%D0%BE%D0%BB%D0%BA%D0%B8/" }; //мемы с волками

                                var inline = CreateInline("ХОЧУ!", "Не интересно `^`", "button6:Yes", "button6:No");

                                await botClient.SendPhotoAsync(chat.Id, InputFile.FromUri("https://avatars.dzeninfra.ru/get-zen_doc/3986532/pub_6066c775dc5df570b1b2f982_6066d5436d990144ce322557/scale_1200"), caption: "Всё многообразие мемов не охватить несколькими темами. Если не одна из предыдущих тебе не по вкусу могу предложить:\n" +
                                    "мемы с животными, мемы про политику (осуждаю), исторические мемы, мемы с постами из твитера, классические мемы, старые мемы, новые мемы, русские мемы и иностранные мемы - мемы на любой вкус и цвет.\n" +
                                    "Ты же не откажишься от того, что человечесто накапливало десятилетиями?", replyMarkup: inline);

                                if (call.Equals("button5:Yes"))
                                    Parsing(urls, chat.Id);
                            }
                            else if (call.StartsWith("button6")) //Переход к отправке мемов
                            {
                                var urls = new string[] { "https://ru.pinterest.com/alinkafr20/%D1%81%D0%BC%D0%B5%D1%88%D0%BD%D1%8B%D0%B5-%D0%BC%D0%B5%D0%BC%D1%8B/", "https://ru.pinterest.com/lengavriuck/meme/", "https://ru.pinterest.com/armyprishvina/%D0%BC%D0%B5%D0%BC%D1%8B/", "https://ru.pinterest.com/karamelllca223/%D0%BC%D0%B5%D0%BC%D1%8B/" }; //общая тематика

                                var inline = new InlineKeyboardMarkup(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Хочу мем!") });
                                var inline1 = new InlineKeyboardMarkup(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Хочу пройти тест заново!", "button1") });

                                if (call.Equals("button6:Yes"))
                                    Parsing(urls, chat.Id);

                                //Проверка, что хотя бы один раз в тесте пользователь нажал да
                                if (users.ContainsKey(chat.Id))
                                    await botClient.SendTextMessageAsync(chat.Id, "Вот и всё! Тест пройден, неправильных ответов в нем нет. Теперь я смогу слать тебе только те мемы, которые тебе нравиться. Начнём?", replyMarkup: inline);
                                else
                                    await botClient.SendTextMessageAsync(chat.Id, "МемоLove сожелеет, что не смог заинтересовать тебя не одной из предложенных категорий. Уверен, что не хочешь взглянуть на них еще раз?", replyMarkup: inline1);
                            }
                            else //Отправка мемов
                            {
                                var inline = new InlineKeyboardMarkup(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Хочу ещё мем!") });

                                Random r = new Random();
                                var len = users[chat.Id].Count();

                                string mem = users[chat.Id][r.Next(0, len - 1)];

                                await botClient.SendPhotoAsync(chat.Id, InputFile.FromUri(mem), replyMarkup: inline);
                            }

                            break;
                        }
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex); }
        }
    }
}

