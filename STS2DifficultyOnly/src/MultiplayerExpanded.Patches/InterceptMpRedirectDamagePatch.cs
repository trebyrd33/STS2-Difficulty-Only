using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.Intents;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(Hook), "ModifyDamage")]
internal static class InterceptMpRedirectDamagePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void Postfix(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer, decimal damage, ValueProp props, CardModel? cardSource, ModifyDamageHookType modifyDamageHookType, CardPreviewMode previewMode, ref decimal __result)
	{
		if (RedirectShare.AnyMeasuring || target == null || dealer == null || combatState == null || ((int)modifyDamageHookType & 0xE) != 14 || !ValuePropExtensions.IsPoweredAttack(props) || dealer.Side == target.Side)
		{
			return;
		}
		decimal shares = 0m;
		foreach (Creature playerCreature in combatState.PlayerCreatures)
		{
			if (playerCreature != target && playerCreature.IsAlive && playerCreature.Side == target.Side && CovererOf(playerCreature) == target && AttackOriginalTargets.WasTargeted(dealer, playerCreature))
			{
				shares += RedirectShare.Measure(runState, combatState, playerCreature, dealer, damage, props, cardSource, modifyDamageHookType, previewMode);
			}
		}
		__result = RedirectShare.AddShares(runState, combatState, target, dealer, __result, shares, props, cardSource, previewMode);
	}

	internal static Creature? CovererOf(Creature covered)
	{
		if (!MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMp()) return null;
		CoveredPower? val = covered.GetPowerInstances<CoveredPower>().LastOrDefault((CoveredPower p) =>
		{
			Creature applier = ((PowerModel)p).Applier;
			return applier != null && applier.IsAlive && applier != covered;
		});
		if (val == null)
		{
			return null;
		}
		return ((PowerModel)val).Applier;
	}
}
