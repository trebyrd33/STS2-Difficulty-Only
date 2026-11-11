using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(NTreasureRoomRelicHolder), "OnFocus")]
internal static class CardChestHolderHoverPatch
{
	private const float ChestHoverScale = 1.06f;

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static bool ShowCardPreview(NTreasureRoomRelicHolder __instance)
	{
		NCard val = ((IEnumerable)((Node)__instance).GetChildren(false)).OfType<NCard>().FirstOrDefault();
		if (val == null)
		{
			return true;
		}
		CardChestRoom? active = CardChestRoom.Active;
		bool flag = active != null && active.Layout == CardPickLayout.ChooseACard;
		Vector2 val2 = CardChestHolderUnhoverPatch.RestScale(__instance);
		((Node)val).CreateTween().TweenProperty((GodotObject)(object)val, (NodePath)("scale"), (Variant)(val2 * (flag ? 1.25f : 1.06f)), 0.05);
		((CanvasItem)__instance).ZIndex = 1;
		CardModel model = val.Model;
		if (model == null)
		{
			return false;
		}
		IEnumerable<IHoverTip> enumerable;
		if (!flag)
		{
			enumerable = ((IEnumerable<IHoverTip>)new IHoverTip[1] { HoverTipFactory.FromCard(model, false) }).Concat(model.HoverTips);
		}
		else
		{
			IEnumerable<IHoverTip> hoverTips = model.HoverTips;
			enumerable = hoverTips;
		}
		IEnumerable<IHoverTip> enumerable2 = enumerable;
		NHoverTipSet.CreateAndShow((Control)(object)__instance, enumerable2, HoverTip.GetHoverTipAlignment((Control)(object)__instance, 0.75f));
		return false;
	}
}
