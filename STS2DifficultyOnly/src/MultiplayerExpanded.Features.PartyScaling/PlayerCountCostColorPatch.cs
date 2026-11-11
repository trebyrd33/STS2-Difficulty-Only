using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers.Models;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Cards;
using MultiplayerExpanded.Features.Reworks;

namespace MultiplayerExpanded.Features.PartyScaling;

[HarmonyPatch(typeof(CardCostHelper), "GetEnergyCostColor")]
internal static class PlayerCountCostColorPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix(CardModel card, ICombatState? state, ref CardCostColor __result)
	{
		if (state == null || (int)__result == 3 || (!(card is MultiplayerExpandedCard) && !ReworkPatches.HasPartyScaledCost(card)) || card.EnergyCost.CostsX)
		{
			return;
		}
		int withModifiers = card.EnergyCost.GetWithModifiers((CostModifiers)0);
		decimal num = default;
		if (!((AbstractModel)card).TryModifyEnergyCostInCombat(card, (decimal)withModifiers, out num))
		{
			return;
		}
		decimal num2 = card.EnergyCost.GetWithModifiers((CostModifiers)2);
		foreach (AbstractModel item in state.IterateHookListeners())
		{
			item.TryModifyEnergyCostInCombat(card, num2, out num2);
		}
		foreach (AbstractModel item2 in state.IterateHookListeners())
		{
			item2.TryModifyEnergyCostInCombatLate(card, num2, out num2);
		}
		if (num2 < 0m)
		{
			num2 = 0m;
		}
		__result = (CardCostColor)((num2 > num) ? 1 : ((num2 < num) ? 2 : 0));
	}
}
