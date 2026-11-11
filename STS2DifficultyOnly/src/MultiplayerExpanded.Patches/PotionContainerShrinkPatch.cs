using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;

namespace MultiplayerExpanded.Patches;

[HarmonyPatch(typeof(NPotionContainer), "GrowPotionHolders")]
internal static class PotionContainerShrinkPatch
{
	private static readonly AccessTools.FieldRef<NPotionContainer, List<NPotionHolder>>? Holders = Field<List<NPotionHolder>>("_holders");

	private static readonly AccessTools.FieldRef<NPotionContainer, Player?>? Owner = Field<Player>("_player");

	private static readonly Action<NPotionContainer>? UpdateNavigation = ResolveUpdateNavigation();

	private static bool Prepare()
	{
		if (Holders != null && Owner != null)
		{
			return UpdateNavigation != null;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(NPotionContainer __instance, int newMaxPotionSlots)
	{
		if (!MultiplayerExpandedConfig.MultiplayerPotionChanges) return;
		try
		{
			List<NPotionHolder> list = Holders.Invoke(__instance);
			if (list.Count <= newMaxPotionSlots)
			{
				return;
			}
			Player val = Owner.Invoke(__instance);
			if (val == null)
			{
				return;
			}
			for (int num = list.Count - 1; num >= newMaxPotionSlots; num--)
			{
				((Node)list[num]).QueueFree();
				list.RemoveAt(num);
			}
			for (int i = 0; i < list.Count && i < val.PotionSlots.Count; i++)
			{
				PotionModel val2 = val.PotionSlots[i];
				if (val2 != null && !list[i].HasPotion)
				{
					NPotion val3 = NPotion.Create(val2);
					if (val3 != null)
					{
						((Control)val3).Position = new Vector2(-30f, -30f);
						list[i].AddPotion(val3);
					}
				}
			}
			UpdateNavigation(__instance);
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[PotionSlots] Could not remove the lost potion slot from the potion bar: {value}", 1);
		}
	}

	private static AccessTools.FieldRef<NPotionContainer, T>? Field<T>(string name)
	{
		if (AccessTools.Field(typeof(NPotionContainer), name) != null)
		{
			return AccessTools.FieldRefAccess<NPotionContainer, T>(name);
		}
		MainFile.Logger.Error("[PotionSlots] NPotionContainer." + name + " not found; lost potion slots stay on screen.", 1);
		return null;
	}

	private static Action<NPotionContainer>? ResolveUpdateNavigation()
	{
		MethodInfo methodInfo = AccessTools.Method(typeof(NPotionContainer), "UpdateNavigation", (Type[])null, (Type[])null);
		if (methodInfo != null)
		{
			return AccessTools.MethodDelegate<Action<NPotionContainer>>(methodInfo, (object)null, true, (Type[])null);
		}
		MainFile.Logger.Error("[PotionSlots] NPotionContainer.UpdateNavigation not found; lost potion slots stay on screen.", 1);
		return null;
	}
}
