using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Potions;

namespace MultiplayerExpanded.Patches;

internal class CardPotionOnUsePatch
{
	public static void Apply(Harmony harmony)
	{
		HarmonyMethod val = new HarmonyMethod(typeof(CardPotionOnUsePatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic));
		Type[] array = new Type[4]
		{
			typeof(AttackPotion),
			typeof(SkillPotion),
			typeof(PowerPotion),
			typeof(ColorlessPotion)
		};
		for (int i = 0; i < array.Length; i++)
		{
			MethodInfo methodInfo = AccessTools.DeclaredMethod(array[i], "OnUse", (Type[])null, (Type[])null);
			RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
			harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Prefix(PotionModel __instance, PlayerChoiceContext choiceContext, Creature? target, ref Task __result)
	{
		if (!(MultiplayerExpandedConfig.MultiplayerPotionChanges && MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMultiplayerRun())) return true;
		Player owner = __instance.Owner;
		List<CardModel> list;
		if (__instance is AttackPotion)
		{
			list = owner.Character.CardPool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint).Where((CardModel c) =>
			{
				return (int)c.Type == 1;
			}).ToList();
		}
		else if (__instance is SkillPotion)
		{
			list = owner.Character.CardPool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint).Where((CardModel c) =>
			{
				return (int)c.Type == 2;
			}).ToList();
		}
		else if (__instance is PowerPotion)
		{
			list = owner.Character.CardPool.GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint).Where((CardModel c) =>
			{
				return (int)c.Type == 3;
			}).ToList();
		}
		else
		{
			list = ((!(__instance is ColorlessPotion)) ? null : ((CardPoolModel)ModelDb.CardPool<ColorlessCardPool>()).GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint).ToList());
		}
		List<CardModel> list2 = list;
		if (list2 == null)
		{
			return true;
		}
		__result = RunForTarget(__instance, choiceContext, target, list2);
		return false;
	}

	private static async Task RunForTarget(PotionModel potion, PlayerChoiceContext choiceContext, Creature? target, List<CardModel> ownerPool)
	{
		Player owner = potion.Owner;
		Player val = ((target != null) ? target.Player : null) ?? owner;
		List<CardModel> list = CardFactory.GetDistinctForCombat(val, (IEnumerable<CardModel>)ownerPool, 3, owner.RunState.Rng.CombatCardGeneration).ToList();
		CardModel val2 = await CardSelectCmd.FromChooseACardScreen(choiceContext, (IReadOnlyList<CardModel>)list, val, true);
		if (val2 != null)
		{
			val2.SetToFreeThisTurn();
			await CardPileCmd.AddGeneratedCardToCombat(val2, (PileType)2, val, (CardPilePosition)1);
		}
	}
}
