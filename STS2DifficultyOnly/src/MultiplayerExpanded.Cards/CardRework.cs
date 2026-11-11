using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Cards;

public abstract class CardRework
{
	public abstract Type BaseCard { get; }

	public abstract int Cost { get; }

	public abstract CardType Type { get; }

	public abstract CardRarity Rarity { get; }

	public abstract TargetType Target { get; }

	public virtual int? StarCost => null;

	public virtual bool GainsBlock => false;

	public virtual IEnumerable<DynamicVar> Vars => Array.Empty<DynamicVar>();

	public virtual IEnumerable<CardKeyword> Keywords => Array.Empty<CardKeyword>();

	public virtual IEnumerable<IHoverTip> HoverTips => Array.Empty<IHoverTip>();

	public virtual IEnumerable<MultiplayerExpandedCard.PartyScaledVar> PartyScaledVars => Array.Empty<MultiplayerExpandedCard.PartyScaledVar>();

	public string LocKey => TypePrefix.GetPrefix(GetType()) + StringHelper.Slugify(BaseName(GetType().Name));

	public virtual int? EnergyCostForPlayerCount(CardModel card, int playerCount)
	{
		return null;
	}

	public virtual IEnumerable<IHoverTip> HoverTipsFor(CardModel card)
	{
		return HoverTips;
	}

	public virtual Task OnPlay(CardModel card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	public virtual void OnUpgrade(CardModel card)
	{
	}

	public virtual Task? OnTurnEndInHand(CardModel card, PlayerChoiceContext choiceContext)
	{
		return null;
	}

	private static string BaseName(string name)
	{
		if (!name.EndsWith("Beta", StringComparison.Ordinal))
		{
			return name;
		}
		int length = "Beta".Length;
		return name.Substring(0, name.Length - length);
	}
}
