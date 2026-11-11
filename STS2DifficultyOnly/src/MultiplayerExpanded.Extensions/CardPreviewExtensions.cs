using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MultiplayerExpanded.Extensions;

public static class CardPreviewExtensions
{
	private const int HorizontalLayoutMax = 5;

	public static void PreviewAdds(IReadOnlyList<CardPileAddResult> results, float time = 1.2f)
	{
		int num = Enumerable.Count(results, (CardPileAddResult r) =>
		{
			return r.success && LocalContext.IsMine(r.cardAdded);
		});
		if (num != 0)
		{
			CardCmd.PreviewCardPileAdd(results, time, (CardPreviewStyle)((num <= 5) ? 1 : 2));
		}
	}

	public static void PreviewAdd(CardPileAddResult result, float time = 1.2f)
	{
		CardCmd.PreviewCardPileAdd(result, time, (CardPreviewStyle)1);
	}

	public static CardPreviewStyle StyleFor(int cardCount)
	{
		if (cardCount > 5)
		{
			return (CardPreviewStyle)2;
		}
		return (CardPreviewStyle)1;
	}
}
