using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.Difficulty;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Features.Intents;

public sealed class IntentTarget
{
	private readonly Func<Creature, ICombatState, Player?> _pick;

	public bool ExcludesPlayer { get; }

	public bool Redirects { get; }

	private IntentTarget(Func<Creature, ICombatState, Player?> pick, bool excludesPlayer, bool redirects)
	{
		_pick = pick;
		ExcludesPlayer = excludesPlayer;
		Redirects = redirects;
	}

	public static IntentTarget MostBlock()
	{
		return new IntentTarget((Creature owner, ICombatState state) => MonsterTargeting.BreakTie(MonsterTargeting.HighestBlockPlayers(state), owner), excludesPlayer: false, redirects: false);
	}

	public static IntentTarget MostDamageThisTurn(bool toOwnerOnly = false)
	{
		return new IntentTarget((Creature owner, ICombatState state) => MonsterTargeting.BreakTie(MonsterTargeting.MostDamageDealers(state, MonsterTargeting.LatestPlayerTurnDamage(state), toOwnerOnly ? owner : null), owner), excludesPlayer: false, redirects: false);
	}

	public static IntentTarget Chosen(Func<Creature, Player?> pick)
	{
		return new IntentTarget((Creature owner, ICombatState _) => pick(owner), excludesPlayer: false, redirects: false);
	}

	public IntentTarget EveryoneElse()
	{
		return new IntentTarget(_pick, excludesPlayer: true, Redirects);
	}

	internal IntentTarget Attacking()
	{
		return new IntentTarget(_pick, ExcludesPlayer, redirects: true);
	}

	public Player? Resolve(Creature owner)
	{
		ICombatState combatState = owner.CombatState;
		if (combatState == null)
		{
			return null;
		}
		Player val = _pick(owner, combatState);
		if (val == null || !val.Creature.IsAlive || !combatState.Players.Contains(val))
		{
			return MonsterTargeting.FirstLivingPlayer(combatState);
		}
		return val;
	}

	public IntentAim Aim(Creature owner)
	{
		return new IntentAim(owner.CombatState, Resolve(owner), ExcludesPlayer, Redirects);
	}

	internal void AddDescriptionVars(LocString description, Creature owner)
	{
		IntentAim intentAim = Aim(owner);
		description.Add("HasTarget", intentAim.SingledOut != null);
		Player? singledOut = intentAim.SingledOut;
		description.Add("Player", ((singledOut != null) ? singledOut.Creature.Name : null) ?? string.Empty);
		IReadOnlyList<Creature> readOnlyList;
		if (intentAim.RedirectedTo == null || intentAim.Redirect != IntentRedirect.Intercept)
		{
			readOnlyList = intentAim.InterceptedFor;
		}
		else
		{
			IReadOnlyList<Creature> readOnlyList2 = new Creature[1] { intentAim.RedirectedTo };
			readOnlyList = readOnlyList2;
		}
		IReadOnlyList<Creature> readOnlyList3 = readOnlyList;
		IReadOnlyList<Creature> readOnlyList4;
		if (intentAim.RedirectedTo == null || intentAim.Redirect != IntentRedirect.Shirk)
		{
			readOnlyList4 = intentAim.ShirkedFor;
		}
		else
		{
			IReadOnlyList<Creature> readOnlyList2 = new Creature[1] { intentAim.RedirectedTo };
			readOnlyList4 = readOnlyList2;
		}
		IReadOnlyList<Creature> readOnlyList5 = readOnlyList4;
		description.Add("IsIntercepted", readOnlyList3.Count > 0);
		description.Add("InterceptNames", string.Join(", ", readOnlyList3.Select((Creature c) => c.Name)));
		description.Add("IsShirking", readOnlyList5.Count > 0);
		description.Add("ShirkNames", string.Join(", ", readOnlyList5.Select((Creature c) => c.Name)));
	}

	internal int PreviewDamage(Creature owner, decimal damage)
	{
		damage = ChallengeOptions.CompensatedBaseDamage(damage, owner.CombatState);
		IntentAim intentAim = Aim(owner);
		IReadOnlyList<Creature> hitCreatures = intentAim.HitCreatures;
		Player me = LocalContext.GetMe(owner.CombatState);
		Creature val = ((me != null) ? me.Creature : null);
		Creature recipient = (Creature)((val != null && hitCreatures.Contains(val)) ? ((object)val) : ((object)(hitCreatures.FirstOrDefault() ?? val)));
		Creature val2 = recipient;
		Player player = ((val2 != null) ? val2.Player : null);
		if (player != null)
		{
			decimal baseDamage = damage;
			damage = AttackOriginalTargets.Preview(owner, intentAim.Originals, () => GameApi.ModifyDamage(player.RunState, recipient.CombatState, recipient, owner, baseDamage, (ValueProp)8, null, null, (ModifyDamageHookType)14, (CardPreviewMode)0, out IEnumerable<AbstractModel> _));
		}
		return Math.Max(0, (int)damage);
	}
}
