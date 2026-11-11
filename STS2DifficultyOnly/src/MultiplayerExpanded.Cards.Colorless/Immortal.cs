using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

[Pool(typeof(EventCardPool))]
public sealed class Immortal : ColorlessCard
{
	public override bool CanBeGeneratedInCombat => false;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[2]
	{
		(CardKeyword)2,
		(CardKeyword)1
	};

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new IntVar("Health", 50m) };
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<ImmortalPower>((int?)null) };

	public override CardPoolModel Pool => (CardPoolModel)(object)ModelDb.CardPool<EventCardPool>();

	public Immortal()
		: base(1, (CardType)2, (CardRarity)6, (TargetType)6)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.TriggerAnim(cardPlay.Target, "PowerUp", cardPlay.Target.Player.Character.PowerUpAnimDelay);
		int powerAmount = cardPlay.Target.GetPowerAmount<ImmortalPower>();
		decimal num = Math.Min(((CardModel)this).DynamicVars["Health"].BaseValue, 100m - (decimal)powerAmount);
		if (num > 0m)
		{
			await PowerCmd.Apply<ImmortalPower>(choiceContext, cardPlay.Target, num, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
		}
		if (((CardModel)this).DeckVersion != null)
		{
			await CardPileCmd.RemoveFromDeck(((CardModel)this).DeckVersion, true);
			((CardModel)this).DeckVersion = null;
		}
	}

	protected override void OnUpgrade()
	{
		((CardModel)this).RemoveKeyword((CardKeyword)2);
		((CardModel)this).DynamicVars["Health"].UpgradeValueBy(50m);
	}
}
