using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Patches;


namespace MultiplayerExpanded.Features.Columns;

internal static class ColumnAttacks
{
	private static readonly ConditionalWeakTable<AttackCommand, Creature[]> Hittable = new ConditionalWeakTable<AttackCommand, Creature[]>();

	internal static void Aim(AttackCommand attack)
	{
		if (attack.IsSingleTargeted)
		{
			return;
		}
		Creature attacker = attack.Attacker;
		if (attacker != null && (int)attacker.Side == 2 && !AttackOriginalTargets.IsDeclared(attack) && !AttackExclusion.TryGetExcluded(attack, out Creature _))
		{
			IReadOnlyList<Creature> readOnlyList = ColumnCombat.TargetsOf(attacker);
			if (readOnlyList != null)
			{
				AttackOriginalTargets.Declare(attack, readOnlyList);
				Hittable.AddOrUpdate(attack, HitBy(attacker, readOnlyList));
			}
		}
	}

	internal static Creature[] HitBy(Creature dealer, IReadOnlyList<Creature> column)
	{
		IReadOnlyList<Creature> playerCreatures = dealer.CombatState.PlayerCreatures;
		IEnumerable<Creature> second = column.Select(InterceptMpRedirectDamagePatch.CovererOf).OfType<Creature>();
		return column.Concat(second).Distinct().ToArray();
	}

	internal static bool TryGetHittable(AttackCommand attack, out Creature[] hittable)
	{
		return Hittable.TryGetValue(attack, out hittable);
	}

	internal static bool Reaches(IReadOnlyList<Creature> column, Creature player)
	{
		if (!column.Contains(player) || SharesMovedAway(player))
		{
			return column.Any((Creature c) => c != player && (InterceptMpRedirectDamagePatch.CovererOf(c) == player));
		}
		return true;
	}

	private static bool SharesMovedAway(Creature player)
	{
		if (InterceptMpRedirectDamagePatch.CovererOf(player) == null)
		{
			return false;
		}
		return true;
	}
}
