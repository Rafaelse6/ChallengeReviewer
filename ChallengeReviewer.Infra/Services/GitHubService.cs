using ChallengeReviewer.Core.Models.GitHub;
using ChallengeReviewer.Core.Services.Abstractions;
using Octokit;

namespace ChallengeReviewer.Infra.Services
{
    public class GitHubService : IGitHubService
    {
        public async Task<GitHubRepository> FetchRepositoryAsync(string url, CancellationToken cancellationToken)
        {
            var (owner, repo) = ParseRepositoryUrl(url);

            var client = new GitHubClient(new ProductHeaderValue("MeuApp"));
            var tree = await client.Git.Tree.GetRecursive(owner, repo, "HEAD");

            var csFiles = tree.Tree
                .Where(item => item.Type == TreeType.Blob && item.Path.EndsWith(".cs"))
                .ToList();

            var tasks = csFiles.Select(item => FetchFileContentAsync(client, owner, repo, item.Path));
            var files = await Task.WhenAll(tasks);

            return new GitHubRepository(owner, repo, files);
        }

        private static async Task<GitHubFile> FetchFileContentAsync(
            GitHubClient client,
            string owner,
            string repo,
            string path)
        {
            var contents = await client.Repository.Content.GetAllContents(owner, repo, path);
            var content = contents[0].Content ?? string.Empty;

            return new GitHubFile(path, content);
        }

        private static (string Owner, string Repo) ParseRepositoryUrl(string repositoryUrl)
        {
            var uri = new Uri(repositoryUrl.TrimEnd('/'));
            var segments = uri.AbsolutePath.Trim('/').Split('/');

            return segments.Length < 2
                ? throw new ArgumentException($"Invalid URL: {repositoryUrl}")
                : (segments[0], segments[1]);
        }
    }
}
