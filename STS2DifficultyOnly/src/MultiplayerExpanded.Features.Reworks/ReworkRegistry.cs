using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MultiplayerExpanded.Cards;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.Reworks;

public static class ReworkRegistry
{
	private static Dictionary<Type, ReworkInfo> _byBase = new Dictionary<Type, ReworkInfo>();

	public static IEnumerable<ReworkInfo> All => _byBase.Values;

	public static int Count => _byBase.Count;

	public static bool Built { get; private set; }

	public static IEnumerable<Type> BaseTypes => _byBase.Keys;

	public static void Build()
	{
		Dictionary<Type, ReworkInfo> dictionary = new Dictionary<Type, ReworkInfo>();
		Type[] types = typeof(CardRework).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsAbstract && typeof(CardRework).IsAssignableFrom(type) && BranchFilter.Allows(type))
			{
				CardRework cardRework = (CardRework)Activator.CreateInstance(type);
				if (dictionary.TryGetValue(cardRework.BaseCard, out var value))
				{
					throw new InvalidOperationException($"{type.Name} and {value.Rework.GetType().Name} both rework {cardRework.BaseCard.Name}");
				}
				dictionary[cardRework.BaseCard] = new ReworkInfo(cardRework.BaseCard, cardRework, cardRework.LocKey);
			}
		}
		_byBase = dictionary;
		Built = true;
	}

	public static bool TryGet(Type baseType, [MaybeNullWhen(false)] out ReworkInfo info)
	{
		return _byBase.TryGetValue(baseType, out info);
	}
}
