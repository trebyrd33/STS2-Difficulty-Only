using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class EnrageContributionPower : MultiplayerExpandedPower
{
	private static readonly AccessTools.FieldRef<PowerModel, string?> ResolvedBigIconPathRef = AccessTools.FieldRefAccess<PowerModel, string>("_resolvedBigIconPath");

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override PowerInstanceType InstanceType => (PowerInstanceType)2;

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new StringVar("Teammate", "This player") };
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[2]
	{
		HoverTipFactory.FromPower<StrengthPower>((int?)null),
		HoverTipFactory.FromPower<EnragePower>((int?)null)
	};

	public override LocString Title
	{
		get
		{
			Creature applier = this.Applier;
			if (((applier != null) ? applier.Player : null) == null)
			{
				return base.Title;
			}
			LocString val = new LocString("powers", this.Id.Entry + ".titleWithPlayer");
			val.Add("Player", applier.Name);
			return val;
		}
	}

	private string IconFileName
	{
		get
		{
			Creature applier = this.Applier;
			object obj;
			if (applier == null)
			{
				obj = null;
			}
			else
			{
				Player player = applier.Player;
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
		if (applier != null)
		{
			SetTeammate(applier);
		}
		return Task.CompletedTask;
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		ResolvedBigIconPathRef.Invoke((PowerModel)(object)this) = null;
	}
}
