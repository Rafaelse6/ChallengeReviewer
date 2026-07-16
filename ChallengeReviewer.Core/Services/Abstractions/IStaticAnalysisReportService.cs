using ChallengeReviewer.Core.Models;
using ChallengeReviewer.Core.Models.GitHub;

namespace ChallengeReviewer.Core.Services.Abstractions
{
    public interface IStaticAnalysisReportService
    {
        StaticAnalysisReport AnalyzeCode(GitHubRepository repoContent);
    }
}
