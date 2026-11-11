using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Cards.Colorless;
using MultiplayerExpanded.Features.AllSelect;

namespace MultiplayerExpanded.Events;

public sealed class GuardianAngel : AllSelectEvent
{
	private const int PrayIndex = 0;

	private const int LootIndex = 1;

	private const int LoopAt = 7;

	private const int RewardOptionCount = 3;

	public override ActModel[] Acts => new ActModel[2]
	{
		(ActModel)ModelDb.Act<Hive>(),
		(ActModel)ModelDb.Act<Glory>()
	};

	public override bool IsAllowed(IRunState runState)
	{
		if (base.IsAllowed(runState))
		{
			RunState val = (RunState)(object)((runState is RunState) ? runState : null);
			if (val != null)
			{
				return !val.VisitedEventIds.Contains(((AbstractModel)ModelDb.Event<HammeringLesson>()).Id);
			}
		}
		return false;
	}

	protected override AllSelectContention ContentionFor(int optionIndex)
	{
		if (optionIndex != 0)
		{
			return AllSelectContention.Shared;
		}
		return AllSelectContention.SoleOnly;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new _003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			this.Option((Func<Task>)Pray, HoverTipFactory.FromCardWithCardHoverTips<Immortal>(false), "INITIAL"),
			this.Option((Func<Task>)Loot, "INITIAL", Array.Empty<IHoverTip>())
		});
	}

	private async Task Pray()
	{
		AllSelectRound round = Round;
		if (round != null && round.IsReprompt)
		{
			this.SetEventState(CrowdedPage(round.RoundNumber + 1), (IEnumerable<EventOption>)this.GenerateInitialOptions());
			return;
		}
		if (round != null && !round.TakesIt)
		{
			this.SetEventFinished(CrowdedPage(7));
			return;
		}
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add((CardModel)(object)((ICardScope)((EventModel)this).Owner.RunState).CreateCard<Immortal>(((EventModel)this).Owner), (PileType)6, (CardPilePosition)1, (AbstractModel)null, false), 2f, (CardPreviewStyle)3);
		this.SetEventFinished(this.PageDescription("PRAY"));
	}

	private async Task Loot()
	{
		CardCreationOptions val = CardCreationOptions.ForNonCombatWithDefaultOdds((IEnumerable<CardPoolModel>)new _003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(((EventModel)this).Owner.Character.CardPool), (Func<CardModel, bool>)((CardModel c) =>
		{
			return (int)c.Type == 1;
		}));
		await RewardsCmd.OfferCustom(((EventModel)this).Owner, new List<Reward>(1) { (Reward)new CardReward(val, 3, ((EventModel)this).Owner, (PlayerChoiceSynchronizer)null) });
		this.SetEventFinished(this.PageDescription("LOOT"));
	}

	private LocString CrowdedPage(int collision)
	{
		return this.PageDescription($"CROWDED_{Math.Min(collision, 7)}");
	}
}
