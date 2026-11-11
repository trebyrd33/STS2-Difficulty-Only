using System.Globalization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Cards;

namespace MultiplayerExpanded.Features.PartyScaling;

internal sealed class PartyScaledIntVar(string name, decimal printed, MultiplayerExpandedCard.PartyScaledVar rule) : IntVar(name, printed), IPartyScaledDynamicVar
{
	public decimal Current => PartyScaledDynamicVar.Amount(this._owner, in rule, ((DynamicVar)this).BaseValue);

	public void RefreshPreview()
	{
		decimal previewValue = (((DynamicVar)this).EnchantedValue = Current);
		((DynamicVar)this).PreviewValue = previewValue;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		RefreshPreview();
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
