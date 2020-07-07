package com.dotdigital.facebookmessenger;

import org.springframework.http.MediaType;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;

import com.mashape.unirest.http.HttpResponse;
import com.mashape.unirest.http.JsonNode;
import com.mashape.unirest.http.Unirest;
import com.mashape.unirest.http.exceptions.UnirestException;

@Controller
public class FacebookMessengerController {
	// **** Enter you API username and password here ****
	public static final String API_USERNAME = "YOUR API USERNAME";
	public static final String API_PASSWORD = "YOUR API PASSWORD";

	public static final String FACEBOOK_PAGE_ID = "YOUR FACEBOOK PAGE ID";

	// Variables
	////////////
	private String metadata;
	private Boolean testButtonsDisplay;
	private Feedback feedback;

	// Profile string is hard coded but this would normally be the logged in user
	private String profileId = "test@acme.com";

	// Constructor
	//////////////
	public FacebookMessengerController() {
		metadata = null;
		feedback = null;
		testButtonsDisplay = false;
	}

	// Handlers
	///////////

	@GetMapping("/")
	public String index(Model model) {
		// Create Facebook Opt-in secure metadata if required
		if (metadata == null) {
			try {
				metadata = this.getMetadata(profileId);
				testButtonsDisplay = true;
				feedback = new Feedback("Facebook secure metadata generated", true);
			} catch (Exception e) {
				// Error
				feedback = new Feedback(e.getMessage(), false);
			}
		}

		// Add model data
		model.addAttribute("profileId", profileId);
		model.addAttribute("facebookPageId", FACEBOOK_PAGE_ID);
		model.addAttribute("metadata", metadata);
		model.addAttribute("testButtonsDisplay", testButtonsDisplay);
		model.addAttribute("feedback", feedback);

		return "index";
	}

	@RequestMapping(value = "/", consumes = MediaType.APPLICATION_FORM_URLENCODED_VALUE)
	public String index(Model model, @RequestBody MultiValueMap<String, String> formData) {
		if (formData.getFirst("messageType").equalsIgnoreCase("SimpleTest")) {
			// Send simple message
			SendFacebookMessage(profileId, "A simple text message", null);
		} else {
			// Send rich message
			String fbCustomMessage = "{" + "    \"fbMessenger\": {" + "      \"text\": \"Pick a color:\","
					+ "      \"quick_replies\": [" + "      {" + "        \"content_type\": \"text\","
					+ "        \"title\": \"Red\","
					+ "        \"payload\": \"DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_RED\"" + "      }," + "      {"
					+ "        \"content_type\": \"text\"," + "        \"title\": \"Blue\","
					+ "        \"payload\": \"DEVELOPER_DEFINED_PAYLOAD_FOR_PICKING_BLUE\"" + "      }" + "      ]"
					+ "    }" + "  }";

			SendFacebookMessage(profileId, "A rich message", fbCustomMessage);
		}

		return index(model);
	}

	// Functions
	////////////

