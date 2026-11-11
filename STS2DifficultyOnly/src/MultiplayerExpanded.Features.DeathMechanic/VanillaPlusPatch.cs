using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MultiplayerExpanded.Cards.Curses;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(Player), "ReviveBeforeCombatEnd")]
internal static class VanillaPlusPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(Player __instance, out bool __state)
	{
		__state = __instance.Creature.IsDead;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(Player __instance, bool __state, ref Task __result)
	{
		if (__state && DeathMechanicGate.IsActive(MultiplayerDeathMode.VanillaPlus, __instance.RunState))
		{
			__result = ReviveThenAddCurse(__result, __instance);
		}
	}

	private static async Task ReviveThenAddCurse(Task revive, Player player)
	{
		await revive;
		try
		{
			await CardPileCmd.AddCurseToDeck<NeowSClutch>(player);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[DeathMechanic] Vanilla+ curse add failed: " + ex.Message, 1);
		}
	}
}
