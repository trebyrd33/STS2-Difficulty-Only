using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Cards.Defect;

public sealed class HotfixMp : CardRework
{
	public override Type BaseCard => typeof(Hotfix);

	public override int Cost => 0;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)2;

	public override TargetType Target => (TargetType)1;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)1 };

	public override IEnumerable<DynamicVar> Vars => (IEnumerable<DynamicVar>)(object)new PowerVar<HotfixPower>[1]
	{
		new PowerVar<HotfixPower>(2m)
	};

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[1] { HoverTipFactory.FromPower<FocusPower>((int?)null) };

	public override void OnUpgrade(CardModel card)
	{
		card.RemoveKeyword((CardKeyword)1);
	}

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		foreach (Player player in card.CombatState.Players)
		{
			if (player.Creature.IsAlive)
			{
				await PowerCmd.Apply<HotfixPower>(choiceContext, player.Creature, DynamicVarSetExtensions.Power<HotfixPower>(card.DynamicVars).BaseValue, card.Owner.Creature, card, false);
			}
		}
	}
}
