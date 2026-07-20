using ChallengeReviewer.Core;
using ChallengeReviewer.Core.Enums;
using ChallengeReviewer.Core.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;

namespace ChallengeReviewer.AI.Factories
{
    public static class AgentFactory
    {
        public static ChatClientAgent CreateAsync(
            Provider provider,
            string name,
            string description,
            Model model,
            float temperature,
            string instructions,
            IList<AITool> tools)
        {
            var client = provider switch
            {
                Provider.Ollama => new OllamaApiClient(Configuration.Ollama.Url),

                Provider.OpenAi => new OpenAIClient(Configuration.OpenAI.ApiKey)
                    .GetChatClient(model.Description())
                    .AsIChatClient(),

                _ => throw new NotSupportedException($"Provider {provider} is not supported")
            };

            return client.AsAIAgent(new ChatClientAgentOptions
            {
                Name = name,
                Description = description,
                ChatOptions = new ChatOptions
                {
                    ModelId = model.Description(),
                    Instructions = instructions,
                    Tools = tools
                }
            });
        }
    }
}
