using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Powers;

public abstract class MultiplayerExpandedPower : CustomPowerModel
{
	public override string CustomPackedIconPath => (BaseLib.Extensions.StringExtensions.RemovePrefix(this.Id.Entry).ToLowerInvariant() + ".png").PowerImagePath();

	public override string CustomBigIconPath => (BaseLib.Extensions.StringExtensions.RemovePrefix(this.Id.Entry).ToLowerInvariant() + ".png").BigPowerImagePath();

	public virtual bool CanBeCleansed => true;

	public void SetTeammate(Creature ally)
	{
		((StringVar)this.DynamicVars["Teammate"]).StringValue = ally.Name;
	}
}
