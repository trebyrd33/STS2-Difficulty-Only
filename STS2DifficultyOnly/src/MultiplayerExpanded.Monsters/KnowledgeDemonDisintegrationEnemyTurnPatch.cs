using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(Hook), "AfterSideTurnStart")]
internal static class KnowledgeDemonDisintegrationEnemyTurnPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(ref Task __result, ICombatState combatState, CombatSide side)
	{
		if ((int)side == 2 && MonsterRework.IsMultiplayer(combatState))
		{
			__result = DisintegrateAfter(__result, combatState);
		}
	}

	private static async Task DisintegrateAfter(Task original, ICombatState combatState)
	{
		await original;
		try
		{
			BlockingPlayerChoiceContext choiceContext = new BlockingPlayerChoiceContext();
			foreach (Player player in combatState.Players)
			{
				Creature creature = player.Creature;
				if (creature.IsAlive)
				{
					DisintegrationPower power = creature.GetPower<DisintegrationPower>();
					if (power != null)
					{
						await GameApi.Damage((PlayerChoiceContext)(object)choiceContext, creature, ((PowerModel)power).Amount, (ValueProp)4, creature, null, null);
						VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_attack_blunt");
					}
				}
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[MonsterReworks] Knowledge Demon: Disintegration damage failed: {value}", 1);
		}
	}
}
