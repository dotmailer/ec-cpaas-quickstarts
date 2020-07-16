var express = require('express');
var router = express.Router();
var request = require("request");

// **** ENTER YOUR DETAILS HERE ****

// Enter your Engagement Cloud API user details
const API_USERNAME = 'YOUR API USERNAME';
const API_PASSWORD = 'YOUR API PASSWORD';

// Your Facebook pages numeric id
const FACEBOOK_PAGE_ID = 'YOUR FACEBOOK PAGE ID';

// Your user, hard coded for demo but would come from your way of identifying a user or visitor
const PROFILE_ID = 'test@acme.com';

/* GET home page route. */
router.get('/', function (req, res) {
  // Setup UI feedback
  var feedback = {
    succeeded: true,
    message: 'Facebook metadata generated for ' + PROFILE_ID
  };

  // Get the encrypted Facebook metadata for the current user, in this example test@acme.com
  generateFacebookMetaData(PROFILE_ID).then(function (response) {
    // Got the Facebook metadata, render the page passing the data
    res.render('pages/index', {
      title: 'Engagement Cloud CPaaS Facebook Tutorial',
      metadata: response,
      fbPageId: FACEBOOK_PAGE_ID,
      profileId: PROFILE_ID,
      feedback,
      testButtonsDisplay: 'none'
    });
  }, function (err) {
    // Failed to retrieve the required metadata for Facebook
    feedback.succeeded = false;
    feedback.message = 'Failed to generate metadata: ' + err;

    res.render('pages/index', {
      title: 'Engagement Cloud CPaaS Facebook Tutorial',
      metadata: null,
      fbPageId: FACEBOOK_PAGE_ID,
      profileId: PROFILE_ID,
      feedback,
      testButtonsDisplay: 'none'
    });
  })
});

/* POST home page route. */
router.post('/', function (req, res, next) {
  // Setup UI feedback
  var feedback = {
    succeeded: true,
    message: 'Message sent to ' + PROFILE_ID
  };

  // Get the encrypted Facebook metadata for the current user, in this example test@acme.com
  generateFacebookMetaData(PROFILE_ID).then(function (response) {
    // Got the Facebook metadata, send the Facebook Messenger message

    // What type of message is required.
    if (req.body.SendMessage.toLowerCase() == "simple") {
      // Send a simple message
      sendToFacebookMessenger(PROFILE_ID, "A simple text message sent via Engagement Cloud CPaaS", null);
    } else {
      // Send a rich message
      sendToFacebookMessenger(PROFILE_ID, "This will be ignored because of the customBody field is defined!", {
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
      });
    }

    // Render the page
    res.render('pages/index', {
      title: 'Engagement Cloud CPaaS Facebook Tutorial',
      metadata: response,
      fbPageId: FACEBOOK_PAGE_ID,
      profileId: PROFILE_ID,
      feedback,
      testButtonsDisplay: 'block'
    });
  }, function (err) {
    // Failed to retrieve the required metadata for Facebook
    feedback.succeeded = false;
    feedback.message = 'Failed to generate metadata: ' + err;

    res.render('pages/index', {
      title: 'Engagement Cloud CPaaS Facebook Tutorial',
      metadata: null,
      fbPageId: FACEBOOK_PAGE_ID,
      profileId: PROFILE_ID,
      feedback,
      testButtonsDisplay: 'block'
    });
  })
});

// Export the router for Express
module.exports = router;

// Generates the secure encrypted metadata to be passed to Facebook so that Engagement Cloud CPaaS can stitch the
// Facebook Messenger Id to the correct profile.
function generateFacebookMetaData(profileId) {
  // Return a new promise.
  return new Promise(function (resolve, reject) {
    // Setup Engagement Cloud CPaaS request JSON, any properties in addition to the profileId will be added to the Engagement Cloud CPaaS profile
    var myRequest = {
      profileId: profileId
    };

    // Call Engagement Cloud CPaaS API
    var options = {
      method: 'POST',
      url: "https://api-cpaas.dotdigital.com/cpaas/channels/facebook/state",
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
    request(options, function (error, response, body) {
      if (error) reject(new Error(error));

      // Check status
      if (response.statusCode == 200) {
        // All ok, resolve the promise with the response body which is the metadata
        resolve(body);
      } else {
        // Something went wrong
        reject(Error("Call to create Facebook metadata failed with HTTP code: " + response.statusCode));
      }
    });
  });
}

// Sends a message to Facebook Messenger using the Engagement Cloud CPaaS API
function sendToFacebookMessenger(profileId, bodyMessage, customBody) {
  // Setup Engagement Cloud CPaaS request JSON
  var myRequest = {
    body: bodyMessage,
    to: {
      profileId: profileId
    },
    channelOptions: {
      fbMessenger: {
        messagingType: "MESSAGE_TAG",
        messageTag: "POST_PURCHASE_UPDATE"
      }
    },
    rules: ['fbMessenger']
  };

  // Do we have a customBody to add?
  if (customBody) {
    myRequest.customBody = {};
    myRequest.customBody.fbMessenger = customBody;
  }

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
}