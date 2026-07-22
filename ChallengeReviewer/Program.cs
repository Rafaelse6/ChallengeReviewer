using ChallengeReviewer;
using ChallengeReviewer.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

Configuration.Ollama.Url = "http://localhost:11434";
Configuration.OpenAI.ApiKey = configuration["OpenAi:ApiKey"]
    ?? throw new InvalidOperationException("OpenAi:ApiKey is not set");

var services = new ServiceCollection();

using var cts = new CancellationTokenSource();

var pipeline = ChallengeReviewerPipeline.Create(services).WithWelcomeScreen();