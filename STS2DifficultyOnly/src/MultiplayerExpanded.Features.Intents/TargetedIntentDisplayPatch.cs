using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BaseLib.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Features.Intents;

[HarmonyPatch(typeof(NIntent), "UpdateVisuals")]
internal static class TargetedIntentDisplayPatch
{
	private const string OverlayNodeName = "MpxIntentOverlay";

	private const string BadgeNodeName = "MpxIntentBadge";

	private const float BadgeSize = 36f;

	private const float BadgeOverhang = 6f;

	private const float IntentSize = 64f;

	private static readonly AccessTools.FieldRef<NIntent, AbstractIntent?>? IntentRef = Ref<AbstractIntent>("_intent");

	private static readonly AccessTools.FieldRef<NIntent, Creature?>? OwnerRef = Ref<Creature>("_owner");

	private static readonly AccessTools.FieldRef<NIntent, Control?>? HolderRef = Ref<Control>("_intentHolder");

	private static readonly AccessTools.FieldRef<NIntent, Sprite2D?>? SpriteRef = Ref<Sprite2D>("_intentSprite");

	private static readonly AccessTools.FieldRef<NIntent, MegaRichTextLabel?>? ValueRef = Ref<MegaRichTextLabel>("_valueLabel");

	private static readonly AccessTools.FieldRef<NIntent, List<Texture2D>>? FramesRef = Ref<List<Texture2D>>("_animationFrames");

	private static readonly Dictionary<string, Texture2D?> Textures = new Dictionary<string, Texture2D>();

	private static AccessTools.FieldRef<NIntent, T>? Ref<T>(string field)
	{
		if (AccessTools.Field(typeof(NIntent), field) != null)
		{
			return AccessTools.FieldRefAccess<NIntent, T>(field);
		}
		MainFile.Logger.Error("[Intents] NIntent." + field + " not found; targeted intents show as plain vanilla intents.", 1);
		return null;
	}

	private static bool Prepare()
	{
		if (IntentRef != null && OwnerRef != null && HolderRef != null && SpriteRef != null && ValueRef != null)
		{
			return FramesRef != null;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(NIntent __instance)
	{
		if (TestMode.IsOn)
		{
			return;
		}
		try
		{
			Control val = HolderRef.Invoke(__instance);
			if (val == null)
			{
				return;
			}
			if (IntentRef.Invoke(__instance) is ITargetedIntent targetedIntent)
			{
				Creature val2 = OwnerRef.Invoke(__instance);
				if (val2 != null)
				{
					UpdateOverlay(__instance, val, targetedIntent);
					UpdateBadge(__instance, val, targetedIntent.Target.Aim(val2).Shown);
					return;
				}
			}
			Remove((Node)(object)val, "MpxIntentOverlay");
			Remove((Node)(object)val, "MpxIntentBadge");
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[Intents] Targeted intent display failed: " + ex.Message, 1);
		}
	}

	private static void UpdateOverlay(NIntent intent, Control holder, ITargetedIntent targeted)
	{
		Texture2D val = Texture(("intents/" + targeted.OverlayStem + ".png").ImagePath());
		if (val != null)
		{
			Sprite2D val2 = SpriteRef.Invoke(intent);
			if (val2 != null)
			{
				Sprite2D val3 = ((Node)holder).GetNodeOrNull<Sprite2D>((NodePath)("MpxIntentOverlay"));
				if (val3 == null)
				{
					val3 = new Sprite2D
					{
						Name = (StringName)("MpxIntentOverlay")
					};
					((Node)holder).AddChild((Node)(object)val3, false, (InternalMode)0);
				}
				val3.Texture = val;
				Vector2 size = val.GetSize();
				List<Texture2D> list = FramesRef.Invoke(intent);
				Vector2 val4;
				if (list.Count <= 0)
				{
					Texture2D texture = val2.Texture;
					val4 = ((texture != null) ? texture.GetSize() : size);
				}
				else
				{
					val4 = list[0].GetSize();
				}
				Vector2 val5 = val4 / size;
				val3.Centered = val2.Centered;
				val3.Offset = val2.Offset / val5;
				((Node2D)val3).Scale = val5 * ((Node2D)val2).Scale;
				((Node2D)val3).Position = ((Node2D)val2).Position;
				return;
			}
		}
		Remove((Node)(object)holder, "MpxIntentOverlay");
	}

	private static void UpdateBadge(NIntent intent, Control holder, Player? player)
	{
		Texture2D val = ((player == null) ? null : BadgeTexture(player));
		if (val == null)
		{
			Remove((Node)(object)holder, "MpxIntentBadge");
			return;
		}
		TextureRect val2 = ((Node)holder).GetNodeOrNull<TextureRect>((NodePath)("MpxIntentBadge"));
		if (val2 == null)
		{
			val2 = new TextureRect
			{
				Name = (StringName)("MpxIntentBadge"),
				ExpandMode = (ExpandModeEnum)1,
				StretchMode = (StretchModeEnum)5,
				MouseFilter = (MouseFilterEnum)2
			};
			((Node)holder).AddChild((Node)(object)val2, false, (InternalMode)0);
		}
		val2.Texture = val;
		((Control)val2).Size = new Vector2(36f, 36f);
		((Control)val2).Position = new Vector2(34f, 34f);
		MegaRichTextLabel val3 = ValueRef.Invoke(intent);
		if (val3 != null && (object)((Node)val3).GetParent() == holder && ((Node)val2).GetIndex(false) > ((Node)val3).GetIndex(false))
		{
			((Node)holder).MoveChild((Node)(object)val2, ((Node)val3).GetIndex(false));
		}
	}

	private static Texture2D? BadgeTexture(Player player)
	{
		CharacterModel character = player.Character;
		string text = ((character != null) ? ((AbstractModel)character).Id.Entry : null);
		return (string.IsNullOrEmpty(text) ? null : Texture(("powers/" + BaseLib.Extensions.StringExtensions.RemovePrefix(text).ToLowerInvariant() + ".png").ImagePath())) ?? Texture("powers/power.png".ImagePath());
	}

	private static Texture2D? Texture(string path)
	{
		if (Textures.TryGetValue(path, out Texture2D value))
		{
			return value;
		}
		Texture2D val = null;
		if (ResourceLoader.Exists(path, ""))
		{
			val = ResourceLoader.Load<Texture2D>(path, (string)null, (CacheMode)1);
		}
		if (val == null)
		{
			MainFile.Logger.Info("[Intents] No texture at '" + path + "'; drawing the intent without it.", 1);
		}
		Textures[path] = val;
		return val;
	}

	private static void Remove(Node parent, string name)
	{
		Node nodeOrNull = parent.GetNodeOrNull((NodePath)(name));
		if (nodeOrNull != null)
		{
			parent.RemoveChild(nodeOrNull);
			nodeOrNull.QueueFree();
		}
	}
}
