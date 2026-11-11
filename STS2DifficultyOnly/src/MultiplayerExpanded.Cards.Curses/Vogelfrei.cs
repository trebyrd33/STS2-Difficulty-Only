using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MultiplayerExpanded.Cards.Curses;

public sealed class Vogelfrei : CurseCard
{
	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[2]
	{
		(CardKeyword)5,
		(CardKeyword)1
	};

	public Vogelfrei()
		: base(2, (CardType)5, (CardRarity)9, (TargetType)0)
	{
	}
}
