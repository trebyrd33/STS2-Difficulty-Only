using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Cards.Colorless;
using MultiplayerExpanded.Features.AllSelect;

namespace MultiplayerExpanded.Events;

public sealed class HammeringLesson : AllSelectEvent
{
	private const int LearnIndex = 0;

	private const int SearchIndex = 1;

	private const int RewardOptionCount = 3;

	public override ActModel[] Acts => new ActModel[2]
	{
		(ActModel)ModelDb.Act<Hive>(),
		(ActModel)ModelDb.Act<Glory>()
	};

	public override int MaxRounds => 5;

	public override bool IsAllowed(IRunState runState)
	{
		if (base.IsAllowed(runState))
		{
			RunState val = (RunState)(object)((runState is RunState) ? runState : null);
			if (val != null)
			{
				return !val.VisitedEventIds.Contains(((AbstractModel)ModelDb.Event<GuardianAngel>()).Id);
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
			this.Option((Func<Task>)Learn, HoverTipFactory.FromCardWithCardHoverTips<Improvements>(false), "INITIAL"),
			this.Option((Func<Task>)Search, "INITIAL", Array.Empty<IHoverTip>())
		});
	}

	private async Task Learn()
	{
		AllSelectRound round = Round;
		if (round != null && round.IsReprompt)
		{
			this.SetEventState(this.PageDescription("CROWDED"), (IEnumerable<EventOption>)this.GenerateInitialOptions());
			return;
		}
		if (round != null && !round.TakesIt)
		{
			this.SetEventFinished(this.PageDescription("CROWDED"));
			return;
		}
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add((CardModel)(object)((ICardScope)((EventModel)this).Owner.RunState).CreateCard<Improvements>(((EventModel)this).Owner), (PileType)6, (CardPilePosition)1, (AbstractModel)null, false), 2f, (CardPreviewStyle)3);
		this.SetEventFinished(this.PageDescription("LEARN"));
	}

	private async Task Search()
	{
		CardCreationOptions val = CardCreationOptions.ForNonCombatWithDefaultOdds((IEnumerable<CardPoolModel>)new _003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(((EventModel)this).Owner.Character.CardPool), (Func<CardModel, bool>)((CardModel c) =>
		{
			return (int)c.Type == 2;
		}));
		await RewardsCmd.OfferCustom(((EventModel)this).Owner, new List<Reward>(1) { (Reward)new CardReward(val, 3, ((EventModel)this).Owner, (PlayerChoiceSynchronizer)null) });
		this.SetEventFinished(this.PageDescription("SEARCH"));
	}
}
