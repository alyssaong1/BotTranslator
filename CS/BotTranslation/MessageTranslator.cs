using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace BotTranslation
{
    public class MessageTranslator
    {
        private static string translationKey = "f6e46aeebeaf466ba1ce00fb87f6591d";
        private static DateTime TokenDate;
        private static string Token;
        private static object Lock = new object();

        // Can hardcode if you know that the language coming in will be chinese/english for sure
        // Otherwise can use the code for locale detection provided here: https://docs.botframework.com/en-us/node/builder/chat/localization/#navtitle
        private static string FromLocale = "zh-CHS"; // Simplified Chinese locale
        private static string ToLocale = "en";

        private MessageTranslator()
        {

        }

        public static MessageTranslator Current = new MessageTranslator();

        public async Task<string> TranslateMessage(string text)
        {
            return await TranslateMessageCore(text, FromLocale, ToLocale);
        }

        public async Task<string> TranslateMessageBack(string text)
        {
            return await TranslateMessageCore(text, ToLocale, FromLocale);
        }


        private async Task<string> TranslateMessageCore(string text, string FromLocale, string ToLocale)
        {
            //The translation token times out after 10 minutes so ensure it's still valid.
            await RefreshToken();

            string urlEncodedText = HttpUtility.UrlEncode(text);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            HttpResponseMessage response = await client.GetAsync("http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + urlEncodedText + "&from=" + FromLocale + "&to=" + ToLocale);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            //return type is XML.
            XmlDocument document = new XmlDocument()
            {
                InnerXml = responseContent
            };
            return document.InnerText;
        }

        private async Task RefreshToken()
        {
            //Ensure we have a token, and it isn't expired.
            if (Token == null || TokenDate.AddMinutes(8) < DateTime.Now)
            {
                Token = await GetTranslationToken();
                TokenDate = DateTime.Now;
            }
        }

        private async Task<string> GetTranslationToken()
        {
            HttpClient client = new HttpClient();

            string tokenURI = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken?subscription-key=" + translationKey;
            HttpResponseMessage response = null;
            response = await client.PostAsync(tokenURI, new HttpRequestMessage().Content);
            response.EnsureSuccessStatusCode();
            return await response?.Content.ReadAsStringAsync();
        }
    }
}