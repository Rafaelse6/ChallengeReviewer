using ChallengeReviewer.Core.Enums;

namespace ChallengeReviewer.Core.Models
{
    public record Issue(
        string FIlePath,
        string RuleName,
        string Message,
        int Line,
        IssueSeverity Severity);
}
