using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MultiplayerExpanded.Features.Columns;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

public sealed class QueenMp : MonsterRework
{
	public const string FrozenInPlaceMoveId = "FROZEN_IN_PLACE_MOVE";

	public const string EnrageMoveId = "ENRAGE_MOVE";

	private static readonly AccessTools.FieldRef<Queen, bool> HasAmalgamDiedRef = AccessTools.FieldRefAccess<Queen, bool>("_hasAmalgamDied");

	private static readonly AccessTools.FieldRef<Queen, MoveState> BurnBrightForMeStateRef = AccessTools.FieldRefAccess<Queen, MoveState>("_burnBrightForMeState");

	private static readonly AccessTools.FieldRef<Queen, MoveState> EnragedStateRef = AccessTools.FieldRefAccess<Queen, MoveState>("_enragedState");

	public override Type BaseMonster => typeof(Queen);

	public override MonsterMoveStateMachine? GenerateMoveStateMachine(MonsterModel monster)
	{
		if (ColumnCombat.Of(monster.Creature.CombatState) == null)
		{
			return null;
		}
		Queen queen = (Queen)monster;
		MoveState val = new MoveState("PUPPET_STRINGS_MOVE", OnParty(queen, "PuppetStringsMove"), new AbstractIntent[1] { (AbstractIntent)ColumnCombat.PartyWide<CardDebuffIntent>(new CardDebuffIntent()) });
		MoveState val2 = new MoveState("YOU_ARE_MINE_MOVE", OnParty(queen, "YoureMineMove"), new AbstractIntent[1] { (AbstractIntent)ColumnCombat.PartyWide<DebuffIntent>(new DebuffIntent(false)) });
		MoveState burnBright = new MoveState("BURN_BRIGHT_FOR_ME_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)queen, "BurnBrightForMeMove"), new AbstractIntent[2]
		{
			(AbstractIntent)new BuffIntent(),
			(AbstractIntent)new DefendIntent()
		});
		MoveState frozenInPlace = new MoveState("FROZEN_IN_PLACE_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => FrozenInPlaceMove(queen)), new AbstractIntent[1] { (AbstractIntent)ColumnCombat.PartyWide<DebuffIntent>(new DebuffIntent(false)) });
		MoveState val3 = new MoveState("OFF_WITH_YOUR_HEAD_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)queen, "OffWithYourHeadMove"), new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)queen, "OffWithYourHeadDamage"), 5) });
		MoveState val4 = new MoveState("EXECUTION_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)queen, "ExecutionMove"), new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)queen, "ExecutionDamage")) });
		Func<IReadOnlyList<Creature>, Task> vanillaEnrage = MonsterRework.VanillaMove((MonsterModel)(object)queen, "EnrageMove");
		MoveState val5 = new MoveState("ENRAGE_MOVE", (Func<IReadOnlyList<Creature>, Task>)(async (IReadOnlyList<Creature> targets) =>
		{
			await vanillaEnrage(targets);
			await FreezeParty(queen);
		}), new AbstractIntent[2]
		{
			(AbstractIntent)new BuffIntent(),
			(AbstractIntent)ColumnCombat.PartyWide<DebuffIntent>(new DebuffIntent(false))
		});
		BurnBrightForMeStateRef.Invoke(queen) = burnBright;
		EnragedStateRef.Invoke(queen) = val5;
		ConditionalBranchState val6 = new ConditionalBranchState("YOURE_MINE_NOW_BRANCH");
		ConditionalBranchState val7 = new ConditionalBranchState("BURN_BRIGHT_FOR_ME_BRANCH");
		ConditionalBranchState[] array = new ConditionalBranchState[2] { val6, val7 };
		foreach (ConditionalBranchState obj in array)
		{
			obj.AddState((MonsterState)(object)frozenInPlace, (Func<bool>)(() => !AmalgamDied() && FreezeTurn()));
			obj.AddState((MonsterState)(object)burnBright, (Func<bool>)(() => !AmalgamDied()));
			obj.AddState((MonsterState)(object)val3, (Func<bool>)AmalgamDied);
		}
		val.FollowUpState = (MonsterState)(object)val2;
		val2.FollowUpState = (MonsterState)(object)val6;
		burnBright.FollowUpState = (MonsterState)(object)val7;
		frozenInPlace.FollowUpState = (MonsterState)(object)val7;
		val3.FollowUpState = (MonsterState)(object)val4;
		val4.FollowUpState = (MonsterState)(object)val5;
		val5.FollowUpState = (MonsterState)(object)val3;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val,
			(MonsterState)(object)val2,
			(MonsterState)(object)burnBright,
			(MonsterState)(object)frozenInPlace,
			(MonsterState)(object)val7,
			(MonsterState)(object)val6,
			(MonsterState)(object)val3,
			(MonsterState)(object)val4,
			(MonsterState)(object)val5
		}, (MonsterState)(object)val);
		bool AmalgamDied()
		{
			return HasAmalgamDiedRef.Invoke(queen);
		}
		bool FreezeTurn()
		{
			return CombatManager.Instance.History.Entries.OfType<MonsterPerformedMoveEntry>().Count((MonsterPerformedMoveEntry e) => (object)e.Monster == queen && (e.Move == burnBright || e.Move == frozenInPlace)) % 3 == 0;
		}
	}

	private static Func<IReadOnlyList<Creature>, Task> OnParty(Queen queen, string methodName)
	{
		Func<IReadOnlyList<Creature>, Task> vanilla = MonsterRework.VanillaMove((MonsterModel)(object)queen, methodName);
		return (IReadOnlyList<Creature> _) => vanilla(LivingPlayers(queen));
	}

	private static List<Creature> LivingPlayers(Queen queen)
	{
		return ((MonsterModel)queen).CombatState.PlayerCreatures.Where((Creature c) => c.IsAlive).ToList();
	}

	private static async Task FrozenInPlaceMove(Queen queen)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/queen/queen_cast", 1f);
		await CreatureCmd.TriggerAnim(((MonsterModel)queen).Creature, "Cast", 0.5f);
		await FreezeParty(queen);
	}

	private static async Task FreezeParty(Queen queen)
	{
		try
		{
			List<Creature> list = LivingPlayers(queen);
			if (list.Count != 0)
			{
				await PowerCmd.Apply<FrozenInPlacePower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)list, 1m, ((MonsterModel)queen).Creature, (CardModel)null, false);
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[QueenMp] applying Frozen in Place failed: " + ex.Message, 1);
		}
	}
}
