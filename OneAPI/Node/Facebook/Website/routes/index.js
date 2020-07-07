var express = require('express');
var router = express.Router();

// **** ENTER YOUR DETAILS HERE ****

// Enter your Engagement Cloud API user details
const API_USERNAME = 'YOUR API USERNAME';
const API_PASSWORD = 'YOUR API PASSWORD';
const FACEBOOK_PAGE_ID = 'YOUR FACEBOOK PAGE ID';
const PROFILE_ID = 'test@acme.com';

/* GET home page route. */
router.get('/', function (req, res, next) {
  // Get the encrypted Facebook metadata for the current user, in this example test@acme.com
  generateFacebookMetaData(PROFILE_ID).then(function (response) {
    // Got the Facebook metadata, render the page passing the data
    res.render('pages/index', {
      title: 'Engagement Cloud CPaaS Facebook Tutorial',
      metadata: response,
      fbPageId: FACEBOOK_PAGE_ID,
      profileId: PROFILE_ID
    });
  });
}, function (error) {
  // Failed to retrieve the required metadata for Facebook
  throw error;
});

module.exports = router;

// Generates the secure encrypted metadata to be passed to Facebook so that Engagement Cloud CPaaS can stitch the
// Facebook Messenger Id to the correct profile.
function generateFacebookMetaData(profileId) {
  // Return a new promise.
  return new Promise(function (resolve, reject) {
    // Bring in required dependencies
    var request = require("request");

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