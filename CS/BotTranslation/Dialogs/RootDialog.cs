using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.FormFlow;

namespace BotTranslation.Dialogs
{
    [Serializable]
    [LuisModel("c507d394-f79f-4887-86c0-52de8e36c712", "f60cc073752e4c2b8265ce4cf31d9ed2")]
    public partial class RootDialog : LuisDialog<object>
    {

        //This method gets called before the text is sent to LUIS, so we can translate our source message here.
        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            IMessageActivity activity = await item;
            activity.Text = await MessageTranslator.Current.TranslateMessage(activity.Text);

            //Create a Translating Context to translate any messages that gets sent subsequently.
            await base.MessageReceived(new TranslatingContext(context), item);
        }

        [Serializable]
        public class FormOrder
        {
            public string OrderName { get; set; }
            public static IForm<FormOrder> BuildForm()
            {
                return new FormBuilder<FormOrder>().Message("Order Form").Build();
            }
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            if (result.Query.ToLower().Contains("test form"))
            {
                await context.Forward(new TranslatingFormDialog<FormOrder>(FormDialog.FromForm(FormOrder.BuildForm, FormOptions.PromptFieldsWithValues)),Resume, context.Activity, new System.Threading.CancellationToken());
                return;
            }
            
            

            if (Regex.IsMatch(result.Query, @"\b(hello|hi|hey|how are you)\b", RegexOptions.IgnoreCase))
                await context.PostAsync("Hello");
            else
                await context.PostAsync("I didn't understand what you said.");
            context.Wait(this.MessageReceived);
        }

        private async Task Resume(IDialogContext context, IAwaitable<FormOrder> result)
        {
            await context.PostAsync("Done!");
        }

        [LuisIntent("GetHelp")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Help stuff goes here");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetLocation")]
        public async Task Location(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Our address is...");
            
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetFoodPlaces")]
        public async Task Food(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("There's lots you can eat.");
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetOpeningHours")]
        public async Task OpeningHours(IDialogContext context, LuisResult result)
        {

            // Resolve and store any entities passed from LUIS.
            if (result.TryFindEntity("builtin.datetime.date", out EntityRecommendation dateEntity))
            {
                // User specified a date
                if (DateTime.TryParse(dateEntity.Resolution["date"], out DateTime date))
                {
                    // Year parsing is inconsistent so we set the year ourselves if the year parsed is less than the current year (we assume the user wouldn't ask about opening hours of previous years)
                    var now = DateTime.Now;

                    if (date.Year < now.Year) {
                        if (date.Month >= now.Month && date >= now) {
                            // Day/month is either today or later in the year
                            date = new DateTime(now.Year, date.Month, date.Day);
                        } else {
                            // The day/month is less than the current day/month, meaning user is likely referring to next year
                            date = new DateTime(now.Year + 1, date.Month, date.Day);
                        }
                    }

                    // Check if weekend or weekday
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) {
                        await context.PostAsync("That day is a weekend, so we are open 9-11pm.");
                    } else {
                        await context.PostAsync("We open from 10-4 on that day.");
                    }
                } 
            }
            else
            {
                // User did not specify a date
                await context.PostAsync("We are open from 10-4pm on weekdays, and 9-11pm on weekends.");
            }
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetPrice")]
        public async Task EntryFee(IDialogContext context, LuisResult result)
        {
            // Resolve and store any entities passed from LUIS.
            if (result.TryFindEntity("PersonType", out EntityRecommendation personType))
            {
                // User stated child or adult (haven't accounted for both)
                string personTypeEntity = personType.Entity;
                if (Regex.IsMatch(personTypeEntity, @"(adult)\w*", RegexOptions.IgnoreCase))
                {
                    await context.PostAsync("Adult entrance fee is $20");
                }
                else if (Regex.IsMatch(personTypeEntity, @"(child|kid)\w*", RegexOptions.IgnoreCase))
                {
                    await context.PostAsync("Child entrance fee is $10");
                }
            }
            else
            {
                // User asked about general entry fee
                await context.PostAsync("Adult entrance fee is $20, kids entrance fee is $10.");
            }

            context.Wait(this.MessageReceived);
        }
    }
}
