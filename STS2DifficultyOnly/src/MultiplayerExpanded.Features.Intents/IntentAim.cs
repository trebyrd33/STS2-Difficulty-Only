using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Patches;


namespace MultiplayerExpanded.Features.Intents;

public sealed class IntentAim
{
	public Player? SingledOut { get; }

	public bool ExcludesPlayer { get; }

	public IReadOnlyList<Creature> Originals { get; }

	public IReadOnlyList<Creature> HitCreatures { get; }

	public Creature? RedirectedTo { get; }

	public IntentRedirect Redirect { get; }

	public IReadOnlyList<Creature> InterceptedFor { get; } = Array.Empty<Creature>();

	public IReadOnlyList<Creature> ShirkedFor { get; } = Array.Empty<Creature>();

	public Player? Shown
	{
		get
		{
			Creature? redirectedTo = RedirectedTo;
			return ((redirectedTo != null) ? redirectedTo.Player : null) ?? SingledOut;
		}
	}

	public IReadOnlyList<Creature> Creatures => Originals.Where((Creature c) => c.IsAlive).ToArray();

	internal IntentAim(ICombatState? state, Player? singledOut, bool excludesPlayer, bool redirects)
	{
		SingledOut = singledOut;
		ExcludesPlayer = excludesPlayer;
		if (state == null || singledOut == null)
		{
			Originals = (HitCreatures = Array.Empty<Creature>());
			return;
		}
		Creature singled = singledOut.Creature;
		Originals = (excludesPlayer ? state.PlayerCreatures.Where((Creature c) => c.IsAlive && c != singled).ToArray() : new Creature[1] { singled });
		HitCreatures = Originals;
		if (!redirects)
		{
			return;
		}
		if (!excludesPlayer)
		{
			Creature val = InterceptMpRedirectDamagePatch.CovererOf(singled);
			if (val != null)
			{
				Creature val2 = val;
				RedirectedTo = val2;
				Redirect = IntentRedirect.Intercept;
			}
			if (RedirectedTo != null)
			{
				HitCreatures = new Creature[1] { RedirectedTo };
			}
		}
		else
		{
			InterceptedFor = Originals.Where((Creature c) => InterceptMpRedirectDamagePatch.CovererOf(c) == singled).ToArray();
			if (InterceptedFor.Count + ShirkedFor.Count > 0)
			{
				HitCreatures = Originals.Append(singled).ToArray();
			}
		}
	}

	public AttackCommand Attack(MonsterModel monster, decimal damagePerHit)
	{
		AttackCommand val = DamageCmd.Attack(damagePerHit);
		if (SingledOut == null)
		{
			return val.FromMonster(monster);
		}
		if (!ExcludesPlayer)
		{
			val.FromMonsterTargeting(monster, RedirectedTo ?? SingledOut.Creature);
		}
		else if (HitCreatures.Contains(SingledOut.Creature))
		{
			val.FromMonster(monster);
		}
		else
		{
			val.FromMonsterExcept(monster, SingledOut.Creature);
		}
		AttackOriginalTargets.Declare(val, Originals);
		return val;
	}
}
