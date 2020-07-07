/**********************************************************************************************
 * Description: Simple SMS send example using the Engagement Cloud CPaaS "One" API and .Net 3.5
 * Author:      Dave Baddeley
 **********************************************************************************************/
 
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;

namespace SendSMS3_5
{
    class Program
    {
        // **** Enter you API username and password here ****
        const string API_USERNAME = "YOUR API USERNAME";
        const string API_PASSWORD = "YOUR API PASSWORD";

        // **** Enter your mobile number here ****
        private const string MOBILE_NUMBER = "447123123123";
        private const int BATCH_SIZE = 3;

        static void Main(string[] args)
        {            
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Engagement Cloud CPaaS \"One\" API SMS send example");
                Console.ForegroundColor = ConsoleColor.White;

                string input, mode = null;
                SMSSendRequest myRequest = null;

                // Ask the user what demo mode they want
                do
                {                    
                    Console.WriteLine("Send a `single` message or a `batch`, enter your choice now?");
                    input = Console.ReadLine().ToLower();
                    switch (input)
                    {
                        case "s":
                        case "single":
                            mode = "single";
                            Console.WriteLine("Performing a single send");
                            break;
                        case "b":
                        case "batch":
                            mode = "batch";
                            Console.WriteLine("Performing a batch send of {0} messages", BATCH_SIZE);
                            break;
                    }

                    if (!string.IsNullOrEmpty(mode)) break;
                 
                } while (true);

                // Set the channel options; optional step, comment out to use a local number to send from automatically
                var myChannelOptions = new SMSSendRequest.channelOptionsStruct();
                myChannelOptions.sms = new SMSSendRequest.smsChannelOptions() { from = "EC CPaaS", allowUnicode = false };

                // Send the messages
                switch (mode)
                {
                    case "single":
                        // Create an SMS request.
                        myRequest = new SMSSendRequest();
                        myRequest.to = new SMSSendRequest.toStruct(MOBILE_NUMBER);
                        myRequest.body = "This is an SMS via Engagement Cloud CPaaS \"One\" API";
                        myRequest.channelOptions = myChannelOptions;

                        // Send it.
                        SendSMS(myRequest);

                        break;
                    case "batch":
                        // Create a couple of requests in an array to create a batch of requests
                        SMSSendRequest[] myBatch = new SMSSendRequest[BATCH_SIZE];

                        for (int i = 0; i < BATCH_SIZE; i++)
                        {
                            // Create a message send request 
                            myRequest = new SMSSendRequest();
                            myRequest.to = new SMSSendRequest.toStruct(MOBILE_NUMBER);
                            myRequest.body = "This is message " + i;
                            myRequest.channelOptions = myChannelOptions;

                            // Add to batch array
                            myBatch[i] = myRequest;
                        }

                        // Send them
                        SendSMSBatch(myBatch);

                        break;
                }
                
                // All good
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SMS sent successfully");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                // Error
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", ex);
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Wait
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }

        private static void SendSMS(SMSSendRequest smsRequest)
        {
            // Setup a REST client object using the web service URI and our API credentials
            var client = new RestClient(@"https://api-cpaas.dotdigital.com/cpaas/messages");
            client.Authenticator = new HttpBasicAuthenticator(API_USERNAME, API_PASSWORD);

            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json");
            
            // Serialise our SMS request object to JSON for submission
            string requestJson = JsonConvert.SerializeObject(smsRequest, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", requestJson, ParameterType.RequestBody);

            // Make the web service call
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                // Something went wrong.
                throw new InvalidOperationException(string.Format("Call to Engagement Cloud CPaaS failed with status code ({0}), and body: {1}", response.StatusCode, response.Content));
            }
            else
            {
                // Sucess output the response body
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(response.Content);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void SendSMSBatch(SMSSendRequest[] smsRequests)
        {
            // Setup a REST client object using the web service URI and our API credentials
            var client = new RestClient(@"https://api-cpaas.dotdigital.com/cpaas/messages/batch");
            client.Authenticator = new HttpBasicAuthenticator(API_USERNAME, API_PASSWORD);

            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json");

            // Serialise our SMS request object to JSON for submission
            string requestJson = JsonConvert.SerializeObject(smsRequests, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            request.AddParameter("application/json", requestJson, ParameterType.RequestBody);

            // Make the web service call
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                // Something went wrong.
                throw new InvalidOperationException(string.Format("Call to Engagement Cloud CPaaS failed with status code ({0}), and body: {1}", response.StatusCode, response.Content));
            }
            else
            {
                // Sucess output the response body
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(response.Content);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        /// <summary>
        /// This object represents an SMS send for the Engagement Cloud CPaaS "One" API
        /// </summary>
        public class SMSSendRequest
        {
            public SMSSendRequest()
            {
                // Default the Engagement Cloud CPaaS channel rules to SMS
                this.rules = new string[] { "sms" };
            }

            #region "Structs"
            public struct toStruct
            {
                public toStruct(string mobileNumber)
                {
                    this.phoneNumber = mobileNumber;
                }

                /// <summary>
                /// The phone number you want to send to in international format e.g. 447123123123
                /// </summary>
                public string phoneNumber;
            }

            public struct smsChannelOptions
            {
                /// <summary>
                /// The originator the SMS is from, this could be a phone number, shortcode or alpha
                /// </summary>
                public string from;

                /// <summary>
                /// Flag to indicate whether unicode messages are allowed to be sent
                /// </summary>
                public bool? allowUnicode;
            }

            public struct channelOptionsStruct
            {
                /// <summary>
                /// The SMS channels options
                /// </summary>
                public smsChannelOptions sms;
            }
            #endregion

            /// <summary>
            /// The SMS message body
            /// </summary>
            public string body { get; set; }

            /// <summary>
            /// The addressing information
            /// </summary>
            public toStruct to { get; set; }

            /// <summary>
            /// The channel options for the request
            /// </summary>
            public channelOptionsStruct? channelOptions { get; set; }

            /// <summary>
            /// The Engagement Cloud CPaaS API channel rules
            /// </summary>
            public string[] rules { get; set; }
        }
    }
}
