using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace MultiplayerExpanded.Features.Columns;

public interface IColumnEncounter
{
	IReadOnlyList<ColumnSide> Columns { get; }

	ColumnSide? ColumnOf(MonsterModel monster);
}
