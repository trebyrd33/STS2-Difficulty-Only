using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Features.Columns;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class FrozenInPlacePower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(this.Owner))
		{
			this.Flash();
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}

	internal static bool InOtherColumn(Creature player, Creature enemy)
	{
		MonsterModel monster = enemy.Monster;
		if (monster != null)
		{
			IColumnEncounter columnEncounter = ColumnCombat.Of(player.CombatState);
			if (columnEncounter != null)
			{
				ColumnSide? columnSide = columnEncounter.ColumnOf(monster);
				if (columnSide.HasValue)
				{
					ColumnSide valueOrDefault = columnSide.GetValueOrDefault();
					columnSide = ColumnCombat.ColumnOf(player);
					if (columnSide.HasValue)
					{
						ColumnSide valueOrDefault2 = columnSide.GetValueOrDefault();
						return valueOrDefault != valueOrDefault2;
					}
				}
			}
		}
		return false;
	}
}
