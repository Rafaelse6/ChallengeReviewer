using ChallengeReviewer.AI.Agents;
using ChallengeReviewer.Core.Agents.Abstractions;
using ChallengeReviewer.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeReviewer.AI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAgents(this IServiceCollection services)
        {
            // -> Used for generic agents: services.AddKeyedTransient<IAgent<StaticAnalysisReport, Review>, Agent<StaticAnalysisReport, Review>>("ChallengeReviewer");
            services.AddTransient<IAgent<StaticAnalysisReport, Review>, ChallengeReviewerAgent>();
            return services;
        }
    }
}
