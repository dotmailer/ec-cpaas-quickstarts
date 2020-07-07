require 'uri'
require 'net/http'
require 'openssl'

class ApplicationController < ActionController::Base

  # Your Engagement Cloud CPaaS settings
  Username = "YOUR API USERNAME"
  Password = "YOUR API PASSWORD"
  FacebookPageId = "YOUR FACEBOOK PAGE ID"

  # Your user, hard coded for demo but would come from your way of identifying a user or visitor
  ProfileId = "joe.blogs@acme.com"

  ################################
  # GET handler for the Index page
  def index
    # Create model for page
    @model = IndexModel.new

    # Assign the Facebook page id we are messaging from
    @model.fbPageId = FacebookPageId

    # Retrieve Facebook metadata for the profile (user) from Engagement Cloud CPaaS
    result = getFacebookMetaData(ProfileId)
    
    # Check the results
    if (result.succeeded)
      @model.metadata = result.data
    else
      # Error
      @model.feedback = result.feedback
    end

    # Render the view
    render :index
  end

  #################################
  # POST handler for the Index page
  def indexPost
    # Create model for page
    @model = IndexModel.new

    # Retrieve Facebook metadata for the profile (user) from Engagement Cloud CPaaS
    result = getFacebookMetaData(ProfileId)
    
    # Check the results
    if (result.succeeded)
      @model.metadata = result.data

      # Which button was pressed?
      if (params['SimpleTest'] != nil)
        # Send a simple text based message
        result = sendFacebookMessage(ProfileId, "This is a simple text based message from Engagement Cloud CPaaS!", nil)
      elsif (params['RichTest'] != nil)
        # Send a Facebook message object
        facebookMessage = "{
                \"text\": \"Pick a color:\",
                \"quick_replies\": [
                    {
                        \"content_type\": \"text\",
                        \"title\": \"Red\",
                        \"payload\": \"DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_RED\"
                    },
                    {
                        \"content_type\": \"text\",
                        \"title\": \"Green\",
                        \"payload\": \"DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_GREEN\"
                    }
                ]
            }"

        result = sendFacebookMessage(ProfileId, nil, facebookMessage)
      end

      # Feedback
      @model.feedback = result.feedback
    else
      # Error
      @model.feedback = result.feedback
    end

    # Show the buttons area
    @model.testButtonsDisplay = "block"

    # Render the view
    render :index
  end

  ###########################################################################################
  private

  #######################################################
  # Send a message to Facebook using the Engagement Cloud CPaaS "One" API
  def sendFacebookMessage(profileId, body, customBody)
    result = WebserviceResult.new

    # Setup the conneciton object
    url = URI("https://api-cpaas.dotdigital.com/cpaas/messages")
    http = Net::HTTP.new(url.host, url.port)
    http.use_ssl = true
    http.verify_mode = OpenSSL::SSL::VERIFY_NONE # This should be removed and SSL certs validated for production environments

    # Setup the HTTP request
    request = Net::HTTP::Post.new(url)
    request.basic_auth Username, Password
    request["content-type"] = 'application/json'
    request["cache-control"] = 'no-cache'
    request["accept"] = 'application/json'

    # Create the Engagement Cloud CPaaS request JSON
    if (body != nil)
      request.body = 
      "{
        \"to\": {
            \"profileId\": \"%s\"
          },
      \"body\": \"%s\",
      \"channelOptions\": {
        \"fbMessenger\": {
          \"messagingType\": \"MESSAGE_TAG\",
          \"messageTag\": \"NON_PROMOTIONAL_SUBSCRIPTION\"
        }
      },
      \"rules\": [ \"fbMessenger\" ]
      }" % [profileId, body]
    elsif (customBody != nil)
      request.body = 
      "{
        \"to\": {
            \"profileId\": \"%s\"
          },
      \"customBody\": {
          \"fbMessenger\": %s
          },
      \"channelOptions\": {
        \"fbMessenger\": {
          \"messagingType\": \"MESSAGE_TAG\",
          \"messageTag\": \"NON_PROMOTIONAL_SUBSCRIPTION\"
        }
      },
      \"rules\": [ \"fbMessenger\" ]
      }" % [profileId, customBody] 
    end

    # Call the web service
    response = http.request(request)

    # Check the results
    if (response.code == "201")
      result.succeeded = true
      result.feedback = Feedback.new
      result.feedback.succeeded = true
      result.feedback.message = "Check your Facebook to see your message"
    else
      # Error
      result.succeeded = false
      result.feedback = Feedback.new
      result.feedback.succeeded = false
      result.feedback.message = "(Returned: %s): %s" % [response.code, response.read_body]
    end

    return result
  end

  ###########################################################################################
  # Retrieve the encrypted Facebook metadata used to tie a Facebook id to a profile in Comapi
  def getFacebookMetaData(profileId)
    result = WebserviceResult.new

    # Setup the conneciton object
    url = URI("https://api-cpaas.dotdigital.com/cpaas/channels/facebook/state")
    http = Net::HTTP.new(url.host, url.port)
    http.use_ssl = true
    http.verify_mode = OpenSSL::SSL::VERIFY_NONE # This should be removed and SSL certs validated for production environments

    # Setup the HTTP request
    request = Net::HTTP::Post.new(url)
    request.basic_auth Username, Password
    request["content-type"] = 'application/json'
    request["cache-control"] = 'no-cache'
    request["accept"] = 'application/json'

    # Setup the request body to request metadata for our user/profile
    # this would usually be the logged in user id, butit is hardcoded for the demo
    request.body = 
    "{
      \"profileId\": \"%s\"
     }" % [ profileId ]

    # Call the web service
    response = http.request(request)

    # Check the results
    if (response.code == "200")
      result.succeeded = true
      result.data = response.read_body.gsub! '"', '' # Strip off double quotes as this is a JSON string
    else
      # Error
      result.succeeded = false
      result.feedback = Feedback.new
      result.feedback.succeeded = false
      result.feedback.message = "(Returned: %s): %s" % [response.code, response.read_body]
    end

    return result
  end

end