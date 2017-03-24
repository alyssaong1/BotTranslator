using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Xml;
using System.Configuration;

namespace BotTranslation
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private string translationKey = "TRANSLATIONKEY";
        private DateTime TokenDate;
        private string Token;
        
        // Can hardcode if you know that the language coming in will be chinese/english for sure
        // Otherwise can use the code for locale detection provided here: https://docs.botframework.com/en-us/node/builder/chat/localization/#navtitle
        string FROMLOCALE = "zh-CHS"; // Simplified Chinese locale
        string TOLOCALE = "en";

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            
            if (activity.Type == ActivityTypes.Message)
            {
                //perform our translation and update the text with the translated response before passing it to LUIS.
                activity.Text = await TranslateMessage(activity.Text);
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<string> TranslateMessage(string text)
        {
            //The translation token times out after 10 minutes so ensure it's still valid.
            await RefreshToken();

            string urlEncodedText = System.Web.HttpUtility.UrlEncode(text);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",Token);
            HttpResponseMessage response = await client.GetAsync("http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + urlEncodedText + "&from=" + FROMLOCALE + "&to=" + TOLOCALE);
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
            HttpResponseMessage response = await client.PostAsync(tokenURI, new HttpRequestMessage().Content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}