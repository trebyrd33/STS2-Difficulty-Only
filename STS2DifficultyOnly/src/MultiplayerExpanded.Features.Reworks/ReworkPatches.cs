using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Cards;

namespace MultiplayerExpanded.Features.Reworks;

internal static class ReworkPatches
{
	private static bool _installed;

	internal static bool CompendiumPreview;
	private static bool _lastEnabled = true;
	internal static void RefreshSettings() {
	 if (_lastEnabled == MultiplayerExpandedConfig.VanillaCardReworks) return;
	 _lastEnabled = MultiplayerExpandedConfig.VanillaCardReworks;
	 if (ReworkRegistry.Built) ReworkStateSync.RebakeTemplates();
	}

	internal static bool IsMp() => MultiplayerExpandedConfig.VanillaCardReworks && IsMultiplayerRun();

	internal static bool IsMultiplayerRun()
	{
		RunManager instance = RunManager.Instance;
		if (instance == null)
		{
			return false;
		}
		INetGameService netService = instance.NetService;
		return ((netService != null) ? new bool?(NetGameTypeExtensions.IsMultiplayer(netService.Type)) : ((bool?)null)) == true;
	}

	internal static bool ShowReworked()
	{
		if (!MultiplayerExpandedConfig.VanillaCardReworks) return false;
		if (!IsMp())
		{
			return CompendiumPreview;
		}
		return true;
	}

