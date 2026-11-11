using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(NTreasureRoomRelicHolder), "OnUnfocus")]
internal static class CardChestHolderUnhoverPatch
{
	internal static Vector2 RestScale(NTreasureRoomRelicHolder holder)
	{
		return Vector2.One * (((Control)holder).Size.Y / NCard.defaultSize.Y);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool HideCardPreview(NTreasureRoomRelicHolder __instance)
	{
		NCard val = ((IEnumerable)((Node)__instance).GetChildren(false)).OfType<NCard>().FirstOrDefault();
		if (val == null)
		{
			return true;
		}
		((Node)val).CreateTween().TweenProperty((GodotObject)(object)val, (NodePath)("scale"), (Variant)(RestScale(__instance)), 0.4).SetEase((EaseType)1)
			.SetTrans((TransitionType)5);
		((CanvasItem)__instance).ZIndex = 0;
		NHoverTipSet.Remove((Control)(object)__instance);
		return false;
	}
}
