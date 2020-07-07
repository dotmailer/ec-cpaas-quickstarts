// Bring in required dependencies
var request = require("request");

// **** ENTER YOUR DETAILS HERE ****

// Enter your Engagement Cloud API user details
const API_USERNAME = 'YOUR API USERNAME';
const API_PASSWORD = 'YOUR API PASSWORD';

// Enter your mobile number in international format here e.g. for the UK 447123123123
var yourMobileNumber = 'YOUR_MOBILE_NUMBER';

console.log('');
console.log('Sending a batch of SMS using Engagement Cloud CPaaS and NodeJS');
console.log('--------------------------------------------------------------');

// Setup Engagement Cloud CPaaS batch request JSON, this is an array of messages to be sent, we are sending to the SMS channel but they could easily
// be a mix of any messages targeting any channels.
var myRequestBatch = [
    {
        body: 'This is message 1',
        to: { phoneNumber: yourMobileNumber },
        rules: ['sms']
    },
    {
        body: 'This is message 2',
        to: { phoneNumber: yourMobileNumber },
        channelOptions: {
            sms: {
                from: 'Engagement Cloud CPaaS',
                allowUnicode: true
            }
        },
        rules: ['sms']
    }
];

// Call Engagement Cloud CPaaS "One" API
var options = {
    method: 'POST',
    url: "https://api-cpaas.dotdigital.com/cpaas/messages/batch",
    headers:
    {
        'cache-control': 'no-cache',
        'content-type': 'application/json',
        'accept': 'application/json',
        'authorization': "Basic " + new Buffer(API_USERNAME + ":" + API_PASSWORD).toString("base64")
    },
    body: myRequestBatch,
    json: true
};

// Send the request
console.log('');
console.log('Calling Engagement Cloud CPaaS...');

request(options, function (error, response, body) {
    if (error) throw new Error(error);

    console.log("HTTP status code returned: " + response.statusCode);
    console.log(body);

    // Check status of Accepted
    if (response.statusCode == 202) {
        // All ok
        console.log('Message batch successfully sent via Engagement Cloud CPaaS "One" API');
        console.log('An array of message ids has been returned mapping to your request array');
    }
    else {
        // Something went wrong
        console.log('Something went wrong!');
    }
});