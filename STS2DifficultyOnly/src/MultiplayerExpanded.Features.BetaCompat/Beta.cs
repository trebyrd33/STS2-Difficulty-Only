using MegaCrit.Sts2.Core.Combat;

namespace MultiplayerExpanded.Features.BetaCompat;

public static class Beta
{
	public static bool IsOn { get; } = typeof(CombatState).Assembly.GetType("MegaCrit.Sts2.Core.Combat.CombatTurnState") != null;

	public static T Pick<T>(T stable, T beta)
	{
		if (!IsOn)
		{
			return stable;
		}
		return beta;
	}
}
