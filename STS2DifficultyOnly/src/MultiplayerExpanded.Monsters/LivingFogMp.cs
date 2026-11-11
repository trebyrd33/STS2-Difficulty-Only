using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

public sealed class LivingFogMp : MonsterRework
{
	public override Type BaseMonster => typeof(LivingFog);

	public override MonsterMoveStateMachine? GenerateMoveStateMachine(MonsterModel monster)
	{
		MoveState val = new MoveState("ADVANCED_GAS_MOVE", MonsterRework.VanillaMove(monster, "AdvancedGasMove"), new AbstractIntent[2]
		{
			(AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>(monster, "AdvancedGasDamage")),
			(AbstractIntent)new CardDebuffIntent()
		});
		MoveState val2 = new MoveState("BLOAT_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> targets) => BloatMove(monster, targets)), new AbstractIntent[2]
		{
			(AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>(monster, "BloatDamage")),
			(AbstractIntent)new SummonIntent()
		});
		MoveState val3 = new MoveState("SUPER_GAS_BLAST_MOVE", MonsterRework.VanillaMove(monster, "SuperGasBlastMove"), new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>(monster, "SuperGasBlastDamage")) });
		val.FollowUpState = (MonsterState)(object)val2;
		val2.FollowUpState = (MonsterState)(object)val3;
		val3.FollowUpState = (MonsterState)(object)val2;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val,
			(MonsterState)(object)val3,
			(MonsterState)(object)val2
		}, (MonsterState)(object)val);
	}

	private static async Task BloatMove(MonsterModel monster, IReadOnlyList<Creature> targets)
	{
		ICombatState combatState = monster.CombatState;
		SfxCmd.Play(MonsterRework.Vanilla<string>(monster, "_summonSfx"), 1f);
		await CreatureCmd.TriggerAnim(monster.Creature, "SpawnBomb", 0.35f);
		foreach (Player player in combatState.Players)
		{
			if (player.Creature.IsAlive)
			{
				EncounterModel encounter = combatState.Encounter;
				string text = ((encounter != null) ? encounter.GetNextSlot(combatState) : null);
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				SfxCmd.Play(MonsterRework.Vanilla<string>(monster, "_appearsSfx"), 1f);
				await SummonBomb(combatState, text, player.Creature);
			}
		}
		await DamageCmd.Attack((decimal)MonsterRework.Vanilla<int>(monster, "BloatDamage")).FromMonster(monster).WithAttackerAnim("Attack", 0.1f, (Creature)null)
			.WithAttackerFx((string)null, MonsterRework.Vanilla<string>(monster, "_attackBlowSfx"), (string)null)
			.WithHitVfxNode((Func<Creature, Node2D>)((Creature _) =>
			{
				return (Node2D?)(object)NGaseousImpactVfx.Create((CombatSide)1, combatState, new Color("#402f45"));
			}))
			.Execute((PlayerChoiceContext)null);
	}

	private static async Task SummonBomb(ICombatState combatState, string slot, Creature target)
	{
		Creature bomb = combatState.CreateCreature(((MonsterModel)ModelDb.Monster<GasBomb>()).ToMutable(), (CombatSide)2, slot);
		int valueIfAscension = AscensionHelper.GetValueIfAscension((AscensionLevel)8, 12, 10);
		bomb.SetMaxHpInternal((decimal)valueIfAscension);
		bomb.SetCurrentHpInternal((decimal)valueIfAscension);
		await CreatureCmd.Add(bomb);
		try
		{
			GasBombTargetPower gasBombTargetPower = (GasBombTargetPower)(object)((PowerModel)ModelDb.Power<GasBombTargetPower>()).ToMutable(0);
			gasBombTargetPower.BombTarget = target;
			await PowerCmd.Apply((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (PowerModel)(object)gasBombTargetPower, bomb, 1m, bomb, (CardModel)null, false);
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[MonsterReworks] Gas Bomb target marker failed: " + ex.Message, 1);
		}
	}
}