	// Send a Facebook message using Engagement Cloud CPaaS
	public void SendFacebookMessage(String profileId, String body, String customBody) {
		System.out.println("");
		System.out.println("Sending FB Message using Engagement Cloud CPaaS and Java");
		System.out.println("--------------------------------------------------------");

		if (this.feedback == null) {
			this.feedback = new Feedback();
		}

		// Create Engagement Cloud CPaaS request, this is just a string but you could
		// use objects and serialise to JSON
		String request;

		if (customBody != null) {
			// Rich Facebook message send
			request = String.format("" + "{" + "  \"body\": \"%s\"," + "  \"to\": {" + "    \"profileId\": \"%s\""
					+ "  }," + "  \"customBody\": %s" + "  ," + "  \"rules\": [" + "    \"fbMessenger\"" + "  ], "
					+ "\"channelOptions\": {\r\n    \"fbMessenger\": {\r\n      \"messagingType\": \"MESSAGE_TAG\",\r\n      "
					+ "\"messageTag\": \"POST_PURCHASE_UPDATE\"\r\n    }\r\n  }\r\n}", body, profileId, customBody);
		} else {
			// Basic text based send
			request = String.format("" + "{" + "  \"body\": \"%s\"," + "  \"to\": {" + "    \"profileId\": \"%s\""
					+ "  }," + "  \"rules\": [" + "    \"fbMessenger\"" + "  ], "
					+ "\"channelOptions\": {\r\n    \"fbMessenger\": {\r\n      \"messagingType\": \"MESSAGE_TAG\",\r\n      "
					+ "\"messageTag\": \"POST_PURCHASE_UPDATE\"\r\n    }\r\n  }\r\n}", body, profileId);
		}

		System.out.println(String.format("Request: \n%s", request));

		// Call Engagement Cloud CPaaS
		try {
			System.out.println("Calling Engagement Cloud CPaaS...");
			HttpResponse<JsonNode> response = Unirest.post("https://api-cpaas.dotdigital.com/cpaas/messages")
					.basicAuth(API_USERNAME, API_PASSWORD).header("content-type", "application/json")
					.header("accept", "application/json").header("cache-control", "no-cache").body(request).asJson();

			System.out.println("Call returned status code: (" + response.getStatus() + ") " + response.getStatusText());
			System.out.println(response.getBody().toString());
			System.out.println();

			if (response.getStatus() == 201) {
				// Sent
				feedback.setSucceeded(true);
				feedback.setMessage("Message sent, check your Facebook account now!");
			} else {
				// Failed
				feedback.setSucceeded(false);
				feedback.setMessage("ERROR: " + response.getBody().toString());
			}

		} catch (UnirestException ex) {
			// Error calling service
			System.out.println("ERROR: " + ex.getLocalizedMessage());
			feedback.setSucceeded(false);
			feedback.setMessage("ERROR: " + ex.getLocalizedMessage());
		}
	}

	// Properties
	/////////////

	/**
	 * @param metadata the metadata to set
	 */
	public void setMetadata(String metadata) {
		this.metadata = metadata;
	}

	/**
	 * @return the encrypted Facebook meta data required to associate a Facebook
	 *         Messenger Id with a Engagement Cloud CPaaS profile
	 * @throws Exception
	 */
	public String getMetadata(String requiredProfileId) throws Exception {
		// Call Engagement Cloud CPaaS to create the Facebook metadata if required
		if (metadata == null || metadata.isEmpty()) {
			// Create Engagement Cloud CPaaS request, this is just a string but you could
			// use objects and serialise to JSON
			// This request just contains the profileId you want the Facebook Id associated
			// with.
			String request = String.format("" + "{" + "\"profileId\": \"%s\"" + "}", requiredProfileId);

			System.out.println("Facebook metsdata request JSON: " + request);

			// Call Engagement Cloud CPaaS
			try {
				System.out.println("Calling Engagement Cloud CPaaS...");
				HttpResponse<String> response = Unirest
						.post("https://api-cpaas.dotdigital.com/cpaas/channels/facebook/state")
						.basicAuth(API_USERNAME, API_PASSWORD).header("content-type", "application/json")
						.header("cache-control", "no-cache").body(request).asString();

				System.out.println(
						"Call returned status code: (" + response.getStatus() + ") " + response.getStatusText());
				System.out.println(response.getBody());
				System.out.println();

				if (response.getStatus() == 200) {
					// Call suceeded
					metadata = response.getBody().replace("\"", ""); // Strip off double quotes from JSON string
				} else {
					// Call failed
					throw new Exception("Call to get metadata failed: " + "Call returned status code: ("
							+ response.getStatus() + ") " + response.getStatusText());
				}
			} catch (UnirestException ex) {
				// Error calling service
				System.out.println("ERROR: " + ex.getLocalizedMessage());
				throw ex;
			}
		}

		return metadata;
	}
}