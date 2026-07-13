using System.ComponentModel;

namespace ChallengeReviewer.Core.Enums;

public enum Model
{
    [Description("gemma4")]
    Gemma4 = 0,

    [Description("qwen2.5-coder:7b")]
    Qwen25Coder7B = 1,

    [Description("gpt-4o-mini")]
    Gpt40mini = 2
}