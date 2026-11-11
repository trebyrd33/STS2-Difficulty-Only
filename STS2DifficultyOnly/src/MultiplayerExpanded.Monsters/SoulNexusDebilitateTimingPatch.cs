using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(DebilitatePower), "AfterSideTurnEnd")]
internal static class SoulNexusDebilitateTimingPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(DebilitatePower __instance, CombatSide side, ref Task __result)
	{
		if (!MonsterRework.IsMultiplayer(((PowerModel)__instance).Owner.CombatState))
		{
			return true;
		}
		if ((int)((PowerModel)__instance).Owner.Side != 1)
		{
			return true;
		}
		__result = (((int)side == 2) ? PowerCmd.TickDownDuration((PowerModel)(object)__instance) : Task.CompletedTask);
		return false;
	}
}
