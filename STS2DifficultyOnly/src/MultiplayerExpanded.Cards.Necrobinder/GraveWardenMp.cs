using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Cards.Necrobinder;

public sealed class GraveWardenMp : CardRework
{
	public override Type BaseCard => typeof(GraveWarden);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)2;

	public override TargetType Target => (TargetType)5;

	public override bool GainsBlock => true;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new BlockVar(8m, (ValueProp)8) };
		}
	}

	public override IEnumerable<IHoverTip> HoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Soul>(false));

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.GainBlock(cardPlay.Target, card.DynamicVars.Block, cardPlay, false);
		CardPreviewExtensions.PreviewAdds(await CardPileCmd.AddGeneratedCardsToCombat((IEnumerable<CardModel>)Soul.Create(card.Owner, 1, card.CombatState), (PileType)1, card.Owner, (CardPilePosition)3));
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Block).UpgradeValueBy(3m);
	}
}
