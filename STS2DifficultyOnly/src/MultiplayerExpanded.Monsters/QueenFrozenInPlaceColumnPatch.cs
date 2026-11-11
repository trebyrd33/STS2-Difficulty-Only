using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Features.Columns;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(ColumnCombat), "Join")]
internal static class QueenFrozenInPlaceColumnPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(ColumnPower mover, Creature target)
	{
		if (!FrozenInPlacePower.InOtherColumn(((PowerModel)mover).Owner, target))
		{
			return true;
		}
		if (((PowerModel)mover).Owner.HasPower<FrozenInPlacePower>())
		{
			return false;
		}
		ICombatState combatState = ((PowerModel)mover).Owner.CombatState;
		IColumnEncounter columnEncounter = ColumnCombat.Of(combatState);
		ColumnSide? column = columnEncounter.ColumnOf(target.Monster);
		List<ColumnPower> list = (from p in combatState.Players.Select((Player p) => p.Creature.GetPower<ColumnPower>()).OfType<ColumnPower>()
			where p.Column == column
			select p).ToList();
		if (list.Count < ColumnCombat.Capacity(combatState, columnEncounter))
		{
			return true;
		}
		return !((PowerModel)(from p in list
			orderby ((PowerModel)p).Owner.IsAlive, p.Stamp
			select p).First()).Owner.HasPower<FrozenInPlacePower>();
	}
}
