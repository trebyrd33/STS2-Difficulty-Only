using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Relics;

namespace MultiplayerExpanded.Events;

public sealed class LabyrinthOfMirrors : MultiplayerExpandedEvent
{
	private const decimal SmashHpLossFraction = 0.15m;

	private const decimal HoldHandsHealFraction = 0.05m;

	public override bool IsShared => true;

	public override ActModel[] Acts => new ActModel[1] { (ActModel)ModelDb.Act<Glory>() };

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new _003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
			{
				(DynamicVar)new HpLossVar(0m),
				(DynamicVar)new HealVar(0m),
				(DynamicVar)new GoldVar(0)
			});
		}
	}

	public override void CalculateVars()
	{
		Player owner = ((EventModel)this).Owner;
		int num = ((owner != null) ? owner.Creature.MaxHp : 0);
		((DynamicVar)((EventModel)this).DynamicVars.HpLoss).BaseValue = (decimal)num * 0.15m;
		((DynamicVar)((EventModel)this).DynamicVars.Heal).BaseValue = (decimal)num * 0.05m;
		((DynamicVar)((EventModel)this).DynamicVars.Gold).BaseValue = ((EventModel)this).Rng.NextInt(100, 131);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new _003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			this.Option((Func<Task>)SmashThrough, "INITIAL", Array.Empty<IHoverTip>()).ThatWillKillPlayerIf((Func<Player, bool>)WouldDieSmashing),
			this.Option((Func<Task>)HoldHands, HoverTipFactory.FromRelic<ALingeringSomething>(), "INITIAL")
		});
	}

	private static bool WouldDieSmashing(Player player)
	{
		return player.Creature.CurrentHp <= (int)((decimal)player.Creature.MaxHp * 0.15m);
	}

	private async Task SmashThrough()
	{
		await GameApi.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((EventModel)this).Owner.Creature, ((DynamicVar)((EventModel)this).DynamicVars.HpLoss).IntValue, (ValueProp)6, null, null, null);
		await PlayerCmd.GainGold((decimal)((DynamicVar)((EventModel)this).DynamicVars.Gold).IntValue, ((EventModel)this).Owner, false);
		this.SetEventFinished(this.PageDescription("SMASH_THROUGH"));
	}

	private async Task HoldHands()
	{
		await CreatureCmd.Heal(((EventModel)this).Owner.Creature, (decimal)((DynamicVar)((EventModel)this).DynamicVars.Heal).IntValue, true);
		await RelicCmd.Obtain<ALingeringSomething>(((EventModel)this).Owner);
		if (RunsPartyWideStep)
		{
			await RedistributeDecks(((EventModel)this).Owner.RunState);
		}
		this.SetEventFinished(this.PageDescription("HOLD_HANDS"));
	}

	private async Task RedistributeDecks(IRunState runState)
	{
		List<Player> players = MultiplayerExpandedEvent.LivingPlayers(runState);
		if (players.Count < 2)
		{
			return;
		}
		List<int> list = new List<int>(players.Count);
		List<int> list2 = new List<int>(players.Count);
		List<CardModel> list3 = new List<CardModel>();
		foreach (Player item2 in players)
		{
			IReadOnlyList<CardModel> cards = PileTypeExtensions.GetPile((PileType)6, item2).Cards;
			List<CardModel> list4 = cards.Where((CardModel c) => c != null && c.IsRemovable).ToList();
			list.Add(list4.Count);
			list2.Add(cards.Count - list4.Count);
			list3.AddRange(list4);
		}
		((EventModel)this).Rng.Shuffle<CardModel>((IList<CardModel>)list3);
		MainFile.Logger.Info($"[LabyrinthOfMirrors] Redealing {list3.Count} movable card(s) across {players.Count} player(s) (deal counts {string.Join("/", list)}, eternals kept {string.Join("/", list2)}).", 1);
		List<(CardModel Card, Player NewOwner)> moves = new List<(CardModel, Player)>();
		int num = 0;
		for (int num2 = 0; num2 < players.Count; num2++)
		{
			Player val = players[num2];
			for (int num3 = 0; num3 < list[num2]; num3++)
			{
				CardModel val2 = list3[num++];
				if (val2.Owner != val)
				{
					moves.Add((val2, val));
				}
			}
		}
		foreach (var item3 in moves)
		{
			CardModel item = item3.Card;
			MapPointHistoryEntry currentMapPointHistoryEntry = runState.CurrentMapPointHistoryEntry;
			if (currentMapPointHistoryEntry != null)
			{
				currentMapPointHistoryEntry.GetEntry(item.Owner.NetId).CardsRemoved.Add(item.ToSerializable());
			}
			item.RemoveFromCurrentPile(false);
			((ICardScope)runState).RemoveCard(item);
		}
		foreach (Player newOwner in players)
		{
			List<CardModel> list5 = (from m in moves
				where m.NewOwner == newOwner
				select m.Card).ToList();
			if (list5.Count == 0)
			{
				continue;
			}
			foreach (CardModel item4 in list5)
			{
				((ICardScope)runState).AddCard(item4, newOwner);
			}
			await CardPileCmd.Add((IEnumerable<CardModel>)list5, (PileType)6, (CardPilePosition)1, (AbstractModel)null, true);
		}
		foreach (Player item5 in players)
		{
			PileTypeExtensions.GetPile((PileType)6, item5).InvokeCardAddFinished();
		}
	}
}
