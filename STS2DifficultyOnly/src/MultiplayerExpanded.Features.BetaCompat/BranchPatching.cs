using System;
using System.Collections.Generic;
using HarmonyLib;

namespace MultiplayerExpanded.Features.BetaCompat;

public static class BranchPatching
{
	public static readonly List<string> Failed = new List<string>();

	public static void PatchAll(Harmony harmony)
	{
		Type[] typesFromAssembly = AccessTools.GetTypesFromAssembly(typeof(BranchPatching).Assembly);
		foreach (Type type in typesFromAssembly)
		{
			if (BranchFilter.Allows(type) && GeneralExtensions.HasHarmonyAttribute(type))
			{
				try
				{
					harmony.CreateClassProcessor(type).Patch();
				}
				catch (Exception ex)
				{
					Failed.Add(type.FullName + ": " + ex.GetBaseException().Message);
					MainFile.Logger.Error($"[BetaCompat] patch class {type.FullName} failed: {ex}", 1);
				}
			}
		}
		MainFile.Logger.Info($"[BetaCompat] patching done; {Failed.Count} class(es) failed.", 1);
	}
}
