using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch]
internal static class WaterfallGiantExplodePatch
{
	private static MethodBase? _target;

	private static PropertyInfo? _steamEruptionDamage;

	private static MethodInfo? _stopAmbientSfx;

	private static bool Prepare()
	{
		if ((object)_target == null)
		{
			_target = AccessTools.Method(typeof(WaterfallGiant), "ExplodeMove", new Type[1] { typeof(IReadOnlyList<Creature>) }, (Type[])null);
		}
		if ((object)_steamEruptionDamage == null)
		{
			_steamEruptionDamage = AccessTools.Property(typeof(WaterfallGiant), "SteamEruptionDamage");
		}
		if ((object)_stopAmbientSfx == null)
		{
			_stopAmbientSfx = AccessTools.Method(typeof(WaterfallGiant), "StopAmbientSfx", (Type[])null, (Type[])null);
		}
		if (_target == null || _steamEruptionDamage == null || _stopAmbientSfx == null)
		{
			MainFile.Logger.Error("[WaterfallGiantRework] WaterfallGiant.ExplodeMove, SteamEruptionDamage or StopAmbientSfx not found; the explosion stays vanilla.", 1);
			return false;
		}
		return true;
	}

	private static MethodBase TargetMethod()
	{
		return _target;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(WaterfallGiant __instance, ref Task __result)
	{
		if (!MonsterRework.IsMultiplayer((MonsterModel)(object)__instance))
		{
			return true;
		}
		__result = Explode(__instance);
		return false;
	}

	private static async Task Explode(WaterfallGiant giant)
	{
		int num = (int)_steamEruptionDamage.GetValue(giant);
		_stopAmbientSfx.Invoke(giant, null);
		int num2 = (from r in (await DamageCmd.Attack((decimal)num).FromMonster((MonsterModel)(object)giant).WithAttackerAnim("Erupt", 0.1f, (Creature)null)
				.WithAttackerFx((string)null, ((MonsterModel)giant).DeathSfx, (string)null)
				.Execute((PlayerChoiceContext)null)).Results.SelectMany((List<DamageResult> hit) => hit)
			where r.Receiver.IsPlayer && r.WasTargetKilled && r.Receiver.IsDead
			select r).Sum((DamageResult r) => r.OverkillDamage);
		if (num2 > 0 && ((MonsterModel)giant).CombatState.Players.Any((Player p) => p.Creature.IsAlive))
		{
			_steamEruptionDamage.SetValue(giant, num2);
			return;
		}
		await CreatureCmd.Kill(((MonsterModel)giant).Creature, false);
		NRunMusicController instance = NRunMusicController.Instance;
		if (instance != null)
		{
			instance.UpdateMusicParameter("waterfall_giant_progress", 5f);
		}
	}
}
