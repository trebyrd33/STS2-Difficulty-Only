using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Features.MonsterReworks;

public static class MonsterReworkRegistry
{
	private static Dictionary<Type, MonsterRework> _byBase = new Dictionary<Type, MonsterRework>();

	public static int Count => _byBase.Count;

	public static IEnumerable<Type> BaseTypes => _byBase.Keys;

	public static void Build()
	{
		Dictionary<Type, MonsterRework> dictionary = new Dictionary<Type, MonsterRework>();
		Type[] types = typeof(MonsterRework).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsAbstract && typeof(MonsterRework).IsAssignableFrom(type) && BranchFilter.Allows(type))
			{
				MonsterRework monsterRework = (MonsterRework)Activator.CreateInstance(type);
				if (dictionary.TryGetValue(monsterRework.BaseMonster, out var value))
				{
					throw new InvalidOperationException($"{type.Name} and {value.GetType().Name} both rework {monsterRework.BaseMonster.Name}");
				}
				dictionary[monsterRework.BaseMonster] = monsterRework;
			}
		}
		_byBase = dictionary;
	}

	public static bool TryGet(Type monsterType, [MaybeNullWhen(false)] out MonsterRework rework)
	{
		return _byBase.TryGetValue(monsterType, out rework);
	}
}
