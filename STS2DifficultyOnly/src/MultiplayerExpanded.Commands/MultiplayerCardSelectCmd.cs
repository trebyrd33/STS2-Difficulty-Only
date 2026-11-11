using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Commands;

public static class MultiplayerCardSelectCmd
{
	private sealed record SelectionSource(Func<Player, List<CardModel>> Pile, Func<List<CardModel>, Task<IEnumerable<CardModel>>> Show, bool InCombat);

	private static bool LocalPlayerIsChoosing
	{
		get
		{
			NPlayerHand instance = NPlayerHand.Instance;
			if (instance == null)
			{
				return false;
			}
			return instance.IsInCardSelection;
		}
	}

	private static bool ShouldSelectLocalCard(Player player)
	{
		if (LocalContext.IsMe(player))
		{
			return (int)RunManager.Instance.NetService.Type != 4;
		}
		return false;
	}

	public static Task<IReadOnlyList<(Player Player, IReadOnlyList<CardModel> Cards)>> FromHandAllPlayers(PlayerChoiceContext context, IReadOnlyList<Player> players, CardSelectorPrefs prefs, Func<CardModel, bool>? filter, AbstractModel source)
	{
		return FromAllPlayers(context, players, prefs, new SelectionSource((Player player) => PileTypeExtensions.GetPile((PileType)2, player).Cards.Where(filter ?? ((Func<CardModel, bool>)((CardModel _) => true))).ToList(), (List<CardModel> _) =>
		{
			return NCombatRoom.Instance.Ui.Hand.SelectCards(prefs, filter, source, (MegaCrit.Sts2.Core.Nodes.Combat.NPlayerHand.Mode)2);
		}, InCombat: true));
	}

	public static Task<IReadOnlyList<(Player Player, IReadOnlyList<CardModel> Cards)>> FromDiscardAllPlayers(PlayerChoiceContext context, IReadOnlyList<Player> players, CardSelectorPrefs prefs, AbstractModel source)
	{
		return FromAllPlayers(context, players, prefs, new SelectionSource((Player player) => PileTypeExtensions.GetPile((PileType)3, player).Cards.ToList(), (List<CardModel> cards) =>
		{
			return ShowGrid(cards, prefs);
		}, InCombat: true));
	}

	public static async Task<IReadOnlyList<(Player Player, CardModel? Card)>> FromDeckAllPlayers(PlayerChoiceContext context, IReadOnlyList<Player> players, CardSelectorPrefs prefs, Func<CardModel, bool>? filter)
	{
		return (await FromAllPlayers(context, players, prefs, new SelectionSource((Player player) => PileTypeExtensions.GetPile((PileType)6, player).Cards.Where(filter ?? ((Func<CardModel, bool>)((CardModel _) => true))).ToList(), (List<CardModel> cards) =>
		{
			return ShowGrid(cards, prefs);
		}, InCombat: false))).Select(((Player Player, IReadOnlyList<CardModel> Cards) s) => (Player: s.Player, Card: s.Cards.FirstOrDefault())).ToArray();
	}

