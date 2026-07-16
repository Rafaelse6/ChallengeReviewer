namespace ChallengeReviewer.Core
{
    public abstract class Configuration
    {
        public abstract class Ollama
        {
            public static string Url { get; set; } = string.Empty;
        }

        public abstract class OpenAI
        {
            public static string ApiKey { get; set; } = string.Empty;
        }
    }
}
