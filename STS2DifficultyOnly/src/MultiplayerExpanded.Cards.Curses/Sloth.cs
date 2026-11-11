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

public sealed class Sloth : CurseCard
{
	private int _dexterityGain = 5;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[2]
	{
		(CardKeyword)2,
		(CardKeyword)1
	};

	public override bool HasTurnEndInHandEffect => true;

	[SavedProperty]
	public int DexterityGain
	{
		get
		{
			return _dexterityGain;
		}
		set
		{
			this.AssertMutable();
			_dexterityGain = value;
			this.DynamicVars["DexGain"].BaseValue = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[3]
			{
				(DynamicVar)new IntVar("DexLoss", 3m),
				(DynamicVar)new IntVar("EveryoneDexLoss", 1m),
				(DynamicVar)new IntVar("DexGain", (decimal)_dexterityGain)
			};
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<DexterityPower>((int?)null) };

	public Sloth()
		: base(1, (CardType)5, (CardRarity)9, (TargetType)6)
	{
	}

	private void DecreaseGainFromPlay()
	{
		DexterityGain = Math.Max(0, DexterityGain - 1);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Target != null)
		{
			await PowerCmd.Apply<DexterityPower>(choiceContext, cardPlay.Target, -this.DynamicVars["DexLoss"].BaseValue, this.Owner.Creature, (CardModel)(object)this, false);
		}
		if (_dexterityGain > 0)
		{
			await PowerCmd.Apply<DexterityPower>(choiceContext, this.Owner.Creature, (decimal)_dexterityGain, this.Owner.Creature, (CardModel)(object)this, false);
			DecreaseGainFromPlay();
			(this.DeckVersion as Sloth)?.DecreaseGainFromPlay();
		}
	}

	protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
	{
		foreach (Player player in this.CombatState.Players)
		{
			if (player.Creature.IsAlive)
			{
				await PowerCmd.Apply<DexterityPower>(choiceContext, player.Creature, -this.DynamicVars["EveryoneDexLoss"].BaseValue, (Creature)null, (CardModel)(object)this, false);
			}
		}
	}
}
