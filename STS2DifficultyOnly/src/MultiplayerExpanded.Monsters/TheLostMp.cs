using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class TheLostMp : MonsterRework
{
	public override Type BaseMonster => typeof(TheLost);

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		VengefulDebuffIntent steal = new VengefulDebuffIntent();
		MoveState val = new MoveState("DEBILITATING_SMOG", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => DebilitatingSmogMove(monster, steal)), new AbstractIntent[2]
		{
			(AbstractIntent)steal,
			(AbstractIntent)new BuffIntent()
		});
		MoveState val2 = (MoveState)(object)(val.FollowUpState = (MonsterState)new MoveState("EYE_LASERS", MonsterRework.VanillaMove(monster, "EyeLasersMove"), new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(MonsterRework.Vanilla<int>(monster, "EyeLasersDamage"), 2) }));
		val2.FollowUpState = (MonsterState)(object)val;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val,
			(MonsterState)(object)val2
		}, (MonsterState)(object)val);
	}

	private static async Task DebilitatingSmogMove(MonsterModel monster, VengefulDebuffIntent intent)
	{
		IntentAim aim = intent.Target.Aim(monster.Creature);
		ICombatState combatState = monster.Creature.CombatState;
		int amount = combatState.Players.Count;
		SfxCmd.Play(MonsterRework.Vanilla<string>(monster, "CastSfx"), 1f);
		await CreatureCmd.TriggerAnim(monster.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)aim.Creatures, (decimal)(-amount), monster.Creature, (CardModel)null, false);
		await PowerCmd.Apply<StrengthPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), monster.Creature, (decimal)amount, monster.Creature, (CardModel)null, false);
	}
}
