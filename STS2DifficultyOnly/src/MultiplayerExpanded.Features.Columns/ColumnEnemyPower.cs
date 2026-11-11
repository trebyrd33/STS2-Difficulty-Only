using System;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Powers;

namespace MultiplayerExpanded.Features.Columns;

public abstract class ColumnEnemyPower : MultiplayerExpandedPower
{
	protected abstract Type IconPower { get; }

	private string IconEntry => ModelDb.GetEntry(IconPower).ToLowerInvariant();

	public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + IconEntry + ".tres");

	public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/" + IconEntry + ".png");

	public override PowerType Type => (PowerType)0;

	public override bool CanBeCleansed => false;

	public override PowerStackType StackType => (PowerStackType)2;

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}
}
