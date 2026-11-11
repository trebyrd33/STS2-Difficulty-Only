using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(EnragePower), "AfterCardPlayed")]
internal static class TestSubjectEnrageAttributionPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(EnragePower __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		if (!MonsterRework.IsMultiplayer(((PowerModel)__instance).Owner.CombatState))
		{
			return true;
		}
		if (((cardPlay != null) ? cardPlay.Card : null) == null || (int)cardPlay.Card.Type != 2)
		{
			return true;
		}
		Player owner = cardPlay.Card.Owner;
		Creature val = ((owner != null) ? owner.Creature : null);
		if (val == null || !val.IsPlayer)
		{
			return true;
		}
		__result = EnrageAndAttribute(__instance, choiceContext, val);
		return false;
	}

	private static async Task EnrageAndAttribute(EnragePower enrage, PlayerChoiceContext choiceContext, Creature contributor)
	{
		await Cmd.Wait(0.5f, false);
		Creature boss = ((PowerModel)enrage).Owner;
		int amount = ((PowerModel)enrage).Amount;
		bool willLand = !CombatManager.Instance.IsEnding && boss.CanReceivePowers;
		await PowerCmd.Apply<StrengthPower>(choiceContext, boss, (decimal)amount, boss, (CardModel)null, false);
		if (!willLand || amount <= 0)
		{
			return;
		}
		try
		{
			await PowerCmd.Apply<EnrageContributionPower>(choiceContext, boss, (decimal)amount, contributor, (CardModel)null, true);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[EnrageAttribution] tracking failed: " + ex.Message, 1);
		}
	}
}
