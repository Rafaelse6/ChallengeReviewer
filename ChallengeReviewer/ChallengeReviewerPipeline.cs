using ChallengeReviewer.AI;
using ChallengeReviewer.Core.Agents.Abstractions;
using ChallengeReviewer.Core.Enums;
using ChallengeReviewer.Core.Extensions;
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

        public ChallengeReviewerPipeline CollectInputs()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            _repositoryUrl = AnsiConsole.Ask<string>("Enter the GitHub repository [green]url[/] to review:?");
            _includeCodeReview = AnsiConsole.Confirm("Include a code review?");
            _provider = AnsiConsole.Prompt(
                new SelectionPrompt<Provider>()
                    .Title("Which [green]AI provider[/] should I use:")
                    .AddChoices(Provider.Ollama, Provider.OpenAi));

            _model = AnsiConsole.Prompt(
                 new SelectionPrompt<Model>()
                .Title("Which [green]model[/] should I use:")
                .AddChoices(Model.Qwen25Coder7B, Model.Gemma4, Model.Gpt4Omini));
            return this;
        }

        public ChallengeReviewerPipeline ConfirmAndStart()
        {
            var summary = new Table()
                .RoundedBorder()
                .BorderColor(Color.Grey)
                .AddColumn("Prompt")
                .AddColumn("Answer")
                .AddRow("URL", _repositoryUrl)
                .AddRow("Review Code", _includeCodeReview ? "Yes" : "No")
                .AddRow("Provider", _provider.Description())
                .AddRow("Model", _model.Description());

            AnsiConsole.Write(summary);
            AnsiConsole.WriteLine();

            if (AnsiConsole.Confirm("Is this correct?"))
                return this;

            AnsiConsole.Markup("[red]Review cancelled. Goodbye![/]");
            Environment.Exit(0);
            return this;
        }

        public async Task<ChallengeReviewerPipeline> FetchRepositoryAsync(CancellationToken cancellationToken = default)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[yellow]GitHub Fetching[/]"));
            AnsiConsole.WriteLine();

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots) 
                .StartAsync($"Fetching GitHub repository for URL [green]{_repositoryUrl}[/]...", async ctx =>
                {
                    _repository = await _gitHubService.FetchRepositoryAsync(_repositoryUrl, cancellationToken);

                    if (string.IsNullOrEmpty(_repository.Title))
                    {
                        AnsiConsole.MarkupLine($"[red]Failed to fetch repository for URL {_repositoryUrl}[/]");
                        Environment.Exit(0);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"Repository: [green]{_repository.Title}[/]");
                        AnsiConsole.MarkupLine($"Author: [green]{_repository.Owner}[/]");
                    }
                }); 

            return this;
        }
    }
}
