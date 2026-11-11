using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace MultiplayerExpanded.Features.Difficulty;

[HarmonyPatch(typeof(AttackIntent), "GetSingleDamage")]
internal static class EnemyDamageCompensationIntentPatch
{
	private static readonly MethodInfo Invoke = AccessTools.Method(typeof(Func<decimal>), "Invoke", (Type[])null, (Type[])null);

	private static readonly MethodInfo Compensate = AccessTools.Method(typeof(EnemyDamageCompensationIntentPatch), "Raise", (Type[])null, (Type[])null);

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int injected = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			yield return instruction;
			if (CodeInstructionExtensions.Calls(instruction, Invoke))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
				yield return new CodeInstruction(OpCodes.Call, (object)Compensate);
				injected++;
			}
		}
		if (injected == 0)
		{
			MainFile.Logger.Error("[Difficulty] AttackIntent.GetSingleDamage: no DamageCalc call found; intents show uncompensated enemy damage.", 1);
		}
	}

	private static decimal Raise(decimal damage, Creature owner)
	{
		return ChallengeOptions.CompensatedBaseDamage(damage, owner.CombatState);
	}
}
