using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Powers;

namespace MultiplayerExpanded.Features.Columns;

public sealed class ColumnPower : MultiplayerExpandedPower
{
	private static readonly string _iconEntry = ModelDb.GetEntry(typeof(SurroundedPower)).ToLowerInvariant();

	private ColumnSide _column;

	private int _stamp;

	public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + _iconEntry + ".tres");

	public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/" + _iconEntry + ".png");

	public override PowerType Type => (PowerType)0;

	public override bool CanBeCleansed => false;

	public override PowerStackType StackType => (PowerStackType)2;

	public ColumnSide Column
	{
		get
		{
			return _column;
		}
		set
		{
			this.AssertMutable();
			_column = value;
		}
	}

	public int Stamp
	{
		get
		{
			return _stamp;
		}
		set
		{
			this.AssertMutable();
			_stamp = value;
		}
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Target != null && cardPlay.Card.Owner == this.Owner.Player)
		{
			ColumnCombat.Join(this, cardPlay.Target);
		}
		return Task.CompletedTask;
	}

	public override Task BeforePotionUsed(PotionModel potion, Creature? target)
	{
		if (CombatManager.Instance.IsInProgress && target != null && potion.Owner == this.Owner.Player)
		{
			ColumnCombat.Join(this, target);
		}
		return Task.CompletedTask;
	}

	public override Task AfterCreatureAddedToCombat(Creature creature)
	{
		if (creature.IsEnemy)
		{
			IColumnEncounter columnEncounter = ColumnCombat.Of(this.Owner.CombatState);
			if (columnEncounter != null)
			{
				return ColumnCombat.MarkEnemy(creature, columnEncounter);
			}
		}
		return Task.CompletedTask;
	}
}
