package com.dotdigital.facebookmessenger;

public class Feedback {
    private Boolean succeeded;
    private String message;

    public Feedback()
    {
        succeeded = false;
        message = "";
    }
    
    public Feedback(String feedbackMessage, Boolean successful)
    {
        succeeded = successful;
        message = feedbackMessage;
    }

    /**
     * @return the Succeeded
     */
    public Boolean getSucceeded() {
        return succeeded;
    }

    /**
     * @param succeeded the Succeeded to set
     */
    public void setSucceeded(Boolean Succeeded) {
        this.succeeded = Succeeded;
    }

    /**
     * @return the Message
     */
    public String getMessage() {
        return message;
    }

    /**
     * @param Message the Message to set
     */
    public void setMessage(String Message) {
        this.message = Message;
    }
}