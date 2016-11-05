//=============================
// Cognitive API Token Setup 
//=============================

var request = require('request');

var token = "";
var tokeninterval;
var TRANSLATIONKEY = process.env.TRANSLATION_KEY; // Replace this with your translation key as a string

// Naive implementation. Token generation should probably happen separate from the bot web service.
// Why I used setTimeout and not setInterval: http://www.thecodeship.com/web-development/alternative-to-javascript-evil-setinterval/
function getToken() {
    // do shit here
    var options = {
        method: 'POST',
        url: 'https://api.cognitive.microsoft.com/sts/v1.0/issueToken?subscription-key=' + TRANSLATIONKEY
    };

    request(options, function (error, response, body){
        //Check for error
        if(error){
            return console.log('Error:', error);
        } else if(response.statusCode !== 200){
            return console.log('Invalid Status Code Returned:', response.statusCode);
        } else {
            //Token gets returned as string in the body
            token = body;
        }
    });

    interval = setTimeout(getToken, 540000); // runs once every 9 minutes, token lasts for 10
}

// Stop the token generation
function stopInterval() {
    clearTimeout(tokeninterval);
}

module.exports = {
  init: function() {
      getToken();
  }, 
  stop: function() {
      stopInterval();
  },
  token: function () {
      return token;
  }
};