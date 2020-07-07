<?php
// src/Controller/WebhookController.php
namespace App\Controller;

use Exception;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Annotation\Route;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\Request;
use Psr\Log\LoggerInterface;


class WebhookController extends AbstractController
{
    private $logger;

    // Constructor
    public function __construct(LoggerInterface $logger)
    {
        $this->logger = $logger;
    }

    /**
     * @Route("/", methods={"GET"})
     */
    public function index(): Response
    {
        return $this->render('index.html.twig', []);
    }

    /**
     * @Route("/", methods={"POST"})
     */
    public function receiveEvent(Request $request): Response
    {
        try {
            // Setup response object
            $response = new Response();
            $response->headers->set('Content-Type', 'text/plain');

            // Grab the raw body data
            $content = $request->getContent();

            if (empty($request)) {
                // No body found, bad request
                $response->setContent('Bad request - No JSON body found!');
                $response->setStatusCode(Response::HTTP_BAD_REQUEST);

                return $response;
            }

            // First lets ensure it hasn't been tampered with and it came from Engagement Cloud CPaaS
            // We do this by checking the HMAC from the X-Comapi-Signature header
            $requestHmac = $request->headers->get('x-comapi-signature');

            if (empty($requestHmac)) {
                // No HMAC, invalid request.
                $response->setContent('Invalid request: No HMAC value found!');
                $response->setStatusCode(Response::HTTP_UNAUTHORIZED);

                return $response;
            } else {
                // Validate the HMAC, ensure you has exposed the rawBody, see app.js for how to do this
                $hash = hash_hmac('sha1', $content, '>>>YOUR SECRET<<<', FALSE);

                $this->logger->info('hmac: ' . $requestHmac);
                $this->logger->info('hash: ' . $hash);

                if (strcmp($requestHmac, $hash) != 0) {
                    // The request is not from Engagement Cloud CPaaS or has been tampered with
                    $response->setContent('Invalid request: HMAC hash check failed!');
                    $response->setStatusCode(Response::HTTP_UNAUTHORIZED);

                    return $response;
                }
            }

            // Store the received event for later processing, remember you only have 10 secs to process, in this simple example we output to the console 
            $event = json_decode($content);
            $this->logger->info('Received a ' . $event->name . ' event id: ' . $event->eventId);
            $this->logger->info(json_encode($content, JSON_PRETTY_PRINT));

            # You could use queuing tech such as RabbitMQ, or possibly a distributed cache such as Redis       

            # Send worked
            $response->setContent('Data accepted');
            $response->setStatusCode(Response::HTTP_OK);
        } catch (Exception $ex) {
            // An error occurred
            $this->logger->error('An error occurred processing a webhook event: ' . $ex->getMessage());
            $response->setContent('An error occurred');
            $response->setStatusCode(Response::HTTP_INTERNAL_SERVER_ERROR);
        } finally {
            // Respond
            return $response;
        }
    }
}
