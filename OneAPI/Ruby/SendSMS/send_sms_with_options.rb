require 'uri'
require 'json'
require 'net/http'
require 'openssl'

# Your Engagement Cloud CPaaS settings
Username = "YOUR API USERNAME"
Password = "YOUR API PASSWORD"

puts ""
puts "Sending SMS using Engagement Cloud CPaaS and Ruby"
puts "-------------------------------------------------"

# Setup the conneciton object
url = URI("https://api-cpaas.dotdigital.com/cpaas/messages")
http = Net::HTTP.new(url.host, url.port)
http.use_ssl = true
http.verify_mode = OpenSSL::SSL::VERIFY_NONE # This should be removed and SSL certs validated for production environments

# Setup the HTTP request
request = Net::HTTP::Post.new(url)
request.basic_auth Username, Password
request["content-type"] = 'application/json'
request["accept"] = 'application/json'
request["cache-control"] = 'no-cache'

# Create the Engagement Cloud CPaaS request JSON
request.body = 
    "{
      \"body\": \"Your SMS message\",
      \"to\": {
          \"phoneNumber\": \"447123123123\"
        },
      \"channelOptions\": {
          \"sms\": {
              \"from\": \"EC CPaaS\",
              \"allowUnicode\": true
            }
        },
     \"rules\": [ \"sms\" ]
     }"

# Call the web service
puts ""
puts "Calling Engagement Cloud CPaaS..."
response = http.request(request)

puts ""
puts "Call returned status code: " + response.code
puts JSON.pretty_unparse(JSON.parse(response.read_body)) # Pretty print the JSON