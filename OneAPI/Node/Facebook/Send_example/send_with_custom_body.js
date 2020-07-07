// Bring in required dependencies
var request = require("request");

// **** ENTER YOUR DETAILS HERE ****

// Enter your Engagement Cloud API user details
const API_USERNAME = 'YOUR API USERNAME';
const API_PASSWORD = 'YOUR API PASSWORD';

// Enter your Engagement Cloud CPaaS profile id for a profile who has already clicked the Send to Messenger control demonstrated in the example web site
// Engagement Cloud CPaaS will automatically lookup the fbMessengerId field from the profile.
var profileId = 'test@acme.com';

console.log('');
console.log('Sending to Facebook using Engagement Cloud CPaaS and NodeJS');
console.log('-----------------------------------------------------------');

// Setup Engagement Cloud CPaaS request JSON
var myRequest = {
    body: 'Your text based message',
    to: {
        profileId: profileId
    },
    rules: ['fbMessenger'],
    channelOptions: {
        fbMessenger: {
            messagingType: "MESSAGE_TAG",
            messageTag: "POST_PURCHASE_UPDATE"
        }
    },
    customBody: {
        fbMessenger: {
            text: 'Pick a color:',
            quick_replies: [{
                    content_type: 'text',
                    title: 'Red',
                    payload: 'DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_RED'
                },
                {
                    content_type: 'text',
                    title: 'Green',
                    payload: 'DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_GREEN'
                }
            ]
        }
    }
};

// Call Engagement Cloud CPaaS "One" API
var options = {
    method: 'POST',
    url: "https://api-cpaas.dotdigital.com/cpaas/messages",
    headers: {
        'cache-control': 'no-cache',
        'content-type': 'application/json',
        'accept': 'application/json',
        'authorization': "Basic " + new Buffer.from(API_USERNAME + ":" + API_PASSWORD).toString("base64")
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
    if (response.statusCode == 201) {
        // All ok
        console.log('Message successfully sent via Engagement Cloud CPaaS "One" API');
    } else {
        // Something went wrong
        console.log('Something went wrong!');
    }

    console.log(body);
});