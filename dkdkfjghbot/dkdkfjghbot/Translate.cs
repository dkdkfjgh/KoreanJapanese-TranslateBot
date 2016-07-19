using Microsoft.International.Converters;
using RestSharp;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Translator
{
    class Language
    {
        public Language() { }
        public String Source = "";
        public String Japanese = "";
        public String Furigana = "";
        public String Korean = "";

        public String Retranslated = "";
        public String SoruceType = "err";
    }

    class Translator
    {


        private static string NaveridKey = "";//Naver 번역기 API Key
        private static string NaversecretKey = "";//Naver API Secret Key
        private static string YahooKey = "";//Yahoo Japan API Key
        private string r = @"\p{IsHangulJamo}|" + @"\p{IsHangulSyllables}";
        public Language L;
        public Translator()
        {
            L = new Language();
        }
        public Translator(String Source)
        {
            L = new Language();
            L.Source = Source;
        }

        public void Translate()
        {
            if (L.Source == "") return;
            L.Source = Regex.Replace(L.Source, @"\p{Cs}", "");

            switch (WhatLanguage())
            {
                case "ko":
                    L.SoruceType = "ko";
                    L.Korean = L.Source;
                    L.Japanese = Translate("ko", "ja", L.Korean);
                    L.Retranslated = Translate("ja", "ko", L.Japanese);
                    break;
                case "ja":
                    L.SoruceType = "ja";
                    L.Japanese = L.Source;
                    L.Korean = Translate("ko", "ja", L.Japanese);
                    L.Retranslated = Translate("ja", "ko", L.Korean);
                    break;
                case "rmj":
                    L.SoruceType = "rmj";
                    L.Japanese = KanaConverter.RomajiToHiragana(L.Source);
                    L.Korean = Translate("ko", "ja", L.Japanese);
                    L.Retranslated = Translate("ja", "ko", L.Korean);
                    break;
                case "err":
                    L.SoruceType = "err";
                    return;

                default:
                    L.SoruceType = "err";
                    return;

            }
            MakeFurigana(L.Japanese);

        }

        private String WhatLanguage()
        {
            int engcnt = 0;
            int othercnt = 0;
            if (L.Source == "") return "err";
            Match match = Regex.Match(L.Source, r);
            for (int i = 0; i < L.Source.Length; i++)
            {
                char simbolo = L.Source.ElementAt(i);
                simbolo = simbolo.ToString().ToLower()[0];
                if ((simbolo >= 'a') && (simbolo <= 'z'))
                {
                    engcnt++;
                }


                else othercnt++;
            }
            if (engcnt >= othercnt) //if it is romaji input
            {
                return "rmj";
            }
            if (match.Success)
            {
                return "ko";
            }
            else
            {
                return "ja";
            }

        } //Source의 언어를 판별합니다. rmj, ko, ja
        private String Translate(string From, string To, string Source)
        {
            var client = new RestClient("https://openapi.naver.com/v1/language/translate");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("charset", "UTF-8");


            request.AddHeader("X-Naver-Client-Id", NaveridKey);
            request.AddHeader("X-Naver-Client-Secret", NaversecretKey);
            request.AddParameter("application/x-www-form-urlencoded", "source=" + From + "&target=" + To + "&text=" + Source, ParameterType.RequestBody);


            IRestResponse response = client.Execute(request);

            RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            var JSONObj = deserial.Deserialize<Dictionary<string, Dictionary<string, object>>>(response);

            object test = JSONObj["message"]["result"];
            Dictionary<string, object> test2 = (Dictionary<string, object>)test;
            string Rval = (string)test2["translatedText"];
            return Rval;
        } //네이버 번역기 이용 번역을 합니다 From : 언어타입, To : 언어타입, Soruce : String
        private void MakeFurigana(string Str)
        {
            Uri address = new Uri("http://jlp.yahooapis.jp/FuriganaService/V1/furigana");
            String Furiganas = "";
            String Response;
            // Create the web request  
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // Create the data we want to send  
            string appId = YahooKey;
            string context = Str;

            StringBuilder data = new StringBuilder();
            data.Append("appid=" + HttpUtility.UrlEncode(appId));
            data.Append("&sentence=" + HttpUtility.UrlEncode(context));

            // Create a byte array of the data we want to send  
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            // Get response  
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Console application output  
                Response = reader.ReadToEnd();
            }
            //  Console.WriteLine(Response);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Response);

            XmlNodeList xnList = xmlDoc.GetElementsByTagName("Word"); //접근할 노드
            foreach (XmlNode xn in xnList)
            {
                if (xn["Furigana"] == null || xn["Roman"] == null) continue;
                Furiganas += xn["Surface"].InnerText + "(" + xn["Furigana"].InnerText + ", ";
                Furiganas += xn["Roman"].InnerText + ") ";
            }
            Furiganas = Furiganas.Replace("tu", "tsu");
            Furiganas = Furiganas.Replace("ti", "tsi");
            Furiganas = Furiganas.Replace("x", "");

            L.Furigana = Furiganas;
        } //L.Furigana에 발음을 붙여줍니다
    }
}
