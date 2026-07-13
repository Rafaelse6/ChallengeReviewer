namespace ChallengeReviewer.Core.Models.GitHub
{
    public sealed record GitHubRepository(string Owner, string Title, IReadOnlyList<GitHubFile> Files);
}
