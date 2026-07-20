using ChallengeReviewer.Core.Models;
using ChallengeReviewer.Core.Models.GitHub;
using ChallengeReviewer.Core.Services.Abstractions;
using System.ComponentModel;

namespace ChallengeReviewer.AI.Tools
{
    public class StaticAnalysisTool(IStaticAnalysisReportService service)
    {
        [Description("Do a static analysis of the code from a public github repository and return a report")]
        public StaticAnalysisReport AnalyzeCode(GitHubRepository repoContent)
        {
            var result = service.AnalyzeCode(repoContent);
            return result;
        }
    }
}
