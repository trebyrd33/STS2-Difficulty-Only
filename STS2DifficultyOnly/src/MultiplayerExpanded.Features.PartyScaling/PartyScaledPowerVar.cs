using System.Collections.Generic;
using System.Globalization;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Cards;

namespace MultiplayerExpanded.Features.PartyScaling;

internal sealed class PartyScaledPowerVar<T>(string name, decimal printed, MultiplayerExpandedCard.PartyScaledVar rule) : PowerVar<T>(name, printed), IPartyScaledDynamicVar where T : PowerModel
{
	public decimal Current => PartyScaledDynamicVar.Amount(this._owner, in rule, ((DynamicVar)this).BaseValue);

	public void RefreshPreview()
	{
		decimal previewValue = (((DynamicVar)this).EnchantedValue = Current);
		((DynamicVar)this).PreviewValue = previewValue;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal num = (((DynamicVar)this).EnchantedValue = Current);
		ICombatState combatState = card.CombatState;
		IEnumerable<AbstractModel> enumerable = default;
		((DynamicVar)this).PreviewValue = ((runGlobalHooks && combatState != null) ? Hook.ModifyPowerAmountGiven(combatState, (PowerModel)(object)ModelDb.Power<T>(), card.Owner.Creature, num, target, card, out enumerable) : num);
	}

	protected override decimal GetBaseValueForIConvertible()
	{
		return Current;
	}

	public override string ToString()
	{
		return ((int)Current).ToString(CultureInfo.InvariantCulture);
	}
}
