using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace MultiplayerExpanded.Features.DoubleBoss;

[HarmonyPatch]
internal static class DoubleBossVictoryMusicPatch
{
	private const float VictoryProgress = 5f;

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new Type[3]
		{
			typeof(Queen),
			typeof(TestSubject),
			typeof(Aeonglass)
		}.Select((Type t) => AccessTools.DeclaredMethod(t, "AfterDeath", (Type[])null, (Type[])null));
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
	{
		MethodInfo update = AccessTools.Method(typeof(NRunMusicController), "UpdateMusicParameter", (Type[])null, (Type[])null);
		MethodInfo redirect = AccessTools.Method(typeof(DoubleBossVictoryMusicPatch), "Redirect", (Type[])null, (Type[])null);
		int injected = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (CodeInstructionExtensions.Calls(instruction, update))
			{
				yield return CodeInstructionExtensions.MoveBlocksFrom(CodeInstructionExtensions.MoveLabelsFrom(new CodeInstruction(OpCodes.Ldarg_0, (object)null), instruction), instruction);
				yield return new CodeInstruction(OpCodes.Call, (object)redirect);
				injected++;
			}
			else
			{
				yield return instruction;
			}
		}
		if (injected == 0)
		{
			MainFile.Logger.Error("[DoubleBoss] no music call in " + original.DeclaringType?.Name + ".AfterDeath — killing one boss will play the victory music. The method's IL likely changed in a game update.", 1);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Redirect(NRunMusicController music, string label, float progress, MonsterModel monster)
	{
		if (progress == 5f)
		{
			ICombatState combatState = monster.Creature.CombatState;
			if (combatState != null && combatState.Encounter is DoubleBossEncounter doubleBossEncounter)
			{
				if (AnotherBossStanding(combatState, monster))
				{
					return;
				}
				label = doubleBossEncounter.MusicProgressParameter;
			}
		}
		music.UpdateMusicParameter(label, progress);
	}

	private static bool AnotherBossStanding(ICombatState state, MonsterModel monster)
	{
		return state.Enemies.Any((Creature e) => e != monster.Creature && e.IsPrimaryEnemy && (e.IsAlive || e.HasPower<AdaptablePower>()));
	}
}
