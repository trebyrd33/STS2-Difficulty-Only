using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class BowlbugRockMp : MonsterRework
{
	private static readonly MethodInfo? StunMethod = AccessTools.Method(typeof(BowlbugRock), "Stun", (Type[])null, (Type[])null);

	public override Type BaseMonster => typeof(BowlbugRock);

	private static int HeadbuttDamage(MonsterModel monster)
	{
		return AscensionHelper.GetValueIfAscension((AscensionLevel)9, 13, 12) * monster.Creature.CombatState.Players.Count;
	}

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		BowlbugRock rock = (BowlbugRock)monster;
		ChosenAttackIntent headbuttIntent = new ChosenAttackIntent(() => HeadbuttDamage(monster), RandomPlayerThisRound);
		MoveState val = new MoveState("HEADBUTT_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => HeadbuttMove(rock, headbuttIntent)), new AbstractIntent[1] { (AbstractIntent)headbuttIntent });
		MoveState val2 = new MoveState("DIZZY_MOVE", MonsterRework.VanillaMove(monster, "DizzyMove"), new AbstractIntent[1] { (AbstractIntent)new StunIntent() });
		ConditionalBranchState val3 = (ConditionalBranchState)(object)(val.FollowUpState = (MonsterState)new ConditionalBranchState("POST_HEADBUTT"));
		val2.FollowUpState = (MonsterState)(object)val;
		val3.AddState((MonsterState)(object)val2, (Func<bool>)(() => rock.IsOffBalance));
		val3.AddState((MonsterState)(object)val, (Func<bool>)(() => !rock.IsOffBalance));
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val2,
			(MonsterState)(object)val3,
			(MonsterState)(object)val
		}, (MonsterState)(object)val);
	}

	private static Player? RandomPlayerThisRound(Creature owner)
	{
		ICombatState combatState = owner.CombatState;
		if (combatState == null)
		{
			return null;
		}
		IRunState runState = combatState.RunState;
		List<Player> list = combatState.Players.ToList();
		string name = $"mpx_bowlbugrock_{runState.TotalFloor}_{combatState.RoundNumber}_{owner.CombatId.GetValueOrDefault()}";
		GameApi.NewRng(GameApi.Seed(runState.Rng), name).Shuffle<Player>((IList<Player>)list);
		return list.FirstOrDefault((Player p) => p.Creature.IsAlive);
	}

	private static async Task HeadbuttMove(BowlbugRock rock, ChosenAttackIntent intent)
	{
		await intent.Target.Aim(((MonsterModel)rock).Creature).Attack((MonsterModel)(object)rock, HeadbuttDamage((MonsterModel)(object)rock)).WithAttackerAnim("Attack", 0.3f, (Creature)null)
			.WithAttackerFx((string)null, MonsterRework.Vanilla<string>((MonsterModel)(object)rock, "AttackSfx"), (string)null)
			.WithHitFx("vfx/vfx_attack_blunt", (string)null, (string)null)
			.Execute((PlayerChoiceContext)null);
		if (rock.IsOffBalance)
		{
			await Stun(rock);
		}
	}

	private static Task Stun(BowlbugRock rock)
	{
		if (StunMethod != null)
		{
			return (Task)StunMethod.Invoke(rock, null);
		}
		MainFile.Logger.Error("[MonsterReworks] BowlbugRock.Stun not found; the stun plays without its sfx and animation.", 1);
		return CreatureCmd.Stun(((MonsterModel)rock).Creature, MonsterRework.VanillaMove((MonsterModel)(object)rock, "DizzyMove"), (string)null);
	}
}
