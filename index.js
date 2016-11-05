var restify = require('restify');
var builder = require('botbuilder');
var request = require('request');
var parseString = require('xml2js').parseString;
var urlencode = require('urlencode');

var tokenHandler = require('./tokenHandler');

//=========================================================
// Bot Setup
//=========================================================

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
  
// Create chat bot
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
var bot = new builder.UniversalBot(connector);
server.post('/api/messages', connector.listen());

//=========================================================
// Bot Translation Middleware
//=========================================================

// Start generating tokens needed to use the translator API
tokenHandler.init();

// Can hardcode if you know that the language coming in will be chinese/english for sure
// Otherwise can use the code for locale detection provided here: https://docs.botframework.com/en-us/node/builder/chat/localization/#navtitle
var FROMLOCALE = 'zh-CHS'; // Simplified Chinese locale
var TOLOCALE = 'en';

// Documentation for text translation API here: http://docs.microsofttranslator.com/text-translate.html
bot.use({
    receive: function (event, next) {
        var token = tokenHandler.token();
        if (token && token !== ""){ //not null or empty string
            var urlencodedtext = urlencode(event.text); // convert foreign characters to utf8
            var options = {
                method: 'GET',
                url: 'http://api.microsofttranslator.com/v2/Http.svc/Translate'+'?text=' + urlencodedtext + '&from=' + FROMLOCALE +'&to=' + TOLOCALE,
                headers: {
                    'Authorization': 'Bearer ' + token
                }
            };
            request(options, function (error, response, body){
                //Check for error
                if(error){
                    return console.log('Error:', error);
                } else if(response.statusCode !== 200){
                    return console.log('Invalid Status Code Returned:', response.statusCode);
                } else {
                    // Returns in xml format, no json option :(
                    parseString(body, function (err, result) {
                        console.log(result.string._);
                        event.text = result.string._;
                        next();
                    });
                    
                }
            });
        } else {
            console.log("No token");
            next();
        }
    }
});

//=========================================================
// Bots Dialogs
//=========================================================

var LUISKEY = process.env.LUIS_KEY; // Replace this with your LUIS key as a string
var APPID = process.env.LUIS_APP_ID; // Replace this with your LUIS app id as a string
var recognizer = new builder.LuisRecognizer('https://api.projectoxford.ai/luis/v1/application?id=' + APPID +'&subscription-key=' + LUISKEY);
var intents = new builder.IntentDialog({ recognizers: [recognizer] });
bot.dialog('/', intents);

//Route the luis intents to the various dialogs
intents.matches(/\b(hello|hi|hey|how are you)\b/i, builder.DialogAction.send('你好。(Hello)'))
    .matches('GetHelp', '/help')
    .matches('GetLocation', '/location')
    .matches('GetFoodPlaces', '/food')
    .matches('GetOpeningHours', '/openinghrs')
    .matches('GetPrice', '/entryfee')
    .onDefault(builder.DialogAction.send('我不懂你在说什么。(I didn\'t understand what you said.)'));

bot.dialog('/help', function (session) {
    //Send the message back in the foreign dialect to avoid translating back
    session.endDialog('Help stuff goes here');
});

bot.dialog('/location', function (session) {
    session.send('Our address is...');
    session.endDialog('我们的地区是。。。'); 
});

bot.dialog('/food', function (session){
    session.send("There's lots you can eat.");
    session.endDialog('有很多可以吃的。');
});

bot.dialog('/openinghrs', [
    function (session, args) {
        // Resolve and store any entities passed from LUIS.
        var dateEntity = builder.EntityRecognizer.findEntity(args.entities, 'builtin.datetime.date');
        if (dateEntity){
            // User specified a date
            var date = new Date(dateEntity.resolution.date);
            // Year parsing is inconsistent so we set the year ourselves if the year parsed is less than the current year (we assume the user wouldn't ask about opening hours of previous years)
            var now = new Date();
            if (date.getFullYear() < now.getFullYear()){
                if (date.getMonth() >= now.getMonth() && date.getDate() >= now.getDate()){
                    // Day/month is either today or later in the year
                    date.setFullYear(now.getFullYear());
                } else {
                    // The day/month is less than the current day/month, meaning user is likely referring to next year
                    date.setFullYear(now.getFullYear() + 1);
                }
            }
            // Check if weekend or weekday
            if (date.getDay() == 6 || date.getDay() == 0){
                session.send('That day is a weekend, so we are open 9-11pm.');
                session.endDialog('那天是周末，所以我们营业时间9-11。');
            } else {
                session.send('We open from 10-4 on that day.');
                session.endDialog('我们那天营业时间10-4。');
            }
        } else {
            // User did not specify a date
            session.send('We open are from 10-4pm on weekdays, and 9-11pm on weekends.');
            session.endDialog('我们平日营业时间10-4pm, 周末营业时间9-11pm.');
        }
    }
]);

bot.dialog('/entryfee', [
    function (session, args){
        // Resolve and store any entities passed from LUIS.
        var personType = builder.EntityRecognizer.findEntity(args.entities, 'PersonType');
        if (personType){
            // User stated child or adult (haven't accounted for both)
            personType = personType.entity;
            if (personType.match(/\b(adult)\w*/i)){
                session.send('Adult entrance fee is $20');
                session.endDialog('大人入门费￥20.');
            } else if (personType.match(/\b(child|kid)\w*/i)){
                session.send('Child entrance fee is $10');
                session.endDialog('小孩入门费￥10.');
            }
        } else {
            // User asked about general entry fee
            session.send('Adult entrance fee is $20, kids entrance fee is $10.');
            session.endDialog('大人入门费￥20，小孩￥10');
        }
    }
]);