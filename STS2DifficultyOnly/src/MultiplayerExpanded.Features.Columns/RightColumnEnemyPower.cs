using System;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Features.Columns;

public sealed class RightColumnEnemyPower : ColumnEnemyPower
{
	protected override Type IconPower => typeof(BackAttackRightPower);
}
