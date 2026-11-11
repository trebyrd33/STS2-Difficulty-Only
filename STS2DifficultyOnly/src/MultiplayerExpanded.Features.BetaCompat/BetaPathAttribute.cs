using System;

namespace MultiplayerExpanded.Features.BetaCompat;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
public sealed class BetaPathAttribute : Attribute
{
}
