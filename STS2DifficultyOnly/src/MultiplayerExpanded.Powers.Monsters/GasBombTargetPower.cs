using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class GasBombTargetPower : MultiplayerExpandedPower
{
	private Creature? _target;

	private static readonly AccessTools.FieldRef<PowerModel, string?> ResolvedBigIconPathRef = AccessTools.FieldRefAccess<PowerModel, string>("_resolvedBigIconPath");

	public override PowerType Type => (PowerType)0;

	public override PowerStackType StackType => (PowerStackType)2;

	public Creature? BombTarget
	{
		get
		{
			return _target;
		}
		set
		{
			this.AssertMutable();
			_target = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new StringVar("Teammate", "One player") };
		}
	}

	public override LocString Title
	{
		get
		{
			if (this.IsMutable)
			{
				Creature? bombTarget = BombTarget;
				if (((bombTarget != null) ? bombTarget.Player : null) != null)
				{
					LocString val = new LocString("powers", this.Id.Entry + ".titleWithPlayer");
					val.Add("Player", BombTarget.Name);
					return val;
				}
			}
			return base.Title;
		}
	}

	private string IconFileName
	{
		get
		{
			object obj;
			if (!this.IsMutable)
			{
				obj = null;
			}
			else
			{
				Creature? bombTarget = BombTarget;
				if (bombTarget == null)
				{
					obj = null;
				}
				else
				{
					Player player = bombTarget.Player;
					if (player == null)
					{
						obj = null;
					}
					else
					{
						CharacterModel character = player.Character;
						obj = ((character != null) ? ((AbstractModel)character).Id.Entry : null);
					}
				}
			}
			string text = (string)obj;
			if (string.IsNullOrEmpty(text))
			{
				return "power.png";
			}
			return BaseLib.Extensions.StringExtensions.RemovePrefix(text).ToLowerInvariant() + ".png";
		}
	}

	public override string CustomPackedIconPath => IconFileName.PowerImagePath();

	public override string CustomBigIconPath => IconFileName.BigPowerImagePath();

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (BombTarget != null)
		{
			SetTeammate(BombTarget);
		}
		return Task.CompletedTask;
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		ResolvedBigIconPathRef.Invoke((PowerModel)(object)this) = null;
	}
}
