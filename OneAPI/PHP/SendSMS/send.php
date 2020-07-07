<?php

# Your Engagement Cloud CPaaS settings
$username = "YOUR API USERNAME";
$password = "YOUR API PASSWORD";

echo "\r\n";
echo "Sending SMS using Engagement Cloud CPaaS and PHP\r\n";
echo "------------------------------------------------\r\n";

# Setup the CURL object
$curl = curl_init();

# Create the Engagement Cloud CPaaS request JSON
$request = "{
    \"to\": {
        \"phoneNumber\":\"447123123123\"
        },
    \"rules\":[\"sms\"]
    ,\"body\":\"Your SMS message\"
}";

# Configure the cURL request
curl_setopt_array($curl, array(
CURLOPT_URL => "https://api-cpaas.dotdigital.com/cpaas/messages",
CURLOPT_USERPWD => $username . ":" . $password,
CURLOPT_RETURNTRANSFER => true,
CURLOPT_ENCODING => "",
CURLOPT_MAXREDIRS => 10,
CURLOPT_TIMEOUT => 60,
CURLOPT_HTTP_VERSION => CURL_HTTP_VERSION_1_1,
CURLOPT_CUSTOMREQUEST => "POST",
CURLOPT_POSTFIELDS => $request,
CURLOPT_HTTPHEADER => array(
"accept: application/json",
"content-type: application/json"
),
));

# Call the web service
echo "\r\n";
echo "Calling Engagement Cloud CPaaS...\r\n";
$response = curl_exec($curl);
$httpcode = curl_getinfo($curl, CURLINFO_HTTP_CODE);
$err = curl_error($curl);

curl_close($curl);

echo "\r\n";
echo "Call returned status code: " . $httpcode . "\r\n";

if ($err) {
    echo "cURL Error #:" . $err. "\r\n";
} else {
    # Pretty print the JSON
    echo json_encode($response, JSON_PRETTY_PRINT);
}
