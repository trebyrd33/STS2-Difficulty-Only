using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.Reworks;

internal static class ReworkStateSync
{
	private static readonly AccessTools.FieldRef<CardModel, int>? UpgradeLevelField = ((AccessTools.DeclaredField(typeof(CardModel), "_currentUpgradeLevel") != null) ? AccessTools.FieldRefAccess<CardModel, int>("_currentUpgradeLevel") : null);

	internal static int RebakeTemplates()
	{
		int num = 0;
		foreach (CardModel allCard in ModelDb.AllCards)
		{
			if (ReworkRegistry.TryGet(((object)allCard).GetType(), out ReworkInfo _))
			{
				ReworkAfterCreatedPatch.Rebuild(allCard);
				num++;
			}
		}
		return num;
	}

	internal static int RebakeRunCards(RunState? state)
	{
		if (state == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Player player in state.Players)
		{
			foreach (CardModel card in player.Deck.Cards)
			{
				if (ReworkRegistry.TryGet(((object)card).GetType(), out ReworkInfo _))
				{
					RebakeCard(card);
					num++;
				}
			}
		}
		return num;
	}

	private static void RebakeCard(CardModel card)
	{
		int num = Math.Max(0, card.CurrentUpgradeLevel);
		try
		{
			card.DowngradeInternal();
			if (card.CurrentUpgradeLevel != 0)
			{
				SetUpgradeLevel(card, 0);
			}
			for (int i = 0; i < num; i++)
			{
				card.UpgradeInternal();
				card.FinalizeUpgradeInternal();
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[Reworks] re-baking {((AbstractModel)card).Id} (upgrade level {num}) failed: {value}", 1);
		}
		if (card.CurrentUpgradeLevel != num)
		{
			MainFile.Logger.Warn($"[Reworks] {((AbstractModel)card).Id} came out of the re-bake at upgrade level {card.CurrentUpgradeLevel} instead of {num}; restoring it.", 1);
			SetUpgradeLevel(card, num);
		}
	}

	private static void SetUpgradeLevel(CardModel card, int level)
	{
		if (UpgradeLevelField != null)
		{
			UpgradeLevelField.Invoke(card) = level;
		}
		else
		{
			MainFile.Logger.Error("[Reworks] CardModel._currentUpgradeLevel not found; cannot correct the upgrade level.", 1);
		}
	}
}
