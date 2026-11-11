using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

internal static class GlobeHeadGalvanizedText
{
	private const string LocKey = "MULTIPLAYEREXPANDED-GALVANIZED";

	private static bool Applies(AfflictionModel affliction)
	{
		if (affliction is Galvanized && affliction.HasCard)
		{
			Player owner = affliction.Card.Owner;
			object combatState;
			if (owner == null)
			{
				combatState = null;
			}
			else
			{
				Creature creature = owner.Creature;
				combatState = ((creature != null) ? creature.CombatState : null);
			}
			return MonsterRework.IsMultiplayer((ICombatState?)combatState);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static LocString? ExtraCardText(AfflictionModel affliction)
	{
		if (!Applies(affliction))
		{
			return affliction.DynamicExtraCardText;
		}
		LocString val = new LocString("afflictions", "MULTIPLAYEREXPANDED-GALVANIZED.extraCardText");
		val.Add("Amount", (decimal)affliction.Amount);
		return val;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IEnumerable<IHoverTip> AfflictionHoverTips(AfflictionModel affliction)
	{
		if (!Applies(affliction))
		{
			return affliction.HoverTips;
		}
		return Tips(affliction.Amount);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IEnumerable<IHoverTip> PowerExtraHoverTips(IEnumerable<IHoverTip> tips, PowerModel power)
	{
		GalvanicPower val = (GalvanicPower)(object)((power is GalvanicPower) ? power : null);
		if (val == null || !((AbstractModel)power).IsMutable || !MonsterRework.IsMultiplayer(((PowerModel)val).Owner.CombatState))
		{
			return tips;
		}
		return Tips(((PowerModel)val).Amount);
	}

	private static IEnumerable<IHoverTip> Tips(int amount)
	{
		LocString val = new LocString("afflictions", "MULTIPLAYEREXPANDED-GALVANIZED.description");
		val.Add("Amount", (decimal)amount);
		return new _003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
		{
			(IHoverTip)(object)new HoverTip((AfflictionModel)(object)ModelDb.Affliction<Galvanized>(), val),
			HoverTipFactory.FromPower<SetupStrikePower>((int?)null)
		});
	}

	internal static IEnumerable<CodeInstruction> ReplaceCall(IEnumerable<CodeInstruction> instructions, MethodInfo original, MethodInfo replacement, string site)
	{
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (CodeInstructionExtensions.Calls(instruction, original))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = replacement;
				replaced++;
			}
			yield return instruction;
		}
		if (replaced == 0)
		{
			MainFile.Logger.Error($"[GlobeHead] {site} transpiler found no {original.Name} call — Galvanized " + "shows vanilla text in multiplayer. The method's IL likely changed in a game update.", 1);
		}
	}
}
