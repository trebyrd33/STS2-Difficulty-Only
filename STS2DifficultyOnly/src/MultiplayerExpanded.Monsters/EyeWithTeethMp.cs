using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MultiplayerExpanded.Features.Intents;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class EyeWithTeethMp : MonsterRework
{
	public override Type BaseMonster => typeof(EyeWithTeeth);

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		int count = MonsterRework.Vanilla<int>(monster, "_distractAmount") * monster.CombatState.Players.Count;
		VengefulStatusIntent intent = new VengefulStatusIntent(count);
		MoveState val = new MoveState("DISTRACT_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => Distract(monster, intent, count)), new AbstractIntent[1] { (AbstractIntent)intent });
		val.FollowUpState = (MonsterState)(object)val;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState> { (MonsterState)(object)val }, (MonsterState)(object)val);
	}

	private static async Task Distract(MonsterModel monster, VengefulStatusIntent intent, int count)
	{
		IntentAim aim = intent.Target.Aim(monster.Creature);
		SfxCmd.Play(MonsterRework.Vanilla<string>(monster, "AttackSfx"), 1f);
		await CreatureCmd.TriggerAnim(monster.Creature, "Attack", 0.7f);
		IReadOnlyList<Creature> creatures = aim.Creatures;
		if (creatures.Count != 0)
		{
			VfxCmd.PlayOnCreatureCenters((IEnumerable<Creature>)creatures, "vfx/vfx_attack_slash");
			await CardPileCmd.AddToCombatAndPreview<Dazed>((IEnumerable<Creature>)creatures, (PileType)3, count, (Player)null, (CardPilePosition)1);
		}
	}
}
