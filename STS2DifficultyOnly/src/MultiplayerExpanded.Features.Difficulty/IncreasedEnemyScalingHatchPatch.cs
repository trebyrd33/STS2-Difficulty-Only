using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MultiplayerExpanded.Features.Difficulty;

[HarmonyPatch]
internal static class IncreasedEnemyScalingHatchPatch
{
	private static readonly MethodInfo? Hatch = AccessTools.Method(typeof(ToughEgg), "Hatch", (Type[])null, (Type[])null);

	private static bool Prepare()
	{
		if (Hatch != null)
		{
			return true;
		}
		MainFile.Logger.Error("[Difficulty] ToughEgg.Hatch not found; hatchlings get the increased HP scaling.", 1);
		return false;
	}

	private static MethodBase TargetMethod()
	{
		return AccessTools.AsyncMoveNext((MethodBase)Hatch);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(out int __state)
	{
		__state = IncreasedEnemyScalingPatch.Enter();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Exception? Finalizer(Exception? __exception, int __state)
	{
		IncreasedEnemyScalingPatch.Exit(__state);
		return __exception;
	}
}
