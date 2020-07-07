using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace Facebook.Controllers
{
    public class HomeController : Controller
    {
        // **** Enter you API username and password here ****
        const string API_USERNAME = "YOUR API USERNAME";
        const string API_PASSWORD = "YOUR API PASSWORD";

        const string FACEBOOK_PAGE_ID = "YOUR FACEBOOK PAGE ID";

        public ActionResult Index()
        {
            var viewData = new Models.HomeIndexViewModel();

            // If logged in create the Facebook metadata for the logged in user using the Engagement Cloud CPaaS web service.
            if (Request.IsAuthenticated)
            {
                try
                {
                    viewData.FacebookMetaData = GetFacebookMetaData(User.Identity.Name);
                    viewData.FacebookPageId = FACEBOOK_PAGE_ID;
                }
                catch (Exception ex)
                {
                    // An error occurred.
                    viewData.TestMessageResult = new Models.ResultFeedback()
                    {
                        Success = false,
                        FeedbackMessage = "Failed to generate Facebook metadata, please check the console logs",
                        ErrorMessage = HttpUtility.JavaScriptStringEncode(ex.Message)
                    };
                }
            }

            // Render the view with the model
            return View(viewData);
        }

        public ActionResult TestMessage()
        {
            var viewData = new Models.HomeIndexViewModel();

            try
            {
                // Create the Facebook metadata for the logged in user using the Engagement Cloud CPaaS web service.
                viewData.FacebookMetaData = GetFacebookMetaData(User.Identity.Name);

                // Send a test message via the Engagement Cloud CPaaS "One" API

                // Setup the channel options to indicate this is a A2P post purchase message
                dynamic fbMessengerOptions = new ExpandoObject();
                fbMessengerOptions.messagingType = "MESSAGE_TAG";
                fbMessengerOptions.messageTag = "POST_PURCHASE_UPDATE";

                // Create the request
                var myRequest = new FacebookSendRequest()
                {
                    to = new FacebookSendRequest.toStruct() { profileId = User.Identity.Name }, // Current logged in user
                    body = "A test message sent via Engagement Cloud CPaaS!",
                    channelOptions = new FacebookSendRequest.channelOptionsStruct() { fbMessenger = fbMessengerOptions }
                };

                // Send it
                SendFacebookMessage(myRequest);

                // Set the result
                viewData.TestMessageResult = new Models.ResultFeedback()
                {
                    Success = true,
                    FeedbackMessage = "Test message sent successfully, check Facebook Messenger"
                };
            }
            catch (Exception ex)
            {
                // An error occurred.
                viewData.TestMessageResult = new Models.ResultFeedback()
                {
                    Success = false,
                    FeedbackMessage = "The web service call failed, check the console logs",
                    ErrorMessage = HttpUtility.JavaScriptStringEncode(ex.Message)
                };
            }

            // Render the view with the model
            return View("Index", viewData);
        }

        public ActionResult TestRichMessage()
        {
            var viewData = new Models.HomeIndexViewModel();

            try
            {
                // Create the Facebook metadata for the logged in user using the Engagement Cloud CPaaS web service.
                viewData.FacebookMetaData = GetFacebookMetaData(User.Identity.Name);

                // Send a test message via the Engagement Cloud CPaaS "One" API

                // Setup the channel options to indicate this is a A2P post purchase message
                dynamic fbMessengerOptions = new ExpandoObject();
                fbMessengerOptions.messagingType = "MESSAGE_TAG";
                fbMessengerOptions.messageTag = "POST_PURCHASE_UPDATE";

                // Create the request
                var myRequest = new FacebookSendRequest()
                {
                    to = new FacebookSendRequest.toStruct { profileId = User.Identity.Name }, // Current logged in user
                    channelOptions = new FacebookSendRequest.channelOptionsStruct() { fbMessenger = fbMessengerOptions },
                    customBody = new FacebookSendRequest.customBodyStruct { fbMessenger = @"
                        {
                          ""attachment"": {
                            ""type"": ""image"",
                            ""payload"": {
                                        ""url"": ""http://cdn.dnky.co/ec-tutorials/Images/laptop.png""
                            }
                                }
                        }" }
                };

                // Send it
                SendFacebookMessage(myRequest);

                // Set the result
                viewData.TestMessageResult = new Models.ResultFeedback()
                {
                    Success = true,
                    FeedbackMessage = "Test message sent successfully, check Facebook"
                };
            }
            catch (Exception ex)
            {
                // An error occurred.
                viewData.TestMessageResult = new Models.ResultFeedback()
                {
                    Success = false,
                    FeedbackMessage = "The web service call failed, check the console logs",
                    ErrorMessage = HttpUtility.JavaScriptStringEncode(ex.Message)
                };
            }

            // Render the view with the model
            return View("Index", viewData);
        }

        private static void SendFacebookMessage(FacebookSendRequest FacebookRequest)
        {
            // Setup a REST client object using the web service URI and our API credentials
            var client = new RestClient(@"https://api-cpaas.dotdigital.com/cpaas/messages");
            client.Authenticator = new HttpBasicAuthenticator(API_USERNAME, API_PASSWORD);

            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");

            // Serialise our Facebook request object to JSON for submission
            string requestJson = JsonConvert.SerializeObject(FacebookRequest, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", requestJson, ParameterType.RequestBody);

            // Make the web service call
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                // Something went wrong.
                throw new InvalidOperationException(string.Format("Call to Engagement Cloud CPaaS failed with status code ({0}), and body: {1}", response.StatusCode, response.Content));
            }
        }

        /// <summary>
        /// Retrieves the encrypted meta data to allow Engagement Cloud CPaaS to associate a FB id with a profile.
        /// </summary>
        /// <param name="ProfileId">The Engagement Cloud CPaaS profile id you want the FB id saved to</param>
        /// <returns>The encrypted Facebook meta data</returns>
        private static String GetFacebookMetaData(String ProfileId)
        {
            // Create the request JSON                              
            var requestJson = string.Format(@"{{ ""profileId"": ""{0}"" }}", ProfileId);

            // Setup a REST client object using the web service URI and our API credentials
            var client = new RestClient(@"https://api-cpaas.dotdigital.com/cpaas/channels/facebook/state");
            client.Authenticator = new HttpBasicAuthenticator(API_USERNAME, API_PASSWORD);

            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", requestJson, ParameterType.RequestBody);

            // Make the web service call
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                // Something went wrong.
                throw new InvalidOperationException(string.Format("Call to Engagement Cloud CPaaS failed with status code ({0}), and body: {1}", response.StatusCode, response.Content));
            }

            // Grab the Facebook metadata for the profileId stripping the double quotes
            return response.Content.Replace(@"""", "");
        }


        /// <summary>
        /// JSON.Net Converter for raw JSON
        /// </summary>
        public class RawJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteRawValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(string).IsAssignableFrom(objectType);
            }

            public override bool CanRead
            {
                get { return false; }
            }
        }

        /// <summary>
        /// This object represents an Facebook send for the Engagement Cloud CPaaS "One" API
        /// </summary>
        public class FacebookSendRequest
        {
            public FacebookSendRequest()
            {
                // Default the Engagement Cloud CPaaS channel rules to Facebook
                this.rules = new string[] { "fbMessenger" };
            }

            #region "Structs"
            public struct toStruct
            {
                public toStruct(string profileId)
                {
                    this.profileId = profileId;
                }

                /// <summary>
                /// The profileId you wnat to send the message to
                /// </summary>
                public string profileId;
            }

            public struct customBodyStruct
            {
                /// <summary>
                /// This can be any valid Facebook message body types in JSON format, see the Facebook graph API docs for more info
                /// </summary>
                [JsonConverter(typeof(RawJsonConverter))]
                public string fbMessenger { get; set; }
            }

            public struct channelOptionsStruct
            {
                public dynamic fbMessenger { get; set; }
            }

            #endregion

            /// <summary>
            /// The message body in text format
            /// </summary>
            public string body { get; set; }

            /// <summary>
            /// The addressing information
            /// </summary>
            public toStruct to { get; set; }

            /// <summary>
            /// The option custom body for the request
            /// </summary>
            public customBodyStruct customBody { get; set; }

            /// <summary>
            /// The Engagement Cloud CPaaS API channel rules
            /// </summary>
            public string[] rules { get; set; }

            /// <summary>
            /// The channel options.
            /// </summary>
            public channelOptionsStruct channelOptions { get; set; }
        }
    }
}
