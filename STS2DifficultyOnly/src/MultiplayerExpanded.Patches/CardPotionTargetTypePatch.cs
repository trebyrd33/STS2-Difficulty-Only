using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Potions;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch]
internal class CardPotionTargetTypePatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		Type[] array = new Type[4]
		{
			typeof(AttackPotion),
			typeof(SkillPotion),
			typeof(PowerPotion),
			typeof(ColorlessPotion)
		};
		foreach (Type type in array)
		{
			yield return AccessTools.DeclaredPropertyGetter(type, "TargetType");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(ref TargetType __result)
	{
		if ((MultiplayerExpandedConfig.MultiplayerPotionChanges && MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMultiplayerRun())) __result = (TargetType)5;
	}
}
