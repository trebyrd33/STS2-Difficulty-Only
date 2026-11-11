using System;

namespace MultiplayerExpanded.Features.BetaCompat;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BetaOnlyAttribute : Attribute
{
}
