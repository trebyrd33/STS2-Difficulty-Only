using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Extensions;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Cards;

public abstract class MultiplayerExpandedCard : CustomCardModel
{
	public readonly record struct PartyScaledVar(string Name, Func<CardModel, int, decimal> Amount, decimal Minimum = 1m)
	{
		internal decimal For(CardModel card, int playerCount)
		{
			return Math.Max(Minimum, Amount(card, playerCount));
		}
	}

	public override CardMultiplayerConstraint MultiplayerConstraint => (CardMultiplayerConstraint)1;

	public override string CustomPortraitPath => (BaseLib.Extensions.StringExtensions.RemovePrefix(((AbstractModel)this).Id.Entry).ToLowerInvariant() + ".png").BigCardImagePath();

	public override string PortraitPath => (BaseLib.Extensions.StringExtensions.RemovePrefix(((AbstractModel)this).Id.Entry).ToLowerInvariant() + ".png").CardImagePath();

	public override string BetaPortraitPath => ("beta/" + BaseLib.Extensions.StringExtensions.RemovePrefix(((AbstractModel)this).Id.Entry).ToLowerInvariant() + ".png").CardImagePath();

	public virtual bool IsRelicUnique => false;

	protected virtual IEnumerable<PartyScaledVar> PartyScaledVars => Array.Empty<PartyScaledVar>();

	internal IEnumerable<PartyScaledVar> PartyScaledVarRules => PartyScaledVars;

	protected MultiplayerExpandedCard(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target, true, false)
	{
		if (BranchFilter.Allows(((object)this).GetType()))
		{
			CustomContentDictionary.AddModel(((object)this).GetType());
		}
	}

	protected virtual int? EnergyCostForPlayerCount(int playerCount)
	{
		return null;
	}

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if ((object)card != this || originalCost <= 0m)
		{
			return false;
		}
		ICombatState combatState = ((CardModel)this).CombatState;
		int num = ((combatState != null) ? combatState.Players.Count : 0);
		if (num <= 0)
		{
			return false;
		}
		int? num2 = EnergyCostForPlayerCount(num);
		if (!num2.HasValue)
		{
			return false;
		}
		int withModifiers = ((CardModel)this).EnergyCost.GetWithModifiers((CostModifiers)0);
		int num3 = Math.Max(0, num2.Value) - withModifiers;
		if (num3 == 0)
		{
			return false;
		}
		modifiedCost = Math.Max(0m, originalCost + (decimal)num3);
		return modifiedCost != originalCost;
	}
}
