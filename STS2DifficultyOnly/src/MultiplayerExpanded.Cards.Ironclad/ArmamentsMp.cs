using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Cards.Ironclad;

public sealed class ArmamentsMp : CardRework
{
	public override Type BaseCard => typeof(Armaments);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)2;

	public override TargetType Target => (TargetType)5;

	public override bool GainsBlock => true;

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new BlockVar(5m, (ValueProp)8) };
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, cardPlay, false);
		Player player = cardPlay.Target.Player;
		if (player == null)
		{
			return;
		}
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)2, player).Cards.Where((CardModel c) => c.IsUpgradable).ToList();
		if (list.Count == 0)
		{
			return;
		}
		if (card.IsUpgraded)
		{
			CardCmd.Upgrade((IEnumerable<CardModel>)list, (CardPreviewStyle)1);
			return;
		}
		LocString val = new LocString("cards", LocKey + ".selectionScreenPrompt");
		card.DynamicVars.AddTo(val);
		CardModel val2 = (await CardSelectCmd.FromSimpleGrid(choiceContext, (IReadOnlyList<CardModel>)list, player, new CardSelectorPrefs(val, 1))).FirstOrDefault();
		if (val2 != null)
		{
			CardCmd.Upgrade(val2, (CardPreviewStyle)1);
		}
	}
}
