namespace ChallengeReviewer.Core.Models
{
    public sealed record Review
    {
        public int OverallScore { get; set; }
        public string Grade { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Encouragement { get; set; } = "";
        public List<string> Strenghts { get; set; } = [];
        public List<string> Improvements { get; set; } = [];
        public List<string> CodeExamples { get; set; } = [];
        public List<string> NextSteps { get; set; } = [];
    }
}
