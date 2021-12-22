namespace AuditManagementCore.ViewModels
{
    public class DiscussionNoteViewModel
    {
    }

    public class DiscussionNoteSummaryModel
    {
        public int Critical { get; set; }
        public int High { get; set; }
        public int Low { get; set; }
        public int Medium { get; set; }
        public int NotStarted { get; set; }
        public int InProgress { get; set; }
        public int InReview { get; set; }
        public int Completed { get; set; }
    }
}