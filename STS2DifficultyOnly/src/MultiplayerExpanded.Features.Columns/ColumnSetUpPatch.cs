using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;

namespace MultiplayerExpanded.Features.Columns;

[HarmonyPatch(typeof(Hook), "BeforeCombatStart")]
internal static class ColumnSetUpPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix(ICombatState? combatState, ref Task __result)
	{
		if (combatState != null)
		{
			IColumnEncounter columnEncounter = ColumnCombat.Of(combatState);
			if (columnEncounter != null)
			{
				__result = After(__result, combatState, columnEncounter);
			}
		}
	}

	private static async Task After(Task vanilla, ICombatState state, IColumnEncounter encounter)
	{
		await vanilla;
		await ColumnCombat.SetUp(state, encounter);
	}
}
