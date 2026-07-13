using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection; 

namespace ChallengeReviewer.Core.Extensions;

public static class EnumExtension 
{
    private static readonly ConcurrentDictionary<Enum, string> DescriptionCache = new();

    public static string Description<T>(this T value) where T : struct, Enum
    {
        return DescriptionCache.GetOrAdd(value, v =>
        {
            var field = v.GetType().GetField(v.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? v.ToString();
        });
    }
}
