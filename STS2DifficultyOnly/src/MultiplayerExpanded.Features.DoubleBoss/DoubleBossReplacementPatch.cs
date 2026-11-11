using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DoubleBoss;

[HarmonyPatch(typeof(RunManager), "GenerateRooms")]
internal static class DoubleBossReplacementPatch
{
	private static readonly PropertyInfo? StateProperty = AccessTools.Property(typeof(RunManager), "State");

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix(RunManager __instance)
	{
		if (!MultiplayerExpandedConfig.SurroundedByBosses)
		{
			return;
		}
		object? obj = StateProperty?.GetValue(__instance);
		RunState val = (RunState)((obj is RunState) ? obj : null);
		if (val == null)
		{
			MainFile.Logger.Error("[DoubleBoss] RunManager.State not found — A10 keeps vanilla's back-to-back bosses.", 1);
		}
		else
		{
			if (!MultiplayerExpanded.Features.Difficulty.DifficultyRules.CombineBosses(true, val.AscensionLevel) || val.Acts.Count < 3)
			{
				return;
			}
			IReadOnlyList<ActModel> acts = val.Acts;
			ActModel val2 = acts[acts.Count - 1];
			EncounterModel secondBossEncounter = val2.SecondBossEncounter;
			if (secondBossEncounter != null)
			{
				DoubleBossEncounter doubleBossEncounter = DoubleBossEncounter.For(val2.BossEncounter, secondBossEncounter);
				if (doubleBossEncounter == null)
				{
					MainFile.Logger.Warn($"[DoubleBoss] no pair for {((AbstractModel)val2.BossEncounter).Id} + {((AbstractModel)secondBossEncounter).Id}; keeping two fights.", 1);
				}
				else
				{
					val2.SetBossEncounter((EncounterModel)(object)doubleBossEncounter);
					val2.SetSecondBossEncounter((EncounterModel)null);
					MainFile.Logger.Info($"[DoubleBoss] final act boss is {((AbstractModel)doubleBossEncounter).Id}.", 1);
				}
			}
		}
	}
}
