using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

public sealed class GasBombMp : MonsterRework
{
	private static readonly MethodInfo? HasExplodedSetter = AccessTools.PropertySetter(typeof(GasBomb), "HasExploded");

	public override Type BaseMonster => typeof(GasBomb);

	public override MonsterMoveStateMachine? GenerateMoveStateMachine(MonsterModel monster)
	{
		int damage = MonsterRework.Vanilla<int>(monster, "ExplodeDamage");
		TargetedDeathBlowIntent intent = new TargetedDeathBlowIntent(damage, AssignedPlayer);
		MoveState val = new MoveState("EXPLODE_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => ExplodeMove(monster, intent, damage)), new AbstractIntent[1] { (AbstractIntent)intent });
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState> { (MonsterState)(object)val }, (MonsterState)(object)val);
	}

	private static Player? AssignedPlayer(Creature bomb)
	{
		GasBombTargetPower power = bomb.GetPower<GasBombTargetPower>();
		if (power == null)
		{
			return null;
		}
		Creature? bombTarget = power.BombTarget;
		if (bombTarget == null)
		{
			return null;
		}
		return bombTarget.Player;
	}

	private static async Task ExplodeMove(MonsterModel bomb, TargetedDeathBlowIntent intent, int damage)
	{
		IntentAim intentAim = intent.Target.Aim(bomb.Creature);
		if (HasExplodedSetter != null)
		{
			HasExplodedSetter.Invoke(bomb, new object[1] { true });
		}
		else
		{
			MainFile.Logger.Error("[MonsterReworks] GasBomb.HasExploded setter not found; the bomb plays its death animation.", 1);
		}
		await intentAim.Attack(bomb, damage).WithAttackerAnim("ExplodeTrigger", 0.1f, (Creature)null).WithAttackerFx((string)null, MonsterRework.Vanilla<string>(bomb, "_explodeSfx"), (string)null)
			.WithHitVfxNode((Func<Creature, Node2D>)((Creature hit) =>
			{
				return (Node2D?)(object)NGaseousImpactVfx.Create(hit, new Color("#402f45"));
			}))
			.Execute((PlayerChoiceContext)null);
		await CreatureCmd.Kill(bomb.Creature, false);
	}
}
