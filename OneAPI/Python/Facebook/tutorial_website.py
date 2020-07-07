# -*- coding: utf-8 -*-
# A basic website to demonstrate Facebook opt-in using Engagement Cloud CPaaS and Python.

import http.client
import json
import os
from flask import Flask, render_template, url_for, send_from_directory, request
from base64 import b64encode

# Your Engagement Cloud CPaaS settings
API_USERNAME = "YOUR API USERNAME"
API_PASSWORD = "YOUR API PASSWORD"
FACEBOOK_PAGE_ID = "YOUR FACEBOOK PAGE ID"

# Create the basic auth header encoded username and password
userAndPass = b64encode(bytes(API_USERNAME + ":" + API_PASSWORD, "ascii")).decode("ascii")

# Your user, hard coded for demo but would come from your way of identifying a user or visitor
profileId = "test@acme.com"

# Create our website app
app = Flask(__name__)

#######################
# Methods and functions
#######################
def createIndexModel():
    model = {
        "metadata": None,
        "testButtonsDisplay": "none",
        "pageId": FACEBOOK_PAGE_ID
    }

    return model

def sendFacebookMessage(profileId, message, customBody):
    # Setup the http connection
    conn = http.client.HTTPSConnection("api-cpaas.dotdigital.com")

    # Construct the Engagement Cloud CPaaS send message API request
    if (customBody == ""):
        # No custom body
        myRequest = {
            "to": {
                "profileId": profileId
            },
            "body": message,
            "channelOptions": {
                "fbMessenger": {
                    "messagingType": "MESSAGE_TAG",
                    "messageTag": "POST_PURCHASE_UPDATE"
                }
            },
            "rules": ["fbMessenger"]
        }
    else:
        # Custom body
        myRequest = {
            "to": {
                "profileId": profileId
            },
            "body": message,
            "customBody": {
                "fbMessenger": customBody
            },
            "channelOptions": {
                "fbMessenger": {
                    "messagingType": "MESSAGE_TAG",
                    "messageTag": "POST_PURCHASE_UPDATE"
                }
            },
            "rules": ["fbMessenger"]
        }

    print("Message send request JSON: ")
    print(json.dumps(myRequest))

    # Setup the http headers
    headers = {
        'authorization' : 'Basic %s' %  userAndPass,
        "content-type": "application/json",
        "cache-control": "no-cache",
        "accept": "application/json"
    }

    # Make the webservice request to send the message
    print("Calling Engagement Cloud CPaaS...")
    conn.request("POST", "/cpaas/messages", json.dumps(myRequest), headers)

    res = conn.getresponse()
    responseBody = res.read().decode("utf-8")

    if (res.status == 201):
        # Message sent
        print("Message sent: " + responseBody)
    else:
        # Error
        raise IOError("Web service call failed with ({0}) - {1}".format(res.status, responseBody))

#################
# Register routes
#################
@app.route("/favicon.ico")
def get_favicon():
    return send_from_directory(os.path.join(app.root_path, 'static'), 'favicon.ico', mimetype='image/vnd.microsoft.icon') 

@app.route("/")
def get_index():
    # Setup the http connection
    conn = http.client.HTTPSConnection("api-cpaas.dotdigital.com")

    # Construct the Engagement Cloud CPaaS API request
    myRequest = {
        "profileId": profileId
    }

    print("")
    print("Request JSON: ")
    print(json.dumps(myRequest))

    # Setup the http headers
    headers = {
        'authorization' : 'Basic %s' %  userAndPass,
        "content-type": "application/json",
        "cache-control": "no-cache",
        "accept": "application/json"
    }

    # Make the webservice request to create the secure meta data with the profile details
    print("")
    print("Calling Engagement Cloud CPaaS...")
    conn.request("POST", "/cpaas/channels/facebook/state", json.dumps(myRequest), headers)

    res = conn.getresponse()
    responseBody = res.read().decode("utf-8")

    if (res.status == 200):
        # Meta data returned
        print("Meta data: " + responseBody)
    else:
        # Error
        raise IOError("Web service call failed with (" + res.status + ") - " + responseBody)
    
    # Create page model data
    model = createIndexModel()
    model["metadata"] = json.loads(responseBody)

    # Render the page passing the model data
    return render_template('index.html', model=model)

@app.route('/', methods=['POST'])
def post_index():
    # Create page model data
    model = createIndexModel()

    # Set the test buttons to visible
    model["testButtonsDisplay"] = "block"

    # Do the action for the button
    try:
        if 'SimpleTest' in request.form:
            print ("Simple test")

            # Do the send
            sendFacebookMessage(profileId, "A simple text message sent via Engagement Cloud CPaaS", "")
        
        if 'RichTest' in request.form:
            print ("Rich test")

            # Create Facebook message object
            facebookMessage = {
                "text": "Pick a color:",
                "quick_replies": [
                    {
                        "content_type": "text",
                        "title": "Red",
                        "payload": "DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_RED"
                    },
                    {
                        "content_type": "text",
                        "title": "Green",
                        "payload": "DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_GREEN"
                    }
                ]
            }

            # Do the send
            sendFacebookMessage(profileId, "", facebookMessage)

        # Send worked
        model["feedback"] = {}
        model["feedback"]["succeeded"] = True
        model["feedback"]["message"] = "Message sent, check Facebook / Facebook Messenger"

    except Exception as ex:
        # Send failed
        model["feedback"] = {}
        model["feedback"]["succeeded"] = False
        model["feedback"]["message"] = "{0}".format(ex)

    return render_template('index.html', model=model)

# Fire up the local server
if __name__ == "__main__":
    app.run()