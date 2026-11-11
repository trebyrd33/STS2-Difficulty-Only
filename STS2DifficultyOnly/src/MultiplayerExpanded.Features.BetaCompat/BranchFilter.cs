using System;
using System.Collections.Generic;

namespace MultiplayerExpanded.Features.BetaCompat;

public static class BranchFilter
{
	public static readonly SortedSet<string> Filtered = new SortedSet<string>(StringComparer.Ordinal);

	public static bool Allows(Type type)
	{
		bool flag = (Beta.IsOn ? (!type.IsDefined(typeof(StableOnlyAttribute), inherit: false)) : (!type.IsDefined(typeof(BetaOnlyAttribute), inherit: false)));
		if (!flag)
		{
			Filtered.Add(type.FullName);
		}
		return flag;
	}
}
