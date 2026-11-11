using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.DeathMechanic;

[HarmonyPatch(typeof(CreatureCmd), "Kill", new Type[]
{
	typeof(IReadOnlyCollection<Creature>),
	typeof(bool)
})]
internal static class EveryoneDiesPatch
{
	private static bool _wiping;

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(IReadOnlyCollection<Creature> creatures, ref Task __result)
	{
		__result = WipeAfter(__result, creatures);
	}

	private static async Task WipeAfter(Task original, IReadOnlyCollection<Creature> creatures)
	{
		await original;
		if (_wiping || RunManager.Instance.IsAbandoned)
		{
			return;
		}
		Creature? val = creatures.FirstOrDefault((Creature c) => c.IsPlayer && c.IsDead);
		Player obj = ((val != null) ? val.Player : null);
		IRunState val2 = ((obj != null) ? obj.RunState : null);
		if (!DeathMechanicGate.IsActive(MultiplayerDeathMode.EveryoneDies, val2))
		{
			return;
		}
		List<Creature> list = (from p in ((IPlayerCollection)val2).Players
			select p.Creature into c
			where c != null && c.IsAlive
			select c).ToList();
		if (list.Count == 0)
		{
			return;
		}
		_wiping = true;
		try
		{
			await CreatureCmd.Kill((IReadOnlyCollection<Creature>)list, true);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[DeathMechanic] EveryoneDies wipe failed: " + ex.Message, 1);
		}
		finally
		{
			_wiping = false;
		}
	}
}
