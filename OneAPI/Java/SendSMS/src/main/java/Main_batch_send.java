import com.cedarsoftware.util.io.JsonWriter;
import com.mashape.unirest.http.HttpResponse;
import com.mashape.unirest.http.Unirest;
import com.mashape.unirest.http.exceptions.UnirestException;

/**
 * Basic example of how to send an SMS using the Engagement Cloud CPaaS "One" API
 *
 * @author dave.baddeley
 */
public class Main_batch_send {

    // Engagement Cloud CPaaS settings
    private static String API_USERNAME = "YOUR API USERNAME";
    private static String API_PASSWORD = "YOUR API PASSWORD";
    
    private static String PHONE_NUMBER = "447123123123";
    
    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        System.out.println("Sending SMS batches using Engagement Cloud CPaaS and Java");
        System.out.println("---------------------------------------------------------");

        // Create Engagement Cloud CPaaS request, this is just a string but you could use objects and serialise to JSON
        // To send a batch you simply create an array of message requests.
        String request = ""
                + "["
                + "{"
                + "  \"body\": \"This is message 1\","
                + "  \"to\": {"
                + "    \"phoneNumber\": \"" + PHONE_NUMBER + "\""
                + "  },"
                + "  \"channelOptions\": {"
                + "    \"sms\": {"
                + "      \"from\": \"EC CPaaS\","
                + "      \"allowUnicode\": true"
                + "    }"
                + "  },"
                + "  \"rules\": ["
                + "    \"sms\""
                + "  ]"
                + "},"
                + "{"
                + "  \"body\": \"This is message 2\","
                + "  \"to\": {"
                + "    \"phoneNumber\": \"" + PHONE_NUMBER + "\""
                + "  },"
                + "  \"channelOptions\": {"
                + "    \"sms\": {"
                + "      \"from\": \"EC CPaaS\","
                + "      \"allowUnicode\": true"
                + "    }"
                + "  },"
                + "  \"rules\": ["
                + "    \"sms\""
                + "  ]"
                + "}"
                + "]";

        // Call Engagement Cloud CPaaS
        try {
            System.out.println("Calling Engagement Cloud CPaaS...");
            HttpResponse<String> response = Unirest.post("https://api-cpaas.dotdigital.com/cpaas/messages/batch")
                    .basicAuth(API_USERNAME, API_PASSWORD)
                    .header("content-type", "application/json")
                    .header("accept", "application/json")
                    .header("cache-control", "no-cache")
                    .body(request)
                    .asString();
            
            System.out.println("Call returned status code: (" + response.getStatus() + ") " + response.getStatusText());
            
            // Check result
            if (response.getStatus() == 202)
            {
                // All ok
                System.out.println("Call succeeded");
                System.out.println(JsonWriter.formatJson(response.getBody().toString()));
                System.out.println();
            }
            else
            {
                // Failed
                System.out.println("Call failed!");
                System.out.println(response.getBody().toString());
                System.out.println();
            }
            
        } catch (UnirestException ex) {
            // Error calling service
            System.out.println("ERROR: " + ex.getLocalizedMessage());
        }
    }
}
