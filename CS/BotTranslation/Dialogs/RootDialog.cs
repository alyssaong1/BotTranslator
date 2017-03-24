using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Text.RegularExpressions;

namespace BotTranslation.Dialogs
{
    [Serializable]
    [LuisModel("LUISAPPMODEL", "LUISAPPKEY")]
    public class RootDialog : LuisDialog<object>
    {

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            
            if (Regex.IsMatch(result.Query, @"\b(hello|hi|hey|how are you)\b", RegexOptions.IgnoreCase))
                await context.PostAsync("你好。(Hello)");
            else
                await context.PostAsync("我不懂你在说什么。(I didn't understand what you said.)");
            context.Wait(this.MessageReceived);
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
            await context.PostAsync("我们的地区是。。。");

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("GetFoodPlaces")]
        public async Task Food(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("There's lots you can eat.");
            await context.PostAsync("有很多可以吃的。");
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
                        await context.PostAsync("那天是周末，所以我们营业时间9-11。");
                    } else {
                        await context.PostAsync("We open from 10-4 on that day.");
                        await context.PostAsync("我们那天营业时间10-4。");
                    }
                } 
            }
            else
            {
                // User did not specify a date
                await context.PostAsync("We are open from 10-4pm on weekdays, and 9-11pm on weekends.");
                await context.PostAsync("我们平日营业时间10-4pm, 周末营业时间9-11pm.");
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
                    await context.PostAsync("大人入门费￥20.");
                }
                else if (Regex.IsMatch(personTypeEntity, @"(child|kid)\w*", RegexOptions.IgnoreCase))
                {
                    await context.PostAsync("Child entrance fee is $10");
                    await context.PostAsync("小孩入门费￥10.");
                }
            }
            else
            {
                // User asked about general entry fee
                await context.PostAsync("Adult entrance fee is $20, kids entrance fee is $10.");
                await context.PostAsync("大人入门费￥20，小孩￥10");
            }

            context.Wait(this.MessageReceived);
        }
    }
}
