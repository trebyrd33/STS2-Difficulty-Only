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

namespace MultiplayerExpanded.Cards.Regent;

public sealed class BulwarkMp : CardRework
{
	public override Type BaseCard => typeof(Bulwark);

	public override int Cost => 2;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)5;

	public override bool GainsBlock => true;

	public override IEnumerable<IHoverTip> HoverTips => HoverTipFactory.FromForge();

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[2]
			{
				(DynamicVar)new BlockVar(12m, (ValueProp)8),
				(DynamicVar)new ForgeVar(10)
			};
		}
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await CreatureCmd.GainBlock(cardPlay.Target, card.DynamicVars.Block, cardPlay, false);
		await ForgeCmd.Forge((decimal)((DynamicVar)card.DynamicVars.Forge).IntValue, card.Owner, (AbstractModel)(object)card);
	}

	public override void OnUpgrade(CardModel card)
	{
		((DynamicVar)card.DynamicVars.Block).UpgradeValueBy(3m);
		((DynamicVar)card.DynamicVars.Forge).UpgradeValueBy(3m);
	}
}
