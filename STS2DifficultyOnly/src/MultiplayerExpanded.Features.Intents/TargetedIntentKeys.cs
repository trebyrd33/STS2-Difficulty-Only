using System;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Helpers;

namespace MultiplayerExpanded.Features.Intents;

internal static class TargetedIntentKeys
{
	public static string Prefix(Type intentType)
	{
		return TypePrefix.GetPrefix(intentType) + StringHelper.Slugify(intentType.Name);
	}
}
