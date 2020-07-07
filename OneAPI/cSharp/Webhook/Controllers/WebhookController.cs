﻿/******************************************************************************
 * Description: Simple webhook implementation for receiving events from Engagement Cloud CPaaS
 * Author:      Dave Baddeley
 *****************************************************************************/

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Webhook.Utils;

/// <summary>
/// Engagement Cloud CPaaS webhook controller
/// </summary>
namespace Webhook.Controllers
{
    public class WebhookController : Controller
    {
        // Your webhooks secret
        private readonly string YOUR_WEBHOOK_SECRET = ">>>YOUR WEBHOOK SECRET<<<";

        [HttpGet]
        [Route("/")]
        public ContentResult Index()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                Content = @"<!DOCTYPE html><html> <head> <title>Engagement Cloud CPaaS webhook page</title> </head> <body> <h1>Engagement Cloud CPaaS webhook page</h1> <p>Configure this page as your Engagement Cloud CPaaS webhook location to start receiving data.</p> </body></html>"
            };
        }

        // POST /webhook
        [HttpPost]
        [Route("/")]
        public async System.Threading.Tasks.Task<IActionResult> PostAsync()
        {
            // Process data received from Engagement Cloud CPaaS
            try
            {
                // Grab the body and parse to a JSON object
                string rawBody = await GetDocumentContentsAsync(Request);
                if (string.IsNullOrEmpty(rawBody))
                {
                    // No body, bad request.
                    return BadRequest("Bad request - No JSON body found!");
                }

                // We have a request body so lets look at what we have

                // First lets ensure it hasn't been tampered with and it came from Engagement Cloud CPaaS
                // We do this by checking the HMAC from the X-Engagement Cloud CPaaS-Signature header
                string hmac = Request.Headers["x-comapi-signature"];

                if (String.IsNullOrEmpty(hmac))
                {
                    // No HMAC, invalid request.
                    RollingLogger.LogMessage("Invalid request: No HMAC value found!");
                    return Unauthorized();
                }
                else
                {
                    // Validate the HMAC, ensure you has exposed the rawBody, see app.js for how to do this
                    var hash = CreateHMAC(rawBody, YOUR_WEBHOOK_SECRET);

                    if (hmac != hash)
                    {
                        // The request is not from Engagement Cloud CPaaS or has been tampered with
                        RollingLogger.LogMessage("Invalid request: HMAC hash check failed!");
                        return Unauthorized();
                    }
                }

                // Parse the recieved JSON to extract data
                dynamic eventObj = JsonConvert.DeserializeObject(rawBody);

                // Store the received event for later processing, remember you only have 10 secs to process, in this simple example we output to the console
                RollingLogger.LogMessage("");
                RollingLogger.LogMessage(String.Format("Received a {0} event id: {1}", (string)eventObj.name, (string)eventObj.eventId));
                RollingLogger.LogMessage(FormatJson(rawBody));

                // You could use queuing tech such as RabbitMQ, MSMQ or possibly a distributed cache such as Redis

                // All good return a 200
                return Ok("Data accepted");
            }
            catch (Exception err)
            {
                // An error occurred
                var msg = "An error occurred receiving data, the error was: " + err.ToString();
                RollingLogger.LogMessage(msg);
                throw;
            }
        }

        /// <summary>
        /// Creates a HMAC-SHA1 hash
        /// </summary>
        /// <param name="data">The data to be hashed</param>
        /// <param name="key">The secret to use as a crypto key</param>
        /// <returns>HMAC-SHA1 hash for the data</returns>
        private string CreateHMAC(string data, string key)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);

            HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);

            byte[] messageBytes = encoding.GetBytes(data);
            byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);

            return ByteToString(hashmessage);
        }

        /// <summary>
        /// Converts a byte array to hex string
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary.ToLower());
        }

        /// <summary>
        /// Retrieves the body of a HTTP request as a string
        /// </summary>
        /// <param name="Request">The HTTP Request</param>
        /// <returns>The body data as a string</returns>
        private async System.Threading.Tasks.Task<string> GetDocumentContentsAsync(HttpRequest Request)
        {
            string documentContents;
            using (Stream receiveStream = Request.Body)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    documentContents = await readStream.ReadToEndAsync();
                }
            }
            return documentContents;
        }

        /// <summary>
        /// Formats JSON to make it more readable.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>Formatted JSON string</returns>
        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}

