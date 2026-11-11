using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Intents;

public static class AttackExclusion
{
	private static readonly ConditionalWeakTable<AttackCommand, Creature> Excluded = new ConditionalWeakTable<AttackCommand, Creature>();

	public static AttackCommand FromMonsterExcept(this AttackCommand attack, MonsterModel monster, Creature excluded)
	{
		attack.FromMonster(monster);
		Excluded.AddOrUpdate(attack, excluded);
		AttackOriginalTargets.Declare(attack, monster.Creature.CombatState.PlayerCreatures.Where((Creature c) => c != excluded));
		return attack;
	}

	internal static bool TryGetExcluded(AttackCommand attack, out Creature excluded)
	{
		return Excluded.TryGetValue(attack, out excluded);
	}
}
