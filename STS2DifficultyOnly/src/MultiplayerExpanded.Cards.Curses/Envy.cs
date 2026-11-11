using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MultiplayerExpanded.Cards.Curses;

public sealed class Envy : CurseCard
{
	private int _strengthGain = 5;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[2]
	{
		(CardKeyword)2,
		(CardKeyword)1
	};

	public override bool HasTurnEndInHandEffect => true;

	[SavedProperty]
	public int StrengthGain
	{
		get
		{
			return _strengthGain;
		}
		set
		{
			this.AssertMutable();
			_strengthGain = value;
			this.DynamicVars["StrGain"].BaseValue = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[3]
			{
				(DynamicVar)new IntVar("StrLoss", 3m),
				(DynamicVar)new IntVar("EveryoneStrLoss", 1m),
				(DynamicVar)new IntVar("StrGain", (decimal)_strengthGain)
			};
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<StrengthPower>((int?)null) };

	public Envy()
		: base(1, (CardType)5, (CardRarity)9, (TargetType)6)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Target != null)
		{
			await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, -this.DynamicVars["StrLoss"].BaseValue, this.Owner.Creature, (CardModel)(object)this, false);
		}
		if (_strengthGain > 0)
		{
			await PowerCmd.Apply<StrengthPower>(choiceContext, this.Owner.Creature, (decimal)_strengthGain, this.Owner.Creature, (CardModel)(object)this, false);
		}
		DecreaseStrengthGain();
		(this.DeckVersion as Envy)?.DecreaseStrengthGain();
	}

	private void DecreaseStrengthGain()
	{
		StrengthGain = Math.Max(0, _strengthGain - 1);
	}

	protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		foreach (Player player in this.CombatState.Players)
		{
			if (player.Creature.IsAlive)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, player.Creature, -this.DynamicVars["EveryoneStrLoss"].BaseValue, (Creature)null, (CardModel)(object)this, false);
			}
		}
	}
}
