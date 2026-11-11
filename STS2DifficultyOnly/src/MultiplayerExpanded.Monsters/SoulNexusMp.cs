using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class SoulNexusMp : MonsterRework
{
	public override Type BaseMonster => typeof(SoulNexus);

	private static int DrainLifeDamage => AscensionHelper.GetValueIfAscension((AscensionLevel)9, 27, 25);

	private static int SoulBurnDamage(MonsterModel monster)
	{
		return AscensionHelper.GetValueIfAscension((AscensionLevel)9, 24, 18) * monster.Creature.CombatState.Players.Count;
	}

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		ChallengerAttackIntent soulBurnIntent = new ChallengerAttackIntent(() => SoulBurnDamage(monster));
		MoveState val = new MoveState("SOUL_BURN_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => SoulBurnMove(monster, soulBurnIntent)), new AbstractIntent[1] { (AbstractIntent)soulBurnIntent });
		MoveState val2 = new MoveState("MAELSTROM_MOVE", MonsterRework.VanillaMove(monster, "MaelstromMove"), new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(MonsterRework.Vanilla<int>(monster, "MaelstromDamage"), MonsterRework.Vanilla<int>(monster, "MaelstromRepeat")) });
		PatronizingAttackIntent drainIntent = new PatronizingAttackIntent(DrainLifeDamage);
		VengefulDebuffIntent debuffIntent = new VengefulDebuffIntent(strong: true);
		MoveState val3 = new MoveState("DRAIN_LIFE_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => DrainLifeMove(monster, drainIntent, debuffIntent)), new AbstractIntent[2]
		{
			(AbstractIntent)drainIntent,
			(AbstractIntent)debuffIntent
		});
		RandomBranchState val4 = (RandomBranchState)(object)(val3.FollowUpState = (val2.FollowUpState = (val.FollowUpState = (MonsterState)new RandomBranchState("RAND"))));
		val4.AddBranch((MonsterState)(object)val, (MoveRepeatType)2, 1f);
		val4.AddBranch((MonsterState)(object)val2, (MoveRepeatType)2, 1f);
		val4.AddBranch((MonsterState)(object)val3, (MoveRepeatType)2, 1f);
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val,
			(MonsterState)(object)val2,
			(MonsterState)(object)val3,
			(MonsterState)(object)val4
		}, (MonsterState)(object)val);
	}

	private static async Task SoulBurnMove(MonsterModel monster, ChallengerAttackIntent intent)
	{
		await intent.Target.Aim(monster.Creature).Attack(monster, SoulBurnDamage(monster)).WithAttackerAnim("Attack", 0.6f, (Creature)null)
			.WithAttackerFx((string)null, MonsterRework.Vanilla<string>(monster, "AttackSfx"), (string)null)
			.WithHitFx("vfx/vfx_attack_slash", (string)null, (string)null)
			.Execute((PlayerChoiceContext)null);
	}

	private static async Task DrainLifeMove(MonsterModel monster, PatronizingAttackIntent attackIntent, VengefulDebuffIntent debuffIntent)
	{
		IntentAim intentAim = attackIntent.Target.Aim(monster.Creature);
		IntentAim debuffAim = debuffIntent.Target.Aim(monster.Creature);
		await intentAim.Attack(monster, DrainLifeDamage).WithAttackerAnim("Cast", 1f, (Creature)null).WithAttackerFx((string)null, MonsterRework.Vanilla<string>(monster, "CastSfx"), (string)null)
			.Execute((PlayerChoiceContext)null);
		int reduction = ((monster.Creature.CombatState.Players.Count == 2) ? 1 : 0);
		IReadOnlyList<Creature> targets = debuffAim.Creatures;
		await PowerCmd.Apply<VulnerablePower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)targets, (decimal)(3 - reduction), monster.Creature, (CardModel)null, false);
		await PowerCmd.Apply<WeakPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)targets, (decimal)(3 - reduction), monster.Creature, (CardModel)null, false);
		if (AscensionHelper.HasAscension((AscensionLevel)9))
		{
			await PowerCmd.Apply<DebilitatePower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)targets, (decimal)(2 - reduction), monster.Creature, (CardModel)null, false);
		}
	}
}
