using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

using Translator;

namespace dkdkfjghbot
{
    class dkdkfjghbot
    { 
        String Token = ""; //Telegram Bot HTTP Token
        String Version = "1.0.2";
        String user = ""; //Telegram 허용된 User의 ID.
        Telegram.Bot.Api Bot;
        Telegram.Bot.Types.User me;
        Update update;
        public  async Task RunBot()
        {
            Bot = new Telegram.Bot.Api(Token);
            me = await Bot.GetMe();
            Translator.Translator T = null;

            Console.WriteLine("System>{0} is on!", me.Username);

            int offset = 0;

            while (true)
            {
                Update[] updates = await Bot.GetUpdates(offset);

                foreach (Update update in updates)
                {
                    this.update = update;
                    if (update.Message.From.Id.ToString() == user)//Auth OK  
                    {
                        if (update.Message.Type == MessageType.TextMessage)
                        {

                            Console.WriteLine(update.Message.Text);

                            if (update.Message.Text == "/start")
                            {
                                SendMessage("일본어 번역기 봇 ver" + Version + " by 조호연@KMOU");
                                SendMessage("일본어, 한국어, Romaji 지원");

                                offset = update.Id + 1;
                                break;
                            }

                            try
                            {
                                T = new Translator.Translator(update.Message.Text);
                                T.Translate();

                                switch (T.L.SoruceType)
                                {
                                    case "ko":
                                        SendMessage("번역된 말 : " + T.L.Japanese + "\n\n재번역된 말 : " + T.L.Retranslated + "\n\n일본어 발음 : " + T.L.Furigana);
                                        SendMessage(T.L.Japanese);

                                        break;
                                    case "ja":
                                        SendMessage("번역된 말 : " + T.L.Korean + "\n\n재번역된 말 : " + T.L.Retranslated + "\n\n일본어 발음 : " + T.L.Furigana);
                                        break;
                                    case "rmj":
                                        SendMessage(T.L.Japanese + "으로 변경되었음.\n" + "번역된 말 : " + T.L.Korean + "\n\n재번역된 말 : " + T.L.Retranslated + "\n\n일본어 발음 : " + T.L.Furigana);
                                        break;

                                    default:
                                        SendMessage("해석 불가!");
                                        break;
                                }
                                T = null;
                            }
                            catch(Exception e)
                            {
                                SendMessage("Error!");
                                offset = update.Id + 1;
                                continue;
                            }
                            

                        }

                        offset = update.Id + 1;
                    }
                }

                await Task.Delay(500);
            }
        }
        void SendMessage(String Message)
        {
             Bot.SendTextMessage(update.Message.Chat.Id, Message).Wait();
        }
    }
}
