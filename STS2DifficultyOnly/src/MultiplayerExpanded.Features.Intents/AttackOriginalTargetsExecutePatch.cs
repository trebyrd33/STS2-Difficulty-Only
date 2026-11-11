using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace MultiplayerExpanded.Features.Intents;

[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class AttackOriginalTargetsExecutePatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Prefix(AttackCommand __instance, out AttackOriginalTargets.Frame? __state)
	{
		__state = AttackOriginalTargets.Begin(__instance);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(AttackOriginalTargets.Frame? __state, ref Task<AttackCommand> __result)
	{
		if (__state != null)
		{
			__result = EndAfter(__result, __state);
		}
	}

	private static async Task<AttackCommand> EndAfter(Task<AttackCommand> execute, AttackOriginalTargets.Frame frame)
	{
		try
		{
			return await execute;
		}
		finally
		{
			AttackOriginalTargets.End(frame);
		}
	}
}
