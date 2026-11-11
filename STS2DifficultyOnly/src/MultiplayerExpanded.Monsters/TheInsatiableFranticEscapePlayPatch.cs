using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch]
internal static class TheInsatiableFranticEscapePlayPatch
{
	private static MethodBase? _target;

	private static bool Prepare()
	{
		if ((object)_target == null)
		{
			_target = AccessTools.Method(typeof(FranticEscape), "OnPlay", new Type[2]
			{
				typeof(PlayerChoiceContext),
				typeof(CardPlay)
			}, (Type[])null);
		}
		if (_target != null)
		{
			return true;
		}
		MainFile.Logger.Error("[MonsterReworks] FranticEscape.OnPlay not found; Frantic Escape won't raise Sandpit in multiplayer.", 1);
		return false;
	}

	private static MethodBase TargetMethod()
	{
		return _target;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(FranticEscape __instance, PlayerChoiceContext choiceContext, ref Task __result)
	{
		if (MonsterRework.IsMultiplayer(((CardModel)__instance).CombatState))
		{
			SandpitMpPower power = ((CardModel)__instance).Owner.Creature.GetPower<SandpitMpPower>();
			if (power != null)
			{
				__result = Escape(__instance, choiceContext, power);
				return false;
			}
		}
		return true;
	}

	private static async Task Escape(FranticEscape card, PlayerChoiceContext choiceContext, SandpitMpPower sandpit)
	{
		await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)sandpit, 1m, ((PowerModel)sandpit).Applier, (CardModel)(object)card, false);
		((CardModel)card).EnergyCost.AddThisCombat(1, false);
	}
}
