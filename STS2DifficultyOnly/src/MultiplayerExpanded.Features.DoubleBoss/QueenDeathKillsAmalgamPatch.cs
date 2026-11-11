using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MultiplayerExpanded.Features.DoubleBoss;

[HarmonyPatch(typeof(Queen), "AfterDeath")]
internal static class QueenDeathKillsAmalgamPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(Queen __instance, Creature creature, ref Task __result)
	{
		if (creature != ((MonsterModel)__instance).Creature)
		{
			return;
		}
		ICombatState combatState = ((MonsterModel)__instance).CombatState;
		if (combatState?.Encounter is DoubleBossEncounter)
		{
			Creature val = combatState.Enemies.FirstOrDefault((Creature e) => e.IsAlive && e.Monster is TorchHeadAmalgam);
			if (val != null && combatState.Enemies.Any((Creature e) => e != creature && e.IsAlive && e.IsPrimaryEnemy))
			{
				__result = KillAfter(__result, val);
			}
		}
	}

	private static async Task KillAfter(Task vanilla, Creature amalgam)
	{
		await vanilla;
		if (amalgam.IsAlive)
		{
			await CreatureCmd.Kill(amalgam, false);
		}
	}
}