	public static void Install(Harmony harmony)
	{
		if (_installed)
		{
			return;
		}
		_installed = true;
		ReworkRegistry.Build();
		HashSet<MethodBase> patched = new HashSet<MethodBase>();
		HarmonyMethod patch = Method("OnPlayPrefix");
		HarmonyMethod patch2 = Method("OnUpgradePrefix");
		HarmonyMethod patch3 = Method("OnTurnEndInHandPrefix");
		HarmonyMethod patch4 = Method("CanonicalVarsPostfix");
		HarmonyMethod patch5 = Method("CanonicalKeywordsPostfix");
		HarmonyMethod patch6 = Method("CanonicalStarCostPostfix");
		HarmonyMethod patch7 = Method("GainsBlockPostfix");
		HarmonyMethod patch8 = Method("ExtraHoverTipsPostfix");
		HarmonyMethod patch9 = Method("EnergyCostInCombatPostfix");
		foreach (Type baseType in ReworkRegistry.BaseTypes)
		{
			PatchPrefix(Implemented(AccessTools.Method(baseType, "OnPlay", (Type[])null, (Type[])null)), patch);
			PatchPrefix(Implemented(AccessTools.Method(baseType, "OnUpgrade", (Type[])null, (Type[])null)), patch2);
			PatchPrefix(Implemented(AccessTools.Method(baseType, "OnTurnEndInHand", (Type[])null, (Type[])null)), patch3);
			PatchPostfix(Implemented(AccessTools.PropertyGetter(baseType, "CanonicalVars")), patch4);
			PatchPostfix(Implemented(AccessTools.PropertyGetter(baseType, "CanonicalKeywords")), patch5);
			PatchPostfix(Implemented(AccessTools.PropertyGetter(baseType, "CanonicalStarCost")), patch6);
			PatchPostfix(Implemented(AccessTools.PropertyGetter(baseType, "GainsBlock")), patch7);
			PatchPostfix(Implemented(AccessTools.PropertyGetter(baseType, "ExtraHoverTips")), patch8);
			if (ReworkRegistry.TryGet(baseType, out ReworkInfo info) && HasPartyScaledCost(info.Rework))
			{
				PatchPostfix(Implemented(AccessTools.Method(baseType, "TryModifyEnergyCostInCombat", (Type[])null, (Type[])null)), patch9);
			}
		}
		MainFile.Logger.Info($"[Reworks] installed patches for {ReworkRegistry.Count} reworked card(s).", 1);
		static MethodBase? Implemented(MethodInfo? m)
		{
			if (m == null)
			{
				return null;
			}
			if (m.DeclaringType == m.ReflectedType)
			{
				return m;
			}
			return MethodBase.GetMethodFromHandle(m.MethodHandle, m.DeclaringType.TypeHandle);
		}
		void PatchPostfix(MethodBase? target, HarmonyMethod val)
		{
			if (!(target == null) && patched.Add(target))
			{
				harmony.Patch(target, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
		void PatchPrefix(MethodBase? target, HarmonyMethod val)
		{
			if (!(target == null) && patched.Add(target))
			{
				RuntimeHelpers.PrepareMethod(target.MethodHandle);
				harmony.Patch(target, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	private static HarmonyMethod Method(string name)
	{
		return new HarmonyMethod(typeof(ReworkPatches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool OnPlayPrefix(CardModel __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		if (!IsMp() || !ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			return true;
		}
		__result = info.Rework.OnPlay(__instance, choiceContext, cardPlay);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool OnUpgradePrefix(CardModel __instance)
	{
		if (!ShowReworked() || !ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			return true;
		}
		info.Rework.OnUpgrade(__instance);
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool OnTurnEndInHandPrefix(CardModel __instance, PlayerChoiceContext choiceContext, ref Task __result)
	{
		if (!IsMp() || !ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			return true;
		}
		Task task = info.Rework.OnTurnEndInHand(__instance, choiceContext);
		if (task == null)
		{
			return true;
		}
		__result = task;
		return false;
	}

	internal static IEnumerable<MultiplayerExpandedCard.PartyScaledVar>? PartyScaledRulesFor(CardModel card)
	{
		if (!ShowReworked() || !ReworkRegistry.TryGet(((object)card).GetType(), out ReworkInfo info))
		{
			return null;
		}
		return info.Rework.PartyScaledVars;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CanonicalVarsPostfix(CardModel __instance, ref IEnumerable<DynamicVar> __result)
	{
		if (ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			__result = info.Rework.Vars;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CanonicalKeywordsPostfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
	{
		if (ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			__result = info.Rework.Keywords;
		}
	}

	private static bool HasPartyScaledCost(CardRework rework)
	{
		return rework.GetType().GetMethod("EnergyCostForPlayerCount")?.DeclaringType != typeof(CardRework);
	}

	internal static bool HasPartyScaledCost(CardModel card)
	{
		if (ShowReworked() && ReworkRegistry.TryGet(((object)card).GetType(), out ReworkInfo info))
		{
			return HasPartyScaledCost(info.Rework);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void EnergyCostInCombatPostfix(AbstractModel __instance, CardModel card, decimal originalCost, ref decimal modifiedCost, ref bool __result)
	{
		if ((object)__instance != card || originalCost <= 0m || card.EnergyCost.CostsX || !ShowReworked() || !ReworkRegistry.TryGet(((object)card).GetType(), out ReworkInfo info))
		{
			return;
		}
		ICombatState combatState = card.CombatState;
		int num = ((combatState != null) ? combatState.Players.Count : 0);
		if (num <= 0)
		{
			return;
		}
		int? num2 = info.Rework.EnergyCostForPlayerCount(card, num);
		if (num2.HasValue)
		{
			int num3 = Math.Max(0, num2.Value) - card.EnergyCost.GetWithModifiers((CostModifiers)0);
			if (num3 != 0)
			{
				modifiedCost = Math.Max(0m, modifiedCost + (decimal)num3);
				__result |= modifiedCost != originalCost;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void CanonicalStarCostPostfix(CardModel __instance, ref int __result)
	{
		if (ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			int? starCost = info.Rework.StarCost;
			if (starCost.HasValue)
			{
				int valueOrDefault = starCost.GetValueOrDefault();
				__result = valueOrDefault;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void GainsBlockPostfix(CardModel __instance, ref bool __result)
	{
		if (ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			__result = info.Rework.GainsBlock;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ExtraHoverTipsPostfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
	{
		if (ShowReworked() && ReworkRegistry.TryGet(((object)__instance).GetType(), out ReworkInfo info))
		{
			__result = info.Rework.HoverTipsFor(__instance);
		}
	}
}
