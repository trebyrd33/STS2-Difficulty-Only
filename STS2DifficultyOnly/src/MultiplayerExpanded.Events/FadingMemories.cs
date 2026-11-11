using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Events;

public sealed class FadingMemories : MultiplayerExpandedEvent
{
	private const int MaxPartySize = 4;

	private const string OutcomePage = "FORGOTTEN";

	private const string OtherPlayersVar = "OtherPlayers";

	private const string OtherPlayerCountVar = "OtherPlayerCount";

	private List<CardModel?>? _offerings;

	private List<CardModel?>? Offerings
	{
		get
		{
			return _offerings;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_offerings = value;
		}
	}

	public override bool IsShared => true;

	public override ActModel[] Acts => Array.Empty<ActModel>();

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			List<DynamicVar> list = new List<DynamicVar>
			{
				(DynamicVar)new StringVar("OtherPlayers", ""),
				(DynamicVar)new IntVar("OtherPlayerCount", 0m)
			};
			for (int i = 0; i < 4; i++)
			{
				list.Add((DynamicVar)new StringVar(PlayerVar(i), ""));
				list.Add((DynamicVar)new StringVar(CardVar(i), ""));
				list.Add((DynamicVar)new StringVar(BasicVar(i), ""));
			}
			return list;
		}
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (base.IsAllowed(runState))
		{
			return ((IPlayerCollection)runState).Players.All((Player player) => player.Deck.Cards.Any(IsOffering) && player.Deck.Cards.Any((CardModel c) => IsBasicTagged(c, (CardTag)1)) && player.Deck.Cards.Any((CardModel c) => IsBasicTagged(c, (CardTag)2)));
		}
		return false;
	}

	public override void CalculateVars()
	{
		base.CalculateVars();
		if (((EventModel)this).Owner != null)
		{
			List<Player> list = SlotOrderedPlayers(((EventModel)this).Owner.RunState);
			Offerings = SelectForParty(list);
			for (int i = 0; i < 4; i++)
			{
				CardModel val = OfferingFor(i);
				SetVar(PlayerVar(i), (i < list.Count) ? NameOf(list[i]) : string.Empty);
				SetVar(CardVar(i), ((val != null) ? val.Title : null) ?? string.Empty);
				string key = BasicVar(i);
				CardModel? val2 = OwnBasicFor(val);
				SetVar(key, ((val2 != null) ? val2.Title : null) ?? string.Empty);
			}
			SetVar("OtherPlayers", CountWord(list.Count - 1));
			((EventModel)this).DynamicVars["OtherPlayerCount"].BaseValue = list.Count - 1;
		}
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		EventOption[] source = new EventOption[4]
		{
			this.Option((Func<Task>)PromiseOfPower, TipsFor(0), "INITIAL"),
			this.Option((Func<Task>)GraspOfRespect, TipsFor(1), "INITIAL"),
			this.Option((Func<Task>)WishOfDesperation, TipsFor(2), "INITIAL"),
			this.Option((Func<Task>)MomentOfExistence, TipsFor(3), "INITIAL")
		};
		Player owner = ((EventModel)this).Owner;
		int count = Math.Clamp((owner != null) ? ((IPlayerCollection)owner.RunState).Players.Count : 4, 1, 4);
		return ((IEnumerable<EventOption>)source).Take(count).ToList();
	}

	private async Task PromiseOfPower()
	{
		await Forget(0);
	}

	private async Task GraspOfRespect()
	{
		await Forget(1);
	}

	private async Task WishOfDesperation()
	{
		await Forget(2);
	}

	private async Task MomentOfExistence()
	{
		await Forget(3);
	}

	private async Task Forget(int chosenSlot)
	{
		CardModel val = ((((EventModel)this).Owner == null) ? null : CardToLose(chosenSlot, ((EventModel)this).Owner));
		if (val != null)
		{
			CardPile pile = val.Pile;
			if (pile != null && (int)pile.Type == 6)
			{
				await CardPileCmd.RemoveFromDeck(val, true);
				goto IL_0122;
			}
		}
		Logger logger = MainFile.Logger;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 2);
		defaultInterpolatedStringHandler.AppendLiteral("[FadingMemories] Player ");
		Player owner = ((EventModel)this).Owner;
		defaultInterpolatedStringHandler.AppendFormatted((owner != null) ? new ulong?(owner.NetId) : ((ulong?)null));
		defaultInterpolatedStringHandler.AppendLiteral(" had nothing to give up for slot ");
		defaultInterpolatedStringHandler.AppendFormatted(chosenSlot);
		defaultInterpolatedStringHandler.AppendLiteral(".");
		logger.Warn(defaultInterpolatedStringHandler.ToStringAndClear(), 1);
		goto IL_0122;
		IL_0122:
		if (RunsPartyWideStep)
		{
			await ForgetForTheDead(chosenSlot);
		}
		this.SetEventFinished(this.PageDescription("FORGOTTEN"));
	}

	private async Task ForgetForTheDead(int chosenSlot)
	{
		try
		{
			List<CardModel> list = (from p in ((IPlayerCollection)((EventModel)this).Owner.RunState).Players.Where((Player p) =>
				{
					Creature creature = p.Creature;
					return creature != null && creature.IsDead;
				})
				select CardToLose(chosenSlot, p)).OfType<CardModel>().Where((CardModel c) =>
			{
				CardPile pile = c.Pile;
				return pile != null && (int)pile.Type == 6;
			}).ToList();
			if (list.Count > 0)
			{
				await CardPileCmd.RemoveFromDeck((IReadOnlyList<CardModel>)list, true);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[FadingMemories] Could not remove dead players' cards: " + ex.Message, 1);
		}
	}

	private CardModel? CardToLose(int chosenSlot, Player player)
	{
		CardModel val = OfferingFor(chosenSlot);
		if (((IPlayerCollection)player.RunState).GetPlayerSlotIndex(player) != chosenSlot)
		{
			return BasicFor(val, player);
		}
		return val;
	}

	private CardModel? OwnBasicFor(CardModel? offering)
	{
		if (((EventModel)this).Owner != null)
		{
			return BasicFor(offering, ((EventModel)this).Owner);
		}
		return null;
	}

	private static CardModel? BasicFor(CardModel? offering, Player player)
	{
		if (offering == null)
		{
			return null;
		}
		CardTag tag = (CardTag)(((int)offering.Type == 1) ? 1 : 2);
		return player.Deck.Cards.FirstOrDefault((CardModel c) =>
		{
			return IsBasicTagged(c, tag);
		});
	}

	private CardModel? OfferingFor(int slot)
	{
		if (Offerings == null || slot < 0 || slot >= Offerings.Count)
		{
			return null;
		}
		return Offerings[slot];
	}

	private List<CardModel?> SelectForParty(IReadOnlyList<Player> players)
	{
		List<CardModel> list = new List<CardModel>(players.Count);
		foreach (Player player in players)
		{
			List<CardModel> list2 = player.Deck.Cards.Where(IsOffering).ToList();
			list.Add((list2.Count > 0) ? ((EventModel)this).Rng.NextItem<CardModel>((IEnumerable<CardModel>)list2) : null);
		}
		return list;
	}

	private static bool IsOffering(CardModel card)
	{
		bool flag = card.IsRemovable && (int)card.Rarity != 1;
		if (flag)
		{
			CardType type = card.Type;
			bool flag2 = (uint)((int)type - 1) <= 1u;
			flag = flag2;
		}
		return flag;
	}

	private static bool IsBasicTagged(CardModel card, CardTag tag)
	{
		if (card.IsRemovable && (int)card.Rarity == 1)
		{
			return card.Tags.Contains(tag);
		}
		return false;
	}

	private static List<Player> SlotOrderedPlayers(IRunState runState)
	{
		return ((IEnumerable<Player>)((IPlayerCollection)runState).Players).OrderBy((Func<Player, int>)((IPlayerCollection)runState).GetPlayerSlotIndex).ToList();
	}

	private static string NameOf(Player player)
	{
		return PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId);
	}

	private static string PlayerVar(int slot)
	{
		return $"Player{slot + 1}";
	}

	private static string CardVar(int slot)
	{
		return $"Card{slot + 1}";
	}

	private static string BasicVar(int slot)
	{
		return $"Basic{slot + 1}";
	}

	private void SetVar(string key, string value)
	{
		((StringVar)((EventModel)this).DynamicVars[key]).StringValue = value;
	}

	private static string CountWord(int count)
	{
		return count switch
		{
			1 => "one", 
			2 => "two", 
			3 => "three", 
			_ => count.ToString(), 
		};
	}

	private IEnumerable<IHoverTip> TipsFor(int slot)
	{
		CardModel val = OfferingFor(slot);
		if (val == null)
		{
			return Array.Empty<IHoverTip>();
		}
		List<IHoverTip> list = new List<IHoverTip> { HoverTipFactory.FromCard(val, false) };
		CardModel val2 = OwnBasicFor(val);
		if (val2 != null)
		{
			list.Add(HoverTipFactory.FromCard(val2, false));
		}
		return list;
	}
}
