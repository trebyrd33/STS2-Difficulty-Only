using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Extensions;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Relics;

public abstract class MultiplayerExpandedRelic : CustomRelicModel
{
	public override string PackedIconPath => (BaseLib.Extensions.StringExtensions.RemovePrefix(((AbstractModel)this).Id.Entry).ToLowerInvariant() + ".png").RelicImagePath();

	protected override string PackedIconOutlinePath => (BaseLib.Extensions.StringExtensions.RemovePrefix(((AbstractModel)this).Id.Entry).ToLowerInvariant() + "_outline.png").RelicImagePath();

	protected override string BigIconPath => (BaseLib.Extensions.StringExtensions.RemovePrefix(((AbstractModel)this).Id.Entry).ToLowerInvariant() + ".png").BigRelicImagePath();

	protected MultiplayerExpandedRelic()
		: base(false)
	{
		if (BranchFilter.Allows(((object)this).GetType()))
		{
			CustomContentDictionary.AddModel(((object)this).GetType());
		}
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (MultiplayerExpandedConfig.MultiplayerEvents && runState != null)
		{
			IReadOnlyList<Player> players = ((IPlayerCollection)runState).Players;
			if (players != null)
			{
				return players.Count > 1;
			}
		}
		return false;
	}
}
