using ChallengeReviewer.AI.Factories;
using ChallengeReviewer.Core.Agents.Abstractions;
using ChallengeReviewer.Core.Enums;

namespace ChallengeReviewer.AI.Agents
{
    public class Agent<TData, TResult>(
        string name,
        string description,
        string prompt,
        float temperature,
        string instructions) : IAgent<TData, TResult>
        where TData : class
        where TResult : class
    {
        public virtual async Task<TResult> RunAsync(
            TData data, 
            Provider provider, 
            Model model, 
            CancellationToken cancellationToken)
        {
            var agent = AgentFactory.CreateAsync(provider, name, description, model, temperature, instructions);
            var result = await agent.RunAsync<TResult>(prompt, cancellationToken: cancellationToken);

            return result.Result;
             
        }
    }
}
