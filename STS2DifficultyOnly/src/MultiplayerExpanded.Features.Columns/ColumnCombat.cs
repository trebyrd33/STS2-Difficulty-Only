using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace MultiplayerExpanded.Features.Columns;

public static class ColumnCombat
{
	private static readonly ConditionalWeakTable<AbstractIntent, object> PartyWideIntents = new ConditionalWeakTable<AbstractIntent, object>();

	public static IColumnEncounter? Of(ICombatState? state)
	{
		if (((state != null) ? state.Encounter : null) is IColumnEncounter columnEncounter)
		{
			IReadOnlyList<ColumnSide> columns = columnEncounter.Columns;
			if (columns != null && columns.Count > 0)
			{
				return columnEncounter;
			}
		}
		return null;
	}

	public static int Capacity(ICombatState state, IColumnEncounter encounter)
	{
		return (state.Players.Count + encounter.Columns.Count - 1) / encounter.Columns.Count;
	}

	public static ColumnSide? ColumnOf(Creature player)
	{
		return player.GetPower<ColumnPower>()?.Column;
	}

	public static IReadOnlyList<Creature>? TargetsOf(Creature enemy)
	{
		MonsterModel monster = enemy.Monster;
		if (monster != null)
		{
			IColumnEncounter columnEncounter = Of(enemy.CombatState);
			if (columnEncounter != null)
			{
				ColumnSide? columnSide = columnEncounter.ColumnOf(monster);
				if (columnSide.HasValue)
				{
					ColumnSide column = columnSide.GetValueOrDefault();
					List<Creature> list = (from p in enemy.CombatState.Players
						select p.Creature into c
						where c.IsAlive
						select c).ToList();
					List<Creature> list2 = list.Where((Creature c) => !ColumnOf(c).HasValue || ColumnOf(c) == column).ToList();
					if (list2.Count > 0)
					{
						return list2;
					}
					if (list.Count <= 0)
					{
						return null;
					}
					return list;
				}
				return null;
			}
		}
		return null;
	}

	public static T PartyWide<T>(T intent) where T : AbstractIntent
	{
		PartyWideIntents.AddOrUpdate((AbstractIntent)(object)intent, PartyWideIntents);
		return intent;
	}

	internal static bool IsPartyWide(AbstractIntent intent)
	{
		object value;
		return PartyWideIntents.TryGetValue(intent, out value);
	}

	internal static async Task SetUp(ICombatState state, IColumnEncounter encounter)
	{
		_ = 1;
		try
		{
			int capacity = Capacity(state, encounter);
			int lastColumn = encounter.Columns.Count - 1;
			for (int i = 0; i < state.Players.Count; i++)
			{
				ColumnPower columnPower = (ColumnPower)(object)((PowerModel)ModelDb.Power<ColumnPower>()).ToMutable(0);
				columnPower.Column = encounter.Columns[Math.Min(i / capacity, lastColumn)];
				columnPower.Stamp = i;
				await PowerCmd.Apply((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (PowerModel)(object)columnPower, state.Players[i].Creature, 1m, (Creature)null, (CardModel)null, false);
			}
			foreach (Creature item in state.Enemies.ToList())
			{
				await MarkEnemy(item, encounter);
			}
		}
		catch (Exception value)
		{
			Logger logger = MainFile.Logger;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
			defaultInterpolatedStringHandler.AppendLiteral("[Columns] setting up columns for ");
			EncounterModel encounter2 = state.Encounter;
			defaultInterpolatedStringHandler.AppendFormatted((encounter2 != null) ? ((AbstractModel)encounter2).Id.Entry : null);
			defaultInterpolatedStringHandler.AppendLiteral(" failed: ");
			defaultInterpolatedStringHandler.AppendFormatted(value);
			logger.Error(defaultInterpolatedStringHandler.ToStringAndClear(), 1);
		}
		ColumnView.Refresh(state);
	}

	internal static async Task MarkEnemy(Creature enemy, IColumnEncounter encounter)
	{
		MonsterModel monster = enemy.Monster;
		if (monster == null)
		{
			return;
		}
		ColumnSide? columnSide = encounter.ColumnOf(monster);
		if (!columnSide.HasValue)
		{
			return;
		}
		ColumnSide column = columnSide.GetValueOrDefault();
		if (!enemy.HasPower<ColumnEnemyPower>())
		{
			if (column != ColumnSide.Left)
			{
				await PowerCmd.Apply<RightColumnEnemyPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), enemy, 1m, enemy, (CardModel)null, false);
			}
			else
			{
				await PowerCmd.Apply<LeftColumnEnemyPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), enemy, 1m, enemy, (CardModel)null, false);
			}
			ColumnView.FaceToward(enemy, (column != ColumnSide.Left) ? ColumnSide.Left : ColumnSide.Right);
		}
	}

	internal static void Join(ColumnPower mover, Creature target)
	{
		if (target.Side == ((PowerModel)mover).Owner.Side)
		{
			return;
		}
		MonsterModel monster = target.Monster;
		if (monster == null)
		{
			return;
		}
		ICombatState combatState = ((PowerModel)mover).Owner.CombatState;
		IColumnEncounter columnEncounter = Of(combatState);
		if (columnEncounter == null)
		{
			return;
		}
		ColumnSide? columnSide = columnEncounter.ColumnOf(monster);
		if (!columnSide.HasValue)
		{
			return;
		}
		ColumnSide column = columnSide.GetValueOrDefault();
		List<ColumnPower> source = combatState.Players.Select((Player p) => p.Creature.GetPower<ColumnPower>()).OfType<ColumnPower>().ToList();
		int stamp = source.Max((ColumnPower p) => p.Stamp) + 1;
		bool flag = mover.Column != column;
		if (flag)
		{
			List<ColumnPower> list = source.Where((ColumnPower p) => p.Column == column).ToList();
			if (list.Count >= Capacity(combatState, columnEncounter))
			{
				(from p in list
					orderby ((PowerModel)p).Owner.IsAlive, p.Stamp
					select p).First().Column = mover.Column;
			}
			mover.Column = column;
		}
		mover.Stamp = stamp;
		if (flag)
		{
			ColumnView.Refresh(combatState);
		}
	}
}
