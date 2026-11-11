using System;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MultiplayerExpanded.Features.Columns;

[HarmonyPatch(typeof(NCreature), "OstyScaleToSize")]
internal static class ColumnOstyPatch
{
	private static bool Prepare()
	{
		if (ColumnView.ScaleTween == null)
		{
			MainFile.Logger.Error("[Columns] NCreature._scaleTween not found; Osty can lag behind a column change.", 1);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyPrefix]
	private static void Prefix(NCreature __instance, out (NCreature Owner, Vector2 Position)? __state)
	{
		__state = null;
		try
		{
			Creature entity = __instance.Entity;
			if (entity == null || !(entity.Monster is Osty))
			{
				return;
			}
			Player petOwner = entity.PetOwner;
			if (petOwner == null || !LocalContext.IsMe(petOwner) || ColumnCombat.Of(entity.CombatState) == null)
			{
				return;
			}
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(petOwner.Creature) : null);
			if (val != null)
			{
				Vector2? val2 = ColumnView.OstyTarget(val, entity);
				if (val2.HasValue)
				{
					Vector2 valueOrDefault = val2.GetValueOrDefault();
					__state = (val, ((Control)val).Position);
					((Control)val).Position = valueOrDefault - NCreature.GetOstyOffsetFromPlayer(entity);
				}
			}
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[Columns] aiming Osty at its owner's column failed: {value}", 1);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, (NCreature Owner, Vector2 Position)? __state)
	{
		if (__state.HasValue)
		{
			(NCreature, Vector2) valueOrDefault = __state.GetValueOrDefault();
			((Control)valueOrDefault.Item1).Position = valueOrDefault.Item2;
		}
		return __exception;
	}
}
