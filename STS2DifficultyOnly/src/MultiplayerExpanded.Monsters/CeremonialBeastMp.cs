using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

public sealed class CeremonialBeastMp : MonsterRework
{
	public override Type BaseMonster => typeof(CeremonialBeast);

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		CeremonialBeast beast = (CeremonialBeast)monster;
		MoveState val = new MoveState("STAMP_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)beast, "StampMove"), new AbstractIntent[1] { (AbstractIntent)new BuffIntent() });
		MoveState val2 = new MoveState("PLOW_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)beast, "PlowMove"), new AbstractIntent[2]
		{
			(AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)beast, "PlowDamage")),
			(AbstractIntent)new BuffIntent()
		});
		MoveState val3 = new MoveState("STUN_MOVE", (Func<IReadOnlyList<Creature>, Task>)beast.StunnedMove, new AbstractIntent[1] { (AbstractIntent)new StunIntent() })
		{
			MustPerformOnceBeforeTransitioning = true
		};
		beast.BeastCryState = new MoveState("BEAST_CRY_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)beast, "BeastCryMove"), new AbstractIntent[1] { (AbstractIntent)new DebuffIntent(false) });
		VengefulDebuffIntent stompRinging = new VengefulDebuffIntent();
		Func<IReadOnlyList<Creature>, Task> vanillaStomp = MonsterRework.VanillaMove((MonsterModel)(object)beast, "StompMove");
		MoveState val4 = new MoveState("STOMP_MOVE", (Func<IReadOnlyList<Creature>, Task>)(async (IReadOnlyList<Creature> targets) =>
		{
			IntentAim aim = stompRinging.Target.Aim(((MonsterModel)beast).Creature);
			await vanillaStomp(targets);
			await ApplyRinging(beast, aim);
		}), new AbstractIntent[2]
		{
			(AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)beast, "StompDamage")),
			(AbstractIntent)stompRinging
		});
		VengefulDebuffIntent crushRinging = new VengefulDebuffIntent();
		Func<IReadOnlyList<Creature>, Task> vanillaCrush = MonsterRework.VanillaMove((MonsterModel)(object)beast, "CrushMove");
		MoveState val5 = new MoveState("CRUSH_MOVE", (Func<IReadOnlyList<Creature>, Task>)(async (IReadOnlyList<Creature> targets) =>
		{
			IntentAim aim = crushRinging.Target.Aim(((MonsterModel)beast).Creature);
			await vanillaCrush(targets);
			await ApplyRinging(beast, aim);
		}), new AbstractIntent[3]
		{
			(AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)beast, "CrushDamage")),
			(AbstractIntent)new BuffIntent(),
			(AbstractIntent)crushRinging
		});
		val.FollowUpState = (MonsterState)(object)val2;
		val2.FollowUpState = (MonsterState)(object)val2;
		val3.FollowUpState = (MonsterState)(object)beast.BeastCryState;
		beast.BeastCryState.FollowUpState = (MonsterState)(object)val4;
		val4.FollowUpState = (MonsterState)(object)val5;
		val5.FollowUpState = (MonsterState)(object)beast.BeastCryState;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val2,
			(MonsterState)(object)val,
			(MonsterState)(object)val3,
			(MonsterState)(object)beast.BeastCryState,
			(MonsterState)(object)val4,
			(MonsterState)(object)val5
		}, (MonsterState)(object)val);
	}

	private static async Task ApplyRinging(CeremonialBeast beast, IntentAim aim)
	{
		try
		{
			IReadOnlyList<Creature> creatures = aim.Creatures;
			if (creatures.Count != 0)
			{
				await PowerCmd.Apply<RingingPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)creatures, 1m, ((MonsterModel)beast).Creature, (CardModel)null, false);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[CeremonialBeastMp] Ringing failed: " + ex.Message, 1);
		}
	}
}
