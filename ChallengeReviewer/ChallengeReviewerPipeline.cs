using ChallengeReviewer.AI;
using ChallengeReviewer.Core.Agents.Abstractions;
using ChallengeReviewer.Core.Enums;
using ChallengeReviewer.Core.Models;
using ChallengeReviewer.Core.Models.GitHub;
using ChallengeReviewer.Core.Services.Abstractions;
using ChallengeReviewer.Infra;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChallengeReviewer
{
    public class ChallengeReviewerPipeline
    {
        private IGitHubService? _gitHubService;
        private IStaticAnalysisReportService? _staticAnalysisReportService;
        private IAgent<StaticAnalysisReport, Review>? _agent;

        private string _repositoryUrl = string.Empty;
        private bool _includeCodeReview;
        private Provider _provider = Provider.Ollama;
        private Model _model = Model.Qwen25Coder7B;
        private GitHubRepository _repository = new("", "", []);
        private StaticAnalysisReport _analysis = default!;
        private Review _review = default!;

        private ChallengeReviewerPipeline(IServiceCollection services)
        {
            services.AddInfrastructure();
            services.AddAgents();

            var provider = services.BuildServiceProvider();
            _gitHubService = provider.GetService<IGitHubService>();
            _staticAnalysisReportService = provider.GetService<IStaticAnalysisReportService>();
            _agent = provider.GetService<IAgent<StaticAnalysisReport, Review>>();
        }

        public static ChallengeReviewerPipeline Create(IServiceCollection services)
        {
            return new ChallengeReviewerPipeline(services);
        }

        public ChallengeReviewerPipeline WithWelcomeScreen()
        {
            Console.Clear();

            //Spectre.Console
            AnsiConsole.Write(new FigletText("Challenger Reviewer")
            {
                Color = Color.Purple,
                Justification = Justify.Center
            });
            AnsiConsole.Write(new Text("Version 0.1.0", new Style(Color.Grey)) { Justification = Justify.Center });
            AnsiConsole.Write(new Text("balta.io", new Style(Color.Grey)) { Justification = Justify.Center });

            return this;
        }
    }
}
