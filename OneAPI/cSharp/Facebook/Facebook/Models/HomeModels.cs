namespace Facebook.Models
{
    /// <summary>
    /// The model class for passing data to the index page
    /// </summary>
    public class HomeIndexViewModel
    {
        /// <summary>
        /// The secure Facebook meta data needed to associate a FacebookId to a Engagement Cloud CPaaS profile
        /// </summary>
        public string FacebookMetaData { get; set; }

        /// <summary>
        /// The numeric page id for the Facebook page that will be messaging
        /// </summary>
        public string FacebookPageId { get; set; }

        /// <summary>
        /// The test message buttons result feedback
        /// </summary>
        public ResultFeedback TestMessageResult { get; set; }
    }

    public class ResultFeedback
    {
        /// <summary>
        /// Indicates whether the call succeeded or not
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The message to display to the user
        /// </summary>
        public string FeedbackMessage { get; set; }

        /// <summary>
        /// The detailed error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}