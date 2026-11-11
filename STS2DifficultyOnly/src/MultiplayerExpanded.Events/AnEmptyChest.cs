using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MultiplayerExpanded.Commands;
using MultiplayerExpanded.Extensions;
using MultiplayerExpanded.Features.CardChest;

namespace MultiplayerExpanded.Events;

public sealed class AnEmptyChest : MultiplayerExpandedEvent
{
	private const string InitialPage = "INITIAL";

	private const int MinEligibleCards = 2;

	public override bool IsShared => true;

	public override string? CustomInitialPortraitPath
	{
		get
		{
			Player owner = ((EventModel)this).Owner;
			return ActPortraitPath((owner != null) ? owner.RunState : null);
		}
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (base.IsAllowed(runState))
		{
			RunState val = (RunState)(object)((runState is RunState) ? runState : null);
			if (val != null && !val.VisitedEventIds.Contains(((AbstractModel)ModelDb.Event<ColorfulPhilosophers>()).Id) && val.VisitedEventIds.Any((ModelId id) => id != ((AbstractModel)this).Id))
			{
				List<Player> list = MultiplayerExpandedEvent.LivingPlayers(runState);
				if (list != null && list.Count >= 2)
				{
					return list.All((Player player) => Enumerable.Count(PileTypeExtensions.GetPile((PileType)6, player).Cards, IsEligible) >= 2);
				}
			}
		}
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<EventOption>(this.Option((Func<Task>)Give, "INITIAL", Array.Empty<IHoverTip>()));
	}

	public override IEnumerable<string> GetAssetPaths(IRunState runState)
	{
		string wanted = ActPortraitPath(runState);
		string ownerless = ActPortraitPath(null);
		return from path in base.GetAssetPaths(runState)
			select (!(path == ownerless)) ? path : wanted;
	}

	private string ActPortraitPath(IRunState? runState)
	{
		return $"{Slug}_act_{((runState != null) ? runState.CurrentActIndex : 0) + 1}.png".EventImagePath();
	}

	private static bool IsEligible(CardModel card)
	{
		bool flag = card.Enchantment == null;
		if (flag)
		{
			CardRarity rarity = card.Rarity;
			bool flag2 = (uint)((int)rarity - 2) <= 2u;
			flag = flag2;
		}
		return flag;
	}

	private async Task Give()
	{
		if (MultiplayerExpandedEvent.LivingPlayers(((EventModel)this).Owner.RunState).Count < 2)
		{
			this.SetEventFinished(this.PageDescription("NO_ALLIES"));
			return;
		}
		if (RunsPartyWideStep)
		{
			await RunTheChest();
		}
		this.SetEventFinished(((EventModel)this).InitialDescription);
		if (LocalContext.IsMe(((EventModel)this).Owner))
		{
			await ProceedToMap();
		}
	}

	private static Task ProceedToMap()
	{
		if (TestMode.IsOn || NMapScreen.Instance == null)
		{
			return Task.CompletedTask;
		}
		return NEventRoom.Proceed();
	}

	private async Task RunTheChest()
	{
		IRunState runState = ((EventModel)this).Owner.RunState;
		List<Player> players = MultiplayerExpandedEvent.LivingPlayers(runState);
		foreach (Player item in players)
		{
			if (Enumerable.Count(PileTypeExtensions.GetPile((PileType)6, item).Cards, IsEligible) == 0)
			{
				MainFile.Logger.Warn($"[AnEmptyChest] Player {item.NetId} has no unenchanted Common/Uncommon/Rare card to offer.", 1);
			}
		}
		CardSelectorPrefs val = new CardSelectorPrefs(this.L10NLookup(((AbstractModel)this).Id.Entry + ".pages.INITIAL.selectionScreenPrompt"), 1) { Cancelable = false };
		CardSelectorPrefs prefs = val;
		List<CardModel> list = (await MultiplayerCardSelectCmd.FromDeckAllPlayers((PlayerChoiceContext)new BlockingPlayerChoiceContext(), players, prefs, IsEligible)).OrderBy(((Player Player, CardModel Card) c) => ((IPlayerCollection)runState).GetPlayerSlotIndex(c.Player)).Select(((Player Player, CardModel Card) c) => c.Card).OfType<CardModel>()
			.ToList();
		MainFile.Logger.Info($"[AnEmptyChest] {list.Count} card(s) put in the chest by {players.Count} player(s).", 1);
		await CardChestCmd.Offer(runState, list, this.L10NLookup(((AbstractModel)this).Id.Entry + ".pages.INITIAL.chestPrompt"));
	}
}
