# Language translator for Microsoft Bot Framework
This code sample uses Microsoft's Translator API to translate the user's utterance into any language you want, and then pass it into LUIS for natural language processing (now available in both Nodejs and C#). The reason for doing this is that LUIS currently does not support all languages, so a workaround is to convert the user's utterance into a supported language, then feed that into LUIS to determine the user's intent. This sample converts simplified chinese into english using the bot middleware.

### [DEMO](http://bottranslationdemo.azurewebsites.net/) - if you know simplified chinese you can try it out

The bot replies in English as well just for reference for non-chinese speakers. 

### What can this sample bot do?

The bot is a generic customer service bot. You can ask for the following in Chinese:

- Get help menu
- Ask if there's food
- Business location
- Business hours on certain days (this uses entity detection)
- Ask for entrance fee (uses entity detection too)

Please do note that LUIS's builtin datetime entity is a bit inconsistent at the moment (refer to https://github.com/Microsoft/BotBuilder/issues/1173).

![demoimg](http://i.imgur.com/JuTwlvi.png)

## Usage

### 1. Obtain a translation key
You will need an Azure subscription for this. If you do not have one, sign up for a free account [here](https://azure.microsoft.com). Note that you'll get advanced tier for a month, and then after that it'll go back to free tier. Free tier is enough for this tutorial. 

Once you've done that, go to the [Azure portal](https://portal.azure.com) and click on New. In the search bar at the top, type 'Cognitive Services' and hit enter. 

![azurenew](http://i.imgur.com/KyMbdK8.png)

You should see Cognitive Services APIs. Click on it and then click create. 

![searchres](http://i.imgur.com/8gvOp5I.png)

Enter any account name you'd like, use your free subscription. Click on API Type and look for Translator Text API, which is the one you'll be using. Select 'Free' in Pricing tier. Name your resource group something sensible, and select the resource group location closest to where you're located. Then click create.

![cognitivetype](http://i.imgur.com/fTVktm7.png)

Wait a little while for it to get set up and deployed. Once it's done, click into 'All resources' blade down the left, and click into the Cognitive Service account you just created. Click into 'keys' and you'll see this:

![luiskeys](http://i.imgur.com/NpvgyXT.png)

Key 1 and 2 are both valid keys. Copy key 1 and paste it into the var TRANSLATIONKEY field in our code in the tokenHandler.js file. 

### 2. Obtain a LUIS app ID and key

First of all, download the source code as a zip file and extract the zip file. Go to the [LUIS Homepage](https://www.luis.ai) and sign into your Microsoft Account. Click on 'new app', then click on 'Import Existing Application'.

![luisimport](http://i.imgur.com/QL6BPyS.png)

Click on Choose File, and in your file explorer look for the ServiceBot.json file in the source code that you just downloaded. Select this json file, give your App a name, then click on Import. This will import the preexisting LUIS model that I have already built, which you can just plug into your bot and start using. 

Click on App Settings near the top left hand corner, then go into index.js and copy and paste it into the var APPID declaration. Go back to your LUIS portal and click on Publish, then publish your application. Once this is done, copy the subscription key (which you will see in the URL) and paste it in the var LUISKEY declaration in index.js.

![luispage](http://i.imgur.com/4b2iucX.png)

### 3. Restore node packages

You're almost there my friend! Go to your command prompt, navigate to the folder the bot is located at, and run `npm install` (I assume you already have [npm](https://npmjs.com) installed). It may take a while for all the packages to be downloaded. When that's done,  you're all set to go. Run `node index.js` in the command prompt. Open up the Bot Framework Emulator, and start talking to your bot in Chinese. If you want to change the to/from translation language, refer to the [locales available](https://msdn.microsoft.com/en-us/library/hh456380.aspx), and change the TOLOCALE and FROMLOCALE variables in index.js.

## Limitations
The current code assumes that the user will type in either English or Simplified Chinese. If you want to autodetect what language the user is speaking in and translate that, please refer to the [following guide](https://docs.botframework.com/en-us/node/builder/chat/localization/#navtitle) for adding automatic locale detection in the middleware.

## Final tips
Translating from another language is not always reliable, but as long as the gist of the sentence is there, LUIS should be able to pick it up. One thing you could do is put in some common phrases in the foreign language through the translator using [Bing translator](https://www.bing.com/translator), observe what it translates into, then train LUIS based on the translations. 

Feel free to suggest improvements and contribute. Drop me an email at ongalyssa@outlook.com or tweet me @alyssaong1337 if you've got any questions. 
