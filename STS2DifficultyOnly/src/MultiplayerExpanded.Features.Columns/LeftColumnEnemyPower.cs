using System;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Features.Columns;

public sealed class LeftColumnEnemyPower : ColumnEnemyPower
{
	protected override Type IconPower => typeof(BackAttackLeftPower);
}
