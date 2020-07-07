// Bring in required dependencies
var request = require("request");

// **** ENTER YOUR DETAILS HERE ****

// Enter your Engagement Cloud API user details
const API_USERNAME = 'YOUR API USERNAME';
const API_PASSWORD = 'YOUR API PASSWORD';

// Enter your mobile number in international format here e.g. for the UK 447123123123
var yourMobileNumber = 'YOUR_MOBILE_NUMBER';

console.log('');
console.log('Sending SMS using Engagement Cloud CPaaS and NodeJS');
console.log('---------------------------------------------------');

// Setup Engagement Cloud CPaaS request JSON
var myRequest = {
    body: 'Your SMS message',
    to: { phoneNumber: yourMobileNumber },
    rules: ['sms']
};

// Call Engagement Cloud CPaaS "One" API
var options = {
    method: 'POST',
    url: "https://api-cpaas.dotdigital.com/cpaas/messages",
    headers:
    {
        'cache-control': 'no-cache',
        'content-type': 'application/json',
        'accept': 'application/json',
        'authorization': "Basic " + new Buffer(API_USERNAME + ":" + API_PASSWORD).toString("base64")
    },
    body: myRequest,
    json: true
};

// Send the request
console.log('');
console.log('Calling Engagement Cloud CPaaS...');

request(options, function (error, response, body) {
    if (error) throw new Error(error);

    console.log("HTTP status code returned: " + response.statusCode);

    // Check status
    if (response.statusCode == 201)
    {
        // All ok
        console.log('SMS message successfully sent via Engagement Cloud CPaaS "One" API');
    }
    else
    {
        // Something went wrong
        console.log('Something went wrong!');
    }

    console.log(body);
});