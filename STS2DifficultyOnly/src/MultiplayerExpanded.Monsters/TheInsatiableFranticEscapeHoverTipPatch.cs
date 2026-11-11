using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch]
internal static class TheInsatiableFranticEscapeHoverTipPatch
{
	private static MethodBase? _target;

	private static bool Prepare()
	{
		if ((object)_target == null)
		{
			_target = AccessTools.PropertyGetter(typeof(FranticEscape), "ExtraHoverTips");
		}
		if (_target != null)
		{
			return true;
		}
		MainFile.Logger.Error("[MonsterReworks] FranticEscape.ExtraHoverTips not found; its tip stays vanilla Sandpit.", 1);
		return false;
	}

	private static MethodBase TargetMethod()
	{
		return _target;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Postfix(FranticEscape __instance, ref IEnumerable<IHoverTip> __result)
	{
		if (MonsterRework.IsMultiplayer(((CardModel)__instance).CombatState))
		{
			SandpitMpPower power = ((CardModel)__instance).Owner.Creature.GetPower<SandpitMpPower>();
			IEnumerable<IHoverTip> enumerable2;
			if (power == null)
			{
				IEnumerable<IHoverTip> enumerable = new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<SandpitMpPower>((int?)null));
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = ((PowerModel)power).HoverTips;
			}
			__result = enumerable2;
		}
	}
}
