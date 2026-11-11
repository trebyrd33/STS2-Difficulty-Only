using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace MultiplayerExpanded.Features.CardChest;

[HarmonyPatch(typeof(NHandImage), "_Ready")]
internal static class CardChestHandFingertipPatch
{
	private const float OpaqueAlpha = 40f / 255f;

	private static readonly AccessTools.FieldRef<NHandImage, TextureRect> TextureRectRef = AccessTools.FieldRefAccess<NHandImage, TextureRect>("_textureRect");

	private static readonly AccessTools.FieldRef<NHandImage, Vector2> OriginalPositionRef = AccessTools.FieldRefAccess<NHandImage, Vector2>("_originalPosition");

	private static readonly Dictionary<string, Vector2?> TipCache = new Dictionary<string, Vector2?>();

	private static readonly ConditionalWeakTable<NHandImage, Tuple<Vector2, Vector2>> VanillaPlacement = new ConditionalWeakTable<NHandImage, Tuple<Vector2, Vector2>>();

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPostfix]
	private static void PutFingertipOnPointer(NHandImage __instance)
	{
		if (CardChestRoom.Active != null)
		{
			Apply(__instance);
		}
	}

	internal static void Apply(NHandImage hand)
	{
		TextureRect rect = TextureRectRef.Invoke(hand);
		if (rect == null)
		{
			return;
		}
		Tuple<Vector2, Vector2> value = VanillaPlacement.GetValue(hand, (NHandImage _) =>
		{
			return Tuple.Create<Vector2, Vector2>(((Control)rect).Position, ((Control)rect).PivotOffset);
		});
		Texture2D texture = rect.Texture;
		if (texture != null)
		{
			Vector2? val = TipInRect(texture, ((Control)rect).Size);
			if (val.HasValue)
			{
				Vector2 valueOrDefault = val.GetValueOrDefault();
				((Control)rect).Position = value.Item1 + value.Item2 - valueOrDefault;
				((Control)rect).PivotOffset = valueOrDefault;
				OriginalPositionRef.Invoke(hand) = ((Control)rect).Position;
			}
		}
	}

	private static Vector2? TipInRect(Texture2D texture, Vector2 rectSize)
	{
		string resourcePath = ((Resource)texture).ResourcePath;
		if (!TipCache.TryGetValue(resourcePath, out var value))
		{
			value = FindTip(texture);
			TipCache[resourcePath] = value;
		}
		if (value.HasValue)
		{
			Vector2 valueOrDefault = value.GetValueOrDefault();
			Vector2 size = texture.GetSize();
			float num = Mathf.Min(rectSize.X / size.X, rectSize.Y / size.Y);
			return (rectSize - size * num) * 0.5f + valueOrDefault * num;
		}
		return null;
	}

	private static Vector2? FindTip(Texture2D texture)
	{
		Image image = texture.GetImage();
		if (image == null)
		{
			return null;
		}
		if (image.IsCompressed())
		{
			image.Decompress();
		}
		int width = image.GetWidth();
		int height = image.GetHeight();
		for (int i = 0; i < height; i++)
		{
			float num = 0f;
			int num2 = 0;
			for (int j = 0; j < width; j++)
			{
				if (!(image.GetPixel(j, i).A <= 40f / 255f))
				{
					num += (float)j;
					num2++;
				}
			}
			if (num2 > 0)
			{
				return new Vector2(num / (float)num2, (float)i);
			}
		}
		return null;
	}
}
