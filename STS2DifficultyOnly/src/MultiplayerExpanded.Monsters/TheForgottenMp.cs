using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class TheForgottenMp : MonsterRework
{
	public override Type BaseMonster => typeof(TheForgotten);

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		ChallengerDebuffIntent steal = new ChallengerDebuffIntent();
		MoveState val = new MoveState("MIASMA", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => MiasmaMove(monster, steal)), new AbstractIntent[3]
		{
			(AbstractIntent)steal,
			(AbstractIntent)new DefendIntent(),
			(AbstractIntent)new BuffIntent()
		});
		MoveState val2 = (MoveState)(object)(val.FollowUpState = (MonsterState)new MoveState("DREAD", MonsterRework.VanillaMove(monster, "DreadMove"), new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent((Func<decimal>)(() => MonsterRework.Vanilla<int>(monster, "DreadDamage"))) }));
		val2.FollowUpState = (MonsterState)(object)val;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val,
			(MonsterState)(object)val2
		}, (MonsterState)(object)val);
	}

	private static async Task MiasmaMove(MonsterModel monster, ChallengerDebuffIntent intent)
	{
		IntentAim aim = intent.Target.Aim(monster.Creature);
		ICombatState combatState = monster.Creature.CombatState;
		int amount = combatState.Players.Count;
		SfxCmd.Play(MonsterRework.Vanilla<string>(monster, "CastSfx"), 1f);
		await CreatureCmd.TriggerAnim(monster.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<DexterityPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)aim.Creatures, (decimal)(-amount), monster.Creature, (CardModel)null, false);
		await CreatureCmd.GainBlock(monster.Creature, 8m, (ValueProp)8, (CardPlay)null, false);
		await PowerCmd.Apply<DexterityPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), monster.Creature, (decimal)amount, monster.Creature, (CardModel)null, false);
	}
}
