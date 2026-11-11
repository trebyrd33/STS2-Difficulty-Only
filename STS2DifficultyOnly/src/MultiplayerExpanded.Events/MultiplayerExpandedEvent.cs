using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Events;

public abstract class MultiplayerExpandedEvent : CustomEventModel
{
	protected string Slug => BaseLib.Extensions.StringExtensions.RemovePrefix(((AbstractModel)this).Id.Entry).ToLowerInvariant();

	public override string? CustomInitialPortraitPath => (Slug + ".png").EventImagePath();

	protected bool RunsPartyWideStep
	{
		get
		{
			if (((EventModel)this).Owner == null)
			{
				return false;
			}
			Player me = LocalContext.GetMe((IPlayerCollection)(object)((EventModel)this).Owner.RunState);
			Creature val = ((me != null) ? me.Creature : null);
			if (val == null || !val.IsDead)
			{
				return LocalContext.IsMe(((EventModel)this).Owner);
			}
			return ((EventModel)this).Owner == LivingPlayers(((EventModel)this).Owner.RunState).FirstOrDefault();
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

	protected static List<Player> LivingPlayers(IRunState runState)
	{
		return ((IPlayerCollection)runState).Players.Where((Player p) =>
		{
			Creature creature = p.Creature;
			return creature != null && creature.IsAlive;
		}).ToList();
	}

	protected MultiplayerExpandedEvent()
		: base(true)
	{
	}
}
