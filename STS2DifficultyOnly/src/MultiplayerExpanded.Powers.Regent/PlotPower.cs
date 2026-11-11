using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Powers.Regent;

public sealed class PlotPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == ((PowerModel)this).Owner.Player && ((PowerModel)this).AmountOnTurnStart != 0)
		{
			this.Flash();
			int val = CardPile.MaxCardsInHand - PileTypeExtensions.GetPile((PileType)2, player).Cards.Count;
			List<CardModel> colorless = PileTypeExtensions.GetPile((PileType)1, player).Cards.Where((CardModel c) => c.VisualCardPool.IsColorless).Take(Math.Min(((PowerModel)this).Amount, val)).ToList();
			if (colorless.Count > 0)
			{
				await CardPileCmd.Add((IEnumerable<CardModel>)colorless, (PileType)1, (CardPilePosition)2, (AbstractModel)null, false);
				await CardPileCmd.Draw(choiceContext, (decimal)colorless.Count, player, false);
			}
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
