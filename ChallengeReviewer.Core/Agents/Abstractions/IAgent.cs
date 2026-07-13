namespace ChallengeReviewer.Core.Agents.Abstractions;

public interface IAgent<in TData, TResponse>
    where TData : class
    where TResponse : class
{
    Task<TResponse> RunAsync(TData data, string Provider, string Model, CancellationToken cancellationToken);
}