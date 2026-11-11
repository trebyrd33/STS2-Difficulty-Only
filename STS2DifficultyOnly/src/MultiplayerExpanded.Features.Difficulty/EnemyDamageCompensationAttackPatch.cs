using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace MultiplayerExpanded.Features.Difficulty;

[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class EnemyDamageCompensationAttackPatch
{
	private static readonly FieldInfo? DamagePerHitField = AccessTools.Field(typeof(AttackCommand), "_damagePerHit");

	private static readonly AccessTools.FieldRef<AttackCommand, decimal>? DamagePerHit = ((DamagePerHitField == null) ? null : AccessTools.FieldRefAccess<AttackCommand, decimal>(DamagePerHitField));

	private static readonly ConditionalWeakTable<AttackCommand, object> Compensated = new ConditionalWeakTable<AttackCommand, object>();

	private static bool Prepare()
	{
		if (DamagePerHit != null)
		{
			return true;
		}
		MainFile.Logger.Error("[Difficulty] AttackCommand._damagePerHit not found; enemy damage compensation is off.", 1);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(AttackCommand __instance)
	{
		Creature attacker = __instance.Attacker;
		if (attacker != null && (int)attacker.Side == 2 && attacker.Monster != null && !Compensated.TryGetValue(__instance, out object _))
		{
			Compensated.Add(__instance, attacker);
			ref decimal reference = ref DamagePerHit.Invoke(__instance);
			reference = ChallengeOptions.CompensatedBaseDamage(reference, attacker.CombatState);
		}
	}
}
