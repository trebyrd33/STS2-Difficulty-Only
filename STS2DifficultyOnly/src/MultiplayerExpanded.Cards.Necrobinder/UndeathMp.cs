using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Cards.Necrobinder;

public sealed class UndeathMp : CardRework
{
	public override Type BaseCard => typeof(Undeath);

	public override int Cost => 0;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)4;

	public override TargetType Target => (TargetType)5;

	public override bool GainsBlock => true;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new BlockVar(7m, (ValueProp)8) };
		}
	}

	public override IEnumerable<IHoverTip> HoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Undeath>(false));

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.GainBlock(cardPlay.Target, card.DynamicVars.Block, cardPlay, false);
		Player player = cardPlay.Target.Player;
		CardPreviewExtensions.PreviewAdd(await CardPileCmd.AddGeneratedCardToCombat(card.CloneForPlayer(player), (PileType)3, player, (CardPilePosition)1), 2.2f);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Block).UpgradeValueBy(2m);
	}
}
