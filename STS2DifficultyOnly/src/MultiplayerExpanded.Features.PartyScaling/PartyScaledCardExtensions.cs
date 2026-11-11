using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.PartyScaling;

internal static class PartyScaledCardExtensions
{
	internal static decimal PartyScaled(this CardModel card, string name)
	{
		DynamicVar val = card.DynamicVars[name];
		if (val is IPartyScaledDynamicVar partyScaledDynamicVar)
		{
			return partyScaledDynamicVar.Current;
		}
		MainFile.Logger.Error($"[PartyScaled] {((AbstractModel)card).Id.Entry} read '{name}' as party-scaled, but it is a {((object)val).GetType().Name}. Is it listed in PartyScaledVars? Using its printed value.", 1);
		return val.BaseValue;
	}
}
