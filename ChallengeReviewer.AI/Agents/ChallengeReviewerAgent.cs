using ChallengeReviewer.Core.Enums;
using ChallengeReviewer.Core.Models;

namespace ChallengeReviewer.AI.Agents
{
    public class ChallengeReviewerAgent()
        : Agent<StaticAnalysisReport, Review>(Name, Description, Prompt, Temperature, Instructions)
    {
        private const float Temperature = 0.7f;
        private const string Name = "Challenge Reviewer";
        private const string Description =
            "Você é um agente especializado em gerar relatórios técnicos detalhados com base em análises estáticas de código";
        private const string Instructions =
            "você é um agente especialista em gerar relatórios técnicos detalhados com base em análises estáticas de código."
            + "Use os dados fornecdiso para criar um relatório que destaque os principais pontos, "
            + "incluindo métricas, problemas encontrados e sugestões de melhoria."
            + "O relatório deve ser claro, conciso e fácil de entender para desenvolvedores e gerentes de projeto.";

        private const string Prompt = "Gere um relatório técnico detalhado com base nestes dados: ";

        //public override async Task<Review> RunAsync(
        //    StaticAnalysisReport data, 
        //    Provider provider, Model model, 
        //    CancellationToken cancellationToken)
        //{
        //    return await base.RunAsync(data, provider, model, cancellationToken);
        //}
    }
}
