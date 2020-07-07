import http.client
from base64 import b64encode
import json

print("")
print("Sending SMS using Engagement Cloud CPaaS and Python")
print("---------------------------------------------------")

# Your Engagement Cloud API user credentials
API_USERNAME = "YOUR API USERNAME"
API_PASSWORD = "YOUR API PASSWORD"

# Setup the http connection
conn = http.client.HTTPSConnection("api-cpaas.dotdigital.com")

# Create the basic auth header encoded username and password
userAndPass = b64encode(bytes(API_USERNAME + ":" + API_PASSWORD, "ascii")).decode("ascii")

# Construct the Comapi API request
myRequest = {
    "to": {
        "phoneNumber": "447123123123"
    },
    "body": "This is an SMS via Engagement Cloud CPaaS \"One\" API",
    "channelOptions":
    {
        "sms": {
            "from": "EC CPaaS",
            "allowUnicode": True
        }
    },
    "rules": ["sms"]
}

print("")
print("Request JSON: ")
print(json.dumps(myRequest, indent=2))

# Setup the http headers
headers = {
    'authorization' : 'Basic %s' %  userAndPass,
    'content-type': "application/json",
    'cache-control': "no-cache"
}

# Make the webservice request
print("")
print("Calling Engagement Cloud CPaaS...")
conn.request("POST", "/cpaas/messages",
             json.dumps(myRequest), headers)

res = conn.getresponse()
data = res.read()

print("")
print("Call returned status code: " + str(res.status))
print(json.dumps(json.loads(data.decode("utf-8")), indent=2)) # Pretty print the JSON
print("")