	private static async Task<IReadOnlyList<(Player Player, IReadOnlyList<CardModel> Cards)>> FromAllPlayers(PlayerChoiceContext context, IReadOnlyList<Player> players, CardSelectorPrefs prefs, SelectionSource source)
	{
		if (source.InCombat && CombatManager.Instance.IsOverOrEnding)
		{
			return players.Select((Player p) => ((Player p, IReadOnlyList<CardModel>))(p: p, Array.Empty<CardModel>())).ToArray();
		}
		Dictionary<Player, uint> choiceIds = players.ToDictionary((Player p) => p, (Player p) => RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(p));
		await context.SignalPlayerChoiceBegunAsOwner(players, (PlayerChoiceOptions)(source.InCombat ? 1 : 0));
		(Player player, IReadOnlyList<CardModel> result)[] allResults = await Task.WhenAll(players.Select(async (Player player) =>
		{
			List<CardModel> pile = source.Pile(player);
			IReadOnlyList<CardModel> readOnlyList;
			if (ShouldSelectLocalCard(player))
			{
				if (source.InCombat)
				{
					NPlayerHand instance = NPlayerHand.Instance;
					if (instance != null)
					{
						instance.CancelAllCardPlay();
					}
				}
				if (pile.Count == 0 || (!prefs.RequireManualConfirmation && pile.Count <= prefs.MinSelect))
				{
					readOnlyList = pile;
				}
				else if (CardSelectCmd.Selector != null)
				{
					readOnlyList = (await CardSelectCmd.Selector.GetSelectedCards((IEnumerable<CardModel>)pile, prefs.MinSelect, prefs.MaxSelect)).ToList();
				}
				else
				{
					try
					{
						DismissBlockingScreens();
						await WaitForLocalSelectionToFinish();
						readOnlyList = (await source.Show(pile)).ToList();
					}
					catch (Exception value)
					{
						MainFile.Logger.Error($"[MultiplayerCardSelect] Selector failed for player {player.NetId}; syncing an empty choice so the other clients are not left waiting: {value}", 1);
						readOnlyList = Array.Empty<CardModel>();
					}
				}
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceIds[player], Encode(readOnlyList, pile, source.InCombat));
			}
			else
			{
				readOnlyList = Decode(await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceIds[player]), pile, source.InCombat);
			}
			return (player: player, result: readOnlyList);
		}));
		await context.SignalPlayerChoiceEnded();
		return allResults;
	}

	private static void DismissBlockingScreens()
	{
		if (!TestMode.IsOn)
		{
			NCapstoneContainer instance = NCapstoneContainer.Instance;
			if (instance != null)
			{
				instance.Close();
			}
			NMapScreen instance2 = NMapScreen.Instance;
			if (instance2 != null)
			{
				instance2.Close(false);
			}
		}
	}

	private static async Task WaitForLocalSelectionToFinish()
	{
		NRun run = NRun.Instance;
		if (TestMode.IsOn || run == null || !LocalPlayerIsChoosing)
		{
			return;
		}
		MainFile.Logger.Info("[MultiplayerCardSelect] Local player is mid-selection; holding the team pick until they finish.", 1);
		int frames = 0;
		while (LocalPlayerIsChoosing)
		{
			if (frames == 600)
			{
				MainFile.Logger.Warn("[MultiplayerCardSelect] Still waiting on the local player's own selection after ~10s.", 1);
			}
			await NodeUtil.AwaitProcessFrame((Node)(object)run, default(CancellationToken));
			frames++;
		}
	}

	private static async Task<IEnumerable<CardModel>> ShowGrid(List<CardModel> cards, CardSelectorPrefs prefs)
	{
		NSimpleCardSelectScreen val = NSimpleCardSelectScreen.Create((IReadOnlyList<CardModel>)cards, prefs);
		NOverlayStack.Instance.Push((IOverlayScreen)(object)val);
		return await ((NCardGridSelectionScreen)val).CardsSelected();
	}

	private static PlayerChoiceResult Encode(IReadOnlyList<CardModel> chosen, List<CardModel> pile, bool inCombat)
	{
		if (!inCombat)
		{
			return PlayerChoiceResult.FromIndexes((from c in chosen
				select pile.IndexOf(c) into i
				where i >= 0
				select i).ToList());
		}
		return PlayerChoiceResult.FromMutableCombatCards((IEnumerable<CardModel>)chosen);
	}

	private static IReadOnlyList<CardModel> Decode(PlayerChoiceResult result, List<CardModel> pile, bool inCombat)
	{
		if (inCombat)
		{
			return result.AsCombatCards().ToList();
		}
		return (from i in result.AsIndexes()
			where i >= 0 && i < pile.Count
			select pile[i]).ToList();
	}
}
