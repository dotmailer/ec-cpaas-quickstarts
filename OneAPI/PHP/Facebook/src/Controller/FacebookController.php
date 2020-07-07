<?php
// src/Controller FacebookController.php
namespace App\Controller;

use Exception;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Psr\Log\LoggerInterface;

class FacebookController extends AbstractController
{
    private $logger;
    private $fbMetaData;

    // **** ENTER YOUR DETAILS HERE ****

    // Enter your Engagement Cloud API user details
    const API_USERNAME = 'YOUR API USERNAME';
    const API_PASSWORD = 'YOUR API PASSWORD';
    const FACEBOOK_PAGE_ID = 'YOUR FACEBOOK PAGE ID';

    // Your user, hard coded for demo but would come from your way of identifying a user or visitor
    const PROFILE_ID = 'test@acme.com';

    // Constructor
    public function __construct(LoggerInterface $logger)
    {
        $this->logger = $logger;
        $this->fbMetaData = $this->generateFacebookMetaData(self::PROFILE_ID);
    }

    /**
     * @Route("/", methods={"GET"})
     */
    public function index(): Response
    {
        // Did we create the secure Facebook metadata?
        if (!empty($this->fbMetaData)) {
            // All is well
            return $this->render('index.html.twig', [
                'metadata' => $this->fbMetaData,
                'fbPageId' => self::FACEBOOK_PAGE_ID,
                'profileId' => self::PROFILE_ID,
                'showFeedback' => TRUE,
                'feedbackMsg' => 'Facebook secure metadata generated',
                'feedbackIsError' => FALSE,
                'testButtonDisplay' => FALSE
            ]);
        } else {
            // Problems
            return $this->render('index.html.twig', [
                'metadata' => $this->fbMetaData,
                'fbPageId' => self::FACEBOOK_PAGE_ID,
                'profileId' => self::PROFILE_ID,
                'showFeedback' => TRUE,
                'feedbackMsg' => 'Failed to generate Facebook metadata, check logs for details!',
                'feedbackIsError' => TRUE,
                'testButtonDisplay' => FALSE
            ]);
        }
    }

    /**
     * @Route("/", methods={"POST"})
     */
    public function indexPost(Request $request): Response
    {
        // Send a test message, simple or rich?
        $buttonPressed = $request->request->get('Button');

        if ($buttonPressed == "simple") {
            // Send a simple message
            $requestJson =
                '{
                    "to": {
                        "profileId":"' . self::PROFILE_ID . '"
                        },
                    "rules":["fbMessenger"],
                    "body":"Your text based message",
                    "channelOptions": {
                        "fbMessenger": {
                            "messagingType": "MESSAGE_TAG",
                            "messageTag": "POST_PURCHASE_UPDATE"
                        }
                    }
                }';
        } else {
            // Send a rich message
            $requestJson =
                '{
                    "to": {
                        "profileId":"' . self::PROFILE_ID . '"
                        },
                    "rules":["fbMessenger"],
                    "customBody": {
                        "fbMessenger": {
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
                    },
                    "channelOptions": {
                        "fbMessenger": {
                            "messagingType": "MESSAGE_TAG",
                            "messageTag": "POST_PURCHASE_UPDATE"
                        }
                    }
                }';
        }

        // Send the message
        if ($this->SendMessage($requestJson)) {
            // All is well
            return $this->render('index.html.twig', [
                'metadata' => $this->fbMetaData,
                'fbPageId' => self::FACEBOOK_PAGE_ID,
                'profileId' => self::PROFILE_ID,
                'showFeedback' => TRUE,
                'feedbackMsg' => 'Message sent successully',
                'feedbackIsError' => FALSE,
                'testButtonDisplay' => TRUE
            ]);
        } else {
            // Problems
            return $this->render('index.html.twig', [
                'metadata' => $this->fbMetaData,
                'fbPageId' => self::FACEBOOK_PAGE_ID,
                'profileId' => self::PROFILE_ID,
                'showFeedback' => TRUE,
                'feedbackMsg' => 'Failed to send message, check logs for details!',
                'feedbackIsError' => TRUE,
                'testButtonDisplay' => FALSE
            ]);
        }
    }

    // Functions
    ////////////
    private function generateFacebookMetaData(String $profileId): String
    {
        $facebookMetaData = '';

        # Setup the CURL object
        $curl = curl_init();

        # Setup Engagement Cloud CPaaS request JSON, any properties in addition to the profileId will be added to the Engagement Cloud CPaaS profile
        $request = '{ "profileId": "' . $profileId . '" }';

        # Configure the cURL request
        curl_setopt_array($curl, array(
            CURLOPT_URL => "https://api-cpaas.dotdigital.com/cpaas/channels/facebook/state",
            CURLOPT_USERPWD => self::API_USERNAME . ":" . self::API_PASSWORD,
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

        # Call the web service to get the secure meta data
        $response = curl_exec($curl);
        $httpcode = curl_getinfo($curl, CURLINFO_HTTP_CODE);
        $err = curl_error($curl);

        curl_close($curl);

        if ($err) {
            $this->logger->error('Call to create Facebook metadata failed with return code:' . $err);
        } else {
            if ($httpcode == 200) {
                // All ok, pass back the secure meta data removing outer double quotes
                $facebookMetaData = str_replace('"', '', $response);
            } else {
                // Something went wrong
                $this->logger->error('Call to create Facebook metadata failed with return code (' . $httpcode . ') and response: ' . $response);
            }
        }

        return $facebookMetaData;
    }

    private function SendMessage(String $requestJson): bool
    {
        $result = FALSE;

        # Setup the CURL object
        $curl = curl_init();

        # Configure the cURL request
        curl_setopt_array($curl, array(
            CURLOPT_URL => "https://api-cpaas.dotdigital.com/cpaas/messages",
            CURLOPT_USERPWD => self::API_USERNAME . ":" . self::API_PASSWORD,
            CURLOPT_RETURNTRANSFER => true,
            CURLOPT_ENCODING => "",
            CURLOPT_MAXREDIRS => 10,
            CURLOPT_TIMEOUT => 60,
            CURLOPT_HTTP_VERSION => CURL_HTTP_VERSION_1_1,
            CURLOPT_CUSTOMREQUEST => "POST",
            CURLOPT_POSTFIELDS => $requestJson,
            CURLOPT_HTTPHEADER => array(
                "accept: application/json",
                "content-type: application/json"
            ),
        ));

        # Call the web service to send the message
        $response = curl_exec($curl);
        $httpcode = curl_getinfo($curl, CURLINFO_HTTP_CODE);
        $err = curl_error($curl);

        curl_close($curl);

        if ($err) {
            $this->logger->error('Call to send Facebook message failed with return code:' . $err);
        } else {
            if ($httpcode == 201) {
                // All ok, pass back the secure meta data removing outer double quotes
                $result = TRUE;
            } else {
                // Something went wrong
                $result = FALSE;
                $this->logger->error('Call to create Facebook message failed with return code (' . $httpcode . ') and response: ' . $response);
            }
        }

        return $result;
    }
}
