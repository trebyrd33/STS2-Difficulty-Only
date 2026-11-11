using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Extensions;

public static class CardExtensions
{
	public static CardModel CloneForPlayer(this CardModel card, Player newOwner)
	{
		CardModel val = card.CreateClone();
		val.Owner = null;
		val.Owner = newOwner;
		return val;
	}

	public static async Task<CardModel> YoinkCard(CardModel card, Player newOwner, PileType targetPile = (PileType)2, CardPilePosition position = (CardPilePosition)1, bool? preview = null)
	{
		await CardPileCmd.RemoveFromCombat(card, false);
		CardModel clone = card.CloneForPlayer(newOwner);
		CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(clone, targetPile, newOwner, position);
		bool? flag = preview;
		bool flag2;
		if (flag.HasValue)
		{
			flag2 = flag == true;
		}
		else
		{
			bool flag3 = (((int)targetPile == 2 || (int)targetPile == 5) ? true : false);
			flag2 = !flag3;
		}
		if (flag2)
		{
			CardPreviewExtensions.PreviewAdd(result);
		}
		return clone;
	}
}
