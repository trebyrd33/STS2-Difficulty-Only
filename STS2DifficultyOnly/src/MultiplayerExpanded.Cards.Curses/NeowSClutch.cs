using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MultiplayerExpanded.Cards.Curses;

public sealed class NeowSClutch : CurseCard, IBingBongExempt
{
	public override int MaxUpgradeLevel => 0;

	public override bool IsRelicUnique => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[2]
	{
		(CardKeyword)4,
		(CardKeyword)7
	};

	public NeowSClutch()
		: base(-1, (CardType)5, (CardRarity)9, (TargetType)0)
	{
	}
}
