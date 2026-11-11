using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace MultiplayerExpanded.Features.Intents;

public static class AttackOriginalTargets
{
	internal sealed class Frame
	{
		public required Creature Dealer;

		public Creature[]? Targets;
	}

	internal readonly record struct PreviewScope(Creature? Dealer, IReadOnlyCollection<Creature>? Targets);

	private static readonly ConditionalWeakTable<AttackCommand, Creature[]> Declared = new ConditionalWeakTable<AttackCommand, Creature[]>();

	private static readonly Dictionary<Creature, List<Frame>> Running = new Dictionary<Creature, List<Frame>>();

	[ThreadStatic]
	private static Creature? _previewDealer;

	[ThreadStatic]
	private static IReadOnlyCollection<Creature>? _previewTargets;

	public static void Declare(AttackCommand attack, IEnumerable<Creature> targets)
	{
		Declared.AddOrUpdate(attack, targets.ToArray());
	}

	internal static bool IsDeclared(AttackCommand attack)
	{
		Creature[] value;
		return Declared.TryGetValue(attack, out value);
	}

	public static IReadOnlyCollection<Creature>? Of(Creature? dealer)
	{
		if (dealer == null)
		{
			return null;
		}
		if (_previewDealer == dealer)
		{
			return _previewTargets;
		}
		if (!Running.TryGetValue(dealer, out List<Frame> value) || value.Count <= 0)
		{
			return null;
		}
		List<Frame> list = value;
		return list[list.Count - 1].Targets;
	}

	public static bool WasTargeted(Creature? dealer, Creature creature)
	{
		return Of(dealer)?.Contains(creature) ?? true;
	}

	internal static decimal Preview(Creature dealer, IReadOnlyCollection<Creature> targets, Func<decimal> modifyDamage)
	{
		PreviewScope previous = BeginPreview(dealer, targets);
		try
		{
			return modifyDamage();
		}
		finally
		{
			EndPreview(previous);
		}
	}

	internal static PreviewScope BeginPreview(Creature dealer, IReadOnlyCollection<Creature> targets)
	{
		PreviewScope result = new PreviewScope(_previewDealer, _previewTargets);
		_previewDealer = dealer;
		_previewTargets = targets;
		return result;
	}

	internal static void EndPreview(PreviewScope previous)
	{
		Creature? dealer = previous.Dealer;
		IReadOnlyCollection<Creature> targets = previous.Targets;
		_previewDealer = dealer;
		_previewTargets = targets;
	}

	internal static Frame? Begin(AttackCommand attack)
	{
		Creature attacker = attack.Attacker;
		if (attacker == null)
		{
			return null;
		}
		Frame frame = new Frame
		{
			Dealer = attacker,
			Targets = (Declared.TryGetValue(attack, out Creature[] value) ? value : null)
		};
		if (!Running.TryGetValue(attacker, out List<Frame> value2))
		{
			value2 = (Running[attacker] = new List<Frame>());
		}
		value2.Add(frame);
		return frame;
	}

	internal static void End(Frame frame)
	{
		if (Running.TryGetValue(frame.Dealer, out List<Frame> value))
		{
			value.Remove(frame);
			if (value.Count == 0)
			{
				Running.Remove(frame.Dealer);
			}
		}
	}

	internal static void Reset()
	{
		Running.Clear();
	}
}
