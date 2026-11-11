using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BaseLib.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MultiplayerExpanded.Features.Settings;

namespace MultiplayerExpanded.Features.MonsterReworks;

public abstract class MonsterRework
{
	private static readonly AccessTools.FieldRef<MonsterModel, Creature?> CreatureRef = AccessTools.FieldRefAccess<MonsterModel, Creature>("_creature");

	public abstract Type BaseMonster { get; }

	public string LocKey => TypePrefix.GetPrefix(GetType()) + StringHelper.Slugify(GetType().Name);

	public virtual MonsterMoveStateMachine? GenerateMoveStateMachine(MonsterModel monster)
	{
		return null;
	}

	public virtual Task AfterAddedToRoom(MonsterModel monster)
	{
		return Task.CompletedTask;
	}

	public static bool IsMultiplayer(ICombatState? combatState)
	{
		if (MultiplayerExpandedConfig.EnemyReworks)
		{
			if (combatState == null)
			{
				return false;
			}
			return combatState.Players.Count > 1;
		}
		return false;
	}

	public static bool IsMultiplayer(MonsterModel monster)
	{
		Creature obj = CreatureRef.Invoke(monster);
		return IsMultiplayer((obj != null) ? obj.CombatState : null);
	}

	protected static Func<IReadOnlyList<Creature>, Task> VanillaMove(MonsterModel monster, string methodName)
	{
		return (Func<IReadOnlyList<Creature>, Task>)(AccessTools.Method(((object)monster).GetType(), methodName, new Type[1] { typeof(IReadOnlyList<Creature>) }, (Type[])null) ?? throw new MissingMethodException(((object)monster).GetType().Name, methodName)).CreateDelegate(typeof(Func<IReadOnlyList<Creature>, Task>), monster);
	}

	protected static T Vanilla<T>(MonsterModel monster, string memberName)
	{
		Type type = ((object)monster).GetType();
		PropertyInfo propertyInfo = AccessTools.Property(type, memberName);
		if ((object)propertyInfo != null)
		{
			return (T)propertyInfo.GetValue(monster);
		}
		FieldInfo fieldInfo = AccessTools.Field(type, memberName);
		if ((object)fieldInfo != null)
		{
			return (T)fieldInfo.GetValue(monster);
		}
		throw new MissingMemberException(type.Name, memberName);
	}
}
