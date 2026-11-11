using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards.Defect;

public sealed class DarknessMp : CardRework
{
	public override Type BaseCard => typeof(Darkness);

	public override int Cost => 1;

	public override CardType Type => (CardType)2;

	public override CardRarity Rarity => (CardRarity)3;

	public override TargetType Target => (TargetType)5;

	public override IEnumerable<IHoverTip> HoverTips => new IHoverTip[2]
	{
		HoverTipFactory.Static((StaticHoverTip)2, Array.Empty<DynamicVar>()),
		HoverTipFactory.FromOrb<DarkOrb>()
	};

	public override async Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "Target");
		await OrbCmd.Channel<DarkOrb>(choiceContext, cardPlay.Target.Player ?? card.Owner);
		int triggerCount = ((!card.IsUpgraded) ? 1 : 2);
		foreach (Player player in card.CombatState.Players)
		{
			if (!player.Creature.IsAlive)
			{
				continue;
			}
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			IReadOnlyList<OrbModel> readOnlyList = ((playerCombatState != null) ? playerCombatState.OrbQueue.Orbs : null);
			if (readOnlyList == null)
			{
				continue;
			}
			foreach (OrbModel orb in readOnlyList.ToList())
			{
				if (orb is DarkOrb)
				{
					for (int i = 0; i < triggerCount; i++)
					{
						await GameApi.OrbPassive(choiceContext, orb, null);
					}
				}
			}
		}
	}
}
