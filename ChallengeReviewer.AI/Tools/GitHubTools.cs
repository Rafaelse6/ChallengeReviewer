using ChallengeReviewer.Core.Models.GitHub;
using ChallengeReviewer.Core.Services.Abstractions;
using System.ComponentModel;

namespace ChallengeReviewer.AI.Tools
{
    public class GitHubTools(IGitHubService service)
    {
        [Description("Return all .cs files in a public github repository")]
        public async Task<GitHubRepository> FetchRepositoryAsync(
            [Description("GitHub repository URL")] string url, CancellationToken cancellationToken)
        {
            var data = await service.FetchRepositoryAsync(url, cancellationToken);
            return data;
        }
    }
}
