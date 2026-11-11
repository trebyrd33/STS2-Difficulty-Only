using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.Intents;

namespace MultiplayerExpanded.Features.MonsterReworks;

public static class MonsterTargeting
{
	private static readonly Func<CombatHistoryEntry, int> RoundOf = AccessTools.MethodDelegate<Func<CombatHistoryEntry, int>>(AccessTools.PropertyGetter(typeof(CombatHistoryEntry), "RoundNumber"), (object)null, true, (Type[])null);

	private static readonly Func<CombatHistoryEntry, CombatSide> SideOf = AccessTools.MethodDelegate<Func<CombatHistoryEntry, CombatSide>>(AccessTools.PropertyGetter(typeof(CombatHistoryEntry), "CurrentSide"), (object)null, true, (Type[])null);

	private static readonly AccessTools.FieldRef<AttackCommand, string?> AttackerAnimRef = AccessTools.FieldRefAccess<AttackCommand, string>("_attackerAnimName");

	private static readonly Action<AttackCommand, Creature?> SetAttacker = AccessTools.MethodDelegate<Action<AttackCommand, Creature>>(AccessTools.PropertySetter(typeof(AttackCommand), "Attacker"), (object)null, true, (Type[])null);

	public static Player? PlayerOf(Creature? dealer)
	{
		object obj = ((dealer != null) ? dealer.Player : null);
		if (obj == null)
		{
			if (dealer == null)
			{
				return null;
			}
			obj = dealer.PetOwner;
		}
		return (Player?)obj;
	}

	public static IEnumerable<DamageReceivedEntry> LatestPlayerTurnDamage(ICombatState combatState)
	{
		return PlayerTurnDamage(combatState, combatState.RoundNumber);
	}

	public static IEnumerable<DamageReceivedEntry> PlayerTurnDamage(ICombatState combatState, int round)
	{
		return CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().Where((DamageReceivedEntry e) =>
		{
			return (int)SideOf((CombatHistoryEntry)(object)e) == 1 && RoundOf((CombatHistoryEntry)(object)e) == round;
		});
	}

	public static Player? MostDamageDealer(ICombatState combatState, IEnumerable<DamageReceivedEntry> entries, Creature? receiver = null)
	{
		Dictionary<Player, int> dictionary = DamageTotals(entries, receiver);
		Player result = null;
		int num = 0;
		foreach (Player player in combatState.Players)
		{
			if (player.Creature.IsAlive && dictionary.TryGetValue(player, out var value) && value > num)
			{
				num = value;
				result = player;
			}
		}
		return result;
	}

	public static IReadOnlyList<Player> MostDamageDealers(ICombatState combatState, IEnumerable<DamageReceivedEntry> entries, Creature? receiver = null)
	{
		Dictionary<Player, int> totals = DamageTotals(entries, receiver);
		return TiedForMost(combatState, (Player p) => totals.GetValueOrDefault(p));
	}

	public static IReadOnlyList<Player> HighestBlockPlayers(ICombatState combatState)
	{
		return TiedForMost(combatState, (Player p) => p.Creature.Block);
	}

	public static Player? BreakTie(IReadOnlyList<Player> tied, Creature monster)
	{
		if (tied.Count > 1)
		{
			ICombatState combatState = monster.CombatState;
			if (combatState != null)
			{
				IRunState runState = combatState.RunState;
				string name = $"mpx_tiebreak_{runState.TotalFloor}_{combatState.RoundNumber}_{monster.CombatId.GetValueOrDefault()}";
				return GameApi.NewRng(GameApi.Seed(runState.Rng), name).NextItem<Player>((IEnumerable<Player>)tied);
			}
		}
		return tied.FirstOrDefault();
	}

	private static Dictionary<Player, int> DamageTotals(IEnumerable<DamageReceivedEntry> entries, Creature? receiver)
	{
		Dictionary<Player, int> dictionary = new Dictionary<Player, int>();
		foreach (DamageReceivedEntry entry in entries)
		{
			if (!((receiver != null) ? (entry.Receiver != receiver) : ((int)entry.Receiver.Side != 2)))
			{
				Player val = PlayerOf(entry.Dealer);
				if (val != null)
				{
					dictionary[val] = dictionary.GetValueOrDefault(val) + entry.Result.TotalDamage;
				}
			}
		}
		return dictionary;
	}

	private static List<Player> TiedForMost(ICombatState combatState, Func<Player, int> score)
	{
		List<Player> list = combatState.Players.Where((Player p) => p.Creature.IsAlive).ToList();
		if (list.Count == 0)
		{
			return list;
		}
		int best = list.Max(score);
		return list.Where((Player p) => score(p) == best).ToList();
	}

	public static Player? MostRecentDamageDealer(ICombatState combatState, Creature? receiver = null)
	{
		foreach (DamageReceivedEntry item in CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().Reverse())
		{
			if (!((receiver != null) ? (item.Receiver != receiver) : ((int)item.Receiver.Side != 2)))
			{
				Player val = PlayerOf(item.Dealer);
				if (val != null && val.Creature.IsAlive && combatState.Players.Contains(val))
				{
					return val;
				}
			}
		}
		return null;
	}

	public static Player? HighestBlockPlayer(ICombatState combatState)
	{
		Player val = null;
		foreach (Player player in combatState.Players)
		{
			if (player.Creature.IsAlive && (val == null || player.Creature.Block > val.Creature.Block))
			{
				val = player;
			}
		}
		return val;
	}

	public static Player? FirstLivingPlayer(ICombatState combatState)
	{
		return combatState.Players.FirstOrDefault((Player p) => p.Creature.IsAlive);
	}

	public static AttackCommand FromMonsterTargeting(this AttackCommand attack, MonsterModel monster, Creature target)
	{
		if (attack.Attacker != null)
		{
			throw new InvalidOperationException("Attacker has already been set.");
		}
		SetAttacker(attack, monster.Creature);
		AttackerAnimRef.Invoke(attack) = "Attack";
		AttackOriginalTargets.Declare(attack, new Creature[1] { target });
		return attack.Targeting(target);
	}
}
