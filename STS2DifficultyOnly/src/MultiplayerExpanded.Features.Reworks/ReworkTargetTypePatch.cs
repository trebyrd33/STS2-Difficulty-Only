using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Reworks;

[HarmonyPatch(typeof(CardModel), "TargetType", MethodType.Getter)]
internal static class ReworkTargetTypePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(CardModel __instance, ref TargetType __result)
	{
		if (ReworkPatches.ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			__result = (TargetType)(int)info.Rework.Target;
		}
	}
}
