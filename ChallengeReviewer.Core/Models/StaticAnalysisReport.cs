namespace ChallengeReviewer.Core.Models
{
    public sealed record StaticAnalysisReport(
        int TotalFiles,
        int TotalLines,
        int TotalClasses,
        int TotalMethods,
        IReadOnlyList<Issue> Issues)
    {
        public IReadOnlyList<Issue> Errors => Issues.Where(i => i.Severity == Enums.IssueSeverity.Error).ToList();
        public IReadOnlyList<Issue> Warnings => Issues.Where(i => i.Severity == Enums.IssueSeverity.Warning).ToList();
        public IReadOnlyList<Issue> Info => Issues.Where(i => i.Severity == Enums.IssueSeverity.Info).ToList();
    }
}
