using ChallengeReviewer.Core.Services.Abstractions;
using ChallengeReviewer.Infra.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeReviewer.Infra
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IGitHubService, GitHubService>();
            return services;
        }
    }
}
