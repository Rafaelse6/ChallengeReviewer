using ChallengeReviewer.Core.Models.GitHub;

namespace ChallengeReviewer.Core.Services.Abstractions
{
    public interface IGitHubService
    {
        Task<GitHubRepository> FetchRepositoryAsync(string url, CancellationToken cancellationToken);
    }
}
