using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MultiplayerExpanded.Features.MonsterReworks;

internal static class MonsterReworkPatches
{
	private static bool _installed;

	public static void Install(Harmony harmony)
	{
		if (_installed)
		{
			return;
		}
		_installed = true;
		MonsterReworkRegistry.Build();
		HashSet<MethodBase> hashSet = new HashSet<MethodBase>();
		HarmonyMethod val = Method("GenerateMoveStateMachinePrefix");
		HarmonyMethod val2 = Method("AfterAddedToRoomPostfix");
		foreach (Type baseType in MonsterReworkRegistry.BaseTypes)
		{
			MethodInfo methodInfo = AccessTools.DeclaredMethod(baseType, "GenerateMoveStateMachine", (Type[])null, (Type[])null);
			if (methodInfo == null)
			{
				MainFile.Logger.Error("[MonsterReworks] " + baseType.Name + ".GenerateMoveStateMachine not found; its move set stays vanilla.", 1);
			}
			else if (hashSet.Add(methodInfo))
			{
				RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
				harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			MethodInfo methodInfo2 = AfterAddedToRoomDeclaration(baseType);
			if (hashSet.Add(methodInfo2))
			{
				harmony.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, val2, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
		MainFile.Logger.Info($"[MonsterReworks] installed patches for {MonsterReworkRegistry.Count} reworked monster(s).", 1);
	}

	private static HarmonyMethod Method(string name)
	{
		return new HarmonyMethod(typeof(MonsterReworkPatches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
	}

	private static MethodInfo AfterAddedToRoomDeclaration(Type monsterType)
	{
		MethodInfo methodInfo = AccessTools.Method(monsterType, "AfterAddedToRoom", (Type[])null, (Type[])null);
		if (!(methodInfo.DeclaringType == methodInfo.ReflectedType))
		{
			return (MethodInfo)MethodBase.GetMethodFromHandle(methodInfo.MethodHandle, methodInfo.DeclaringType.TypeHandle);
		}
		return methodInfo;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool GenerateMoveStateMachinePrefix(MonsterModel __instance, ref MonsterMoveStateMachine __result)
	{
		if (!MonsterRework.IsMultiplayer(__instance) || !MonsterReworkRegistry.TryGet(((object)__instance).GetType(), out MonsterRework rework))
		{
			return true;
		}
		MonsterMoveStateMachine val = rework.GenerateMoveStateMachine(__instance);
		if (val == null)
		{
			return true;
		}
		__result = val;
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void AfterAddedToRoomPostfix(MonsterModel __instance, MethodBase __originalMethod, ref Task __result)
	{
		if (MonsterReworkRegistry.TryGet(((object)__instance).GetType(), out MonsterRework rework) && !(__originalMethod.DeclaringType != AfterAddedToRoomDeclaration(((object)__instance).GetType()).DeclaringType) && MonsterRework.IsMultiplayer(__instance))
		{
			__result = After(__result, __instance, rework);
		}
	}

	private static async Task After(Task vanilla, MonsterModel monster, MonsterRework rework)
	{
		await vanilla;
		await rework.AfterAddedToRoom(monster);
	}
}
