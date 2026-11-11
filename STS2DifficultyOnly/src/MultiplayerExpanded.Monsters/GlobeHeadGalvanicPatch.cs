using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

[HarmonyPatch(typeof(GalvanicPower), "AfterCardPlayed")]
internal static class GlobeHeadGalvanicPatch
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(GalvanicPower __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		if (!MonsterRework.IsMultiplayer(((PowerModel)__instance).Owner.CombatState))
		{
			return true;
		}
		__result = ((cardPlay.Card.Affliction is Galvanized) ? Charge(__instance, choiceContext) : Task.CompletedTask);
		return false;
	}

	private static async Task Charge(GalvanicPower galvanic, PlayerChoiceContext choiceContext)
	{
		VfxCmd.PlayOnCreature(((PowerModel)galvanic).Owner, "vfx/vfx_attack_lightning");
		await PowerCmd.Apply<SetupStrikePower>(choiceContext, ((PowerModel)galvanic).Owner, (decimal)((PowerModel)galvanic).Amount, ((PowerModel)galvanic).Owner, (CardModel)null, false);
	}
}
