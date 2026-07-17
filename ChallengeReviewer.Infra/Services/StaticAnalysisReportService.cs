using ChallengeReviewer.Core.Enums;
using ChallengeReviewer.Core.Models;
using ChallengeReviewer.Core.Models.GitHub;
using ChallengeReviewer.Core.Services.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChallengeReviewer.Infra.Services
{
    public class StaticAnalysisReportService : IStaticAnalysisReportService
    {
        private const int MaxClassLines = 200;
        private const int MaxMethodLines = 50;
        private const int MaxParameters = 5;

        public StaticAnalysisReport AnalyzeCode(GitHubRepository repoContent)
        {
            var issues = new List<Core.Models.Issue>();
            int totalLines = 0;
            var totalClasses = 0;
            var totalMethods = 0;

            foreach (var file in repoContent.Files)
            {
                var tree = CSharpSyntaxTree.ParseText(file.Content);
                var root = tree.GetRoot();
                var lines = file.Content.Split('\n').Length;
                totalLines += lines;

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
                totalClasses += classes.Count;
                totalMethods += methods.Count;

                issues.AddRange(AnalyzeClasses(file.Path, tree, classes));
                issues.AddRange(AnalyzeMethods(file.Path, tree, methods));
                issues.AddRange(AnalyzeCatches(file.Path, tree, root));
                issues.AddRange(AnalyzeFields(file.Path, tree, root));
                issues.AddRange(AnalyzeNaming(file.Path, tree, root));
            }

            return new StaticAnalysisReport(
                repoContent.Files.Count,
                totalLines,
                totalClasses,
                totalMethods,
                issues
            );
        }

        private static IEnumerable<Issue> AnalyzeClasses(
            string path,
            SyntaxTree tree,
            List<ClassDeclarationSyntax> classes)
        {
            foreach (var cls in classes)
            {
                var span = cls.GetLocation().GetLineSpan();
                var start = span.StartLinePosition.Line;
                var end = span.EndLinePosition.Line;
                var size = end - start + 1;
                var line = start + 1;

                if (size > MaxClassLines)
                {
                    yield return Issue(path, tree, cls,
                        "CLASS_TOO_LARGE",
                        $"Classe '{cls.Identifier.Text}' tem {size} linhas (máx {MaxClassLines}).",
                        IssueSeverity.Warning);
                }

                // Classe sem modificador de acesso explícito
                var hasAccessModifier = cls.Modifiers.Any(m =>
                    m.IsKind(SyntaxKind.PublicKeyword) ||
                    m.IsKind(SyntaxKind.InternalKeyword) ||
                    m.IsKind(SyntaxKind.PrivateKeyword));

                if (!hasAccessModifier)
                    yield return Issue(path, tree, cls,
                        "MISSING_ACCESS_MODIFIER",
                        $"Classe '{cls.Identifier.Text}' sem modificador de acesso explícito.",
                        IssueSeverity.Info);


            }
        }

        private static IEnumerable<Issue> AnalyzeMethods(
            string path,
            SyntaxTree tree,
            List<MethodDeclarationSyntax> methods)
        {
            foreach (var method in methods)
            {
                var name = method.Identifier.Text;
                var span = method.GetLocation().GetLineSpan();
                var size = span.EndLinePosition.Line - span.StartLinePosition.Line + 1;

                // Tamanho
                if (size > MaxMethodLines)
                    yield return Issue(path, tree, method,
                        "METHOD_TOO_LARGE",
                        $"Método '{name}' tem {size} linhas (máx {MaxMethodLines}).",
                        IssueSeverity.Warning);

                // Parâmetros em excesso
                var paramCount = method.ParameterList.Parameters.Count;
                if (paramCount > MaxParameters)
                    yield return Issue(path, tree, method,
                        "TOO_MANY_PARAMETERS",
                        $"Método '{name}' tem {paramCount} parâmetros (máx {MaxParameters}).",
                        IssueSeverity.Warning);

                // async void (perigoso - engole exceções)
                var isAsync = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
                var returnsVoid = method.ReturnType is PredefinedTypeSyntax pts &&
                    pts.Keyword.IsKind(SyntaxKind.VoidKeyword);

                if (isAsync && returnsVoid && !IsEventHandler(method))
                    yield return Issue(path, tree, method,
                        "ASYNC_VOID",
                        $"Método '{name}' é async void - use async Task.",
                        IssueSeverity.Error);

                // Método async sem sufixo Async
                if (isAsync && !name.EndsWith("Async") && !IsEventHandler(method))
                    yield return Issue(path, tree, method,
                        "MISSING_ASYNC_SUFFIX",
                        $"Método assíncrono '{name}' deveria terminar em 'Async'.",
                        IssueSeverity.Warning);

                // .Result ou .Wait() - deadlock em contextos sincronizados
                var resultAccesses = method.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Where(m => m.Name.Identifier.Text is "Result" or "Wait");

                foreach (var access in resultAccesses)
                    yield return Issue(path, tree, access,
                        "BLOCKING_ASYNC",
                        $"Uso de '.{access.Name.Identifier.Text}' em '{name}' pode causar deadlock.",
                        IssueSeverity.Error);

            }
        }

        private static IEnumerable<Issue> AnalyzeCatches(
            string path, SyntaxTree tree, SyntaxNode root)
        {
            var catches = root.DescendantNodes().OfType<CatchClauseSyntax>();

            foreach (var clause in catches)
            {
                // Catch vazio (sem body útil)
                var statements = clause.Block.Statements;
                if (!statements.Any())
                    yield return Issue(path, tree, clause,
                        "EMPTY_CATCH",
                        "Bloco catch vazio - exceção silenciada.",
                        IssueSeverity.Error);

                // Catch genérico (catch Exception)
                var typeName = clause.Declaration?.Type.ToString();
                if (typeName is "Exception" or "System.Exception")
                    yield return Issue(path, tree, clause,
                        "GENERIC_CATCH",
                        "Catch de 'Exception' genérico - prefira exceções específicas.",
                        IssueSeverity.Warning);
            }
        }

        private static IEnumerable<Issue> AnalyzeFields(
            string path, SyntaxTree tree, SyntaxNode root)
        {
            var fields = root.DescendantNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                var isPublic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
                var isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
                var isReadonly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

                if (isPublic && !isConst && !isReadonly)
                    yield return Issue(path, tree, field,
                        "PUBLIC_FIELD",
                        $"Campo público '{field.Declaration.Variables.First().Identifier.Text}' – use propriedades.",
                        IssueSeverity.Warning);
            } 
        }

        private static IEnumerable<Issue> AnalyzeNaming(
           string path, SyntaxTree tree, SyntaxNode root)
        {
            // Classes com inicial minúscula
            foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var name = cls.Identifier.Text;
                if (char.IsLower(name[0]))
                    yield return Issue(path, tree, cls,
                        "NAMING_CLASS",
                        $"Classe '{name}' deve começar com letra maiúscula (PascalCase).",
                        IssueSeverity.Error);
            }

            // Métodos com inicial minúscula
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var name = method.Identifier.Text;
                if (char.IsLower(name[0]))
                    yield return Issue(path, tree, method,
                        "NAMING_METHOD",
                        $"Método '{name}' deve começar com letra maiúscula (PascalCase).",
                        IssueSeverity.Error);
            }
        }

        private static Issue Issue(
            string path,
            SyntaxTree tree,
            SyntaxNode node,
            string rule,
            string message,
            IssueSeverity severity)
        {
            var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            return new Issue(path, rule, message, line, severity);
        }

        private static bool IsEventHandler(MethodDeclarationSyntax method)
        {
            var parameters = method.ParameterList.Parameters;
            if (parameters.Count != 2) return false;

            var second = parameters[1].Type?.ToString() ?? "";
            return second.Contains("EventArgs");
        }

    }
}
