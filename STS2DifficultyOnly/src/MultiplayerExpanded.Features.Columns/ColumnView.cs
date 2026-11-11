using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.Intents;

namespace MultiplayerExpanded.Features.Columns;

internal static class ColumnView
{
	private sealed class Placement
	{
		public Vector2 Target;

		public Tween? Tween;
	}

	private const float Inset = 15f;

	private const float RowRise = 60f;

	private const float RowShift = 50f;

	private const float FrontY = 200f;

	private static readonly Color BackRowTint = new Color(0.5f, 0.5f, 0.5f, 1f);

	private const double SlideSeconds = 0.35;

	private static readonly Color OtherColumnIntent = new Color(0.5f, 0.5f, 0.5f, 0.75f);

	private static readonly ConditionalWeakTable<NCreature, Placement> Placements = new ConditionalWeakTable<NCreature, Placement>();

	private static readonly MethodInfo? UpdateNavigation = AccessTools.Method(typeof(NCombatRoom), "UpdateCreatureNavigation", (Type[])null, (Type[])null);

	internal static readonly AccessTools.FieldRef<NCreature, Tween?>? ScaleTween = ((AccessTools.Field(typeof(NCreature), "_scaleTween") != null) ? AccessTools.FieldRefAccess<NCreature, Tween>("_scaleTween") : null);

	internal static readonly AccessTools.FieldRef<NIntent, AbstractIntent?>? IntentRef = ((AccessTools.Field(typeof(NIntent), "_intent") != null) ? AccessTools.FieldRefAccess<NIntent, AbstractIntent>("_intent") : null);

	public static void Refresh(ICombatState state)
	{
		if (TestMode.IsOn)
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return;
		}
		try
		{
			Player me = LocalContext.GetMe(state);
			List<(NCreature, List<NCreature>, NCreature, int)> list = new List<(NCreature, List<NCreature>, NCreature, int)>();
			ColumnSide[] array = new ColumnSide[2]
			{
				ColumnSide.Right,
				ColumnSide.Left
			};
			foreach (ColumnSide side in array)
			{
				List<Player> list2 = (from p in state.Players
					where ColumnCombat.ColumnOf(p.Creature) == side
					orderby (p != me) ? 1 : 0
					select p).ToList();
				float num = ((side == ColumnSide.Right) ? 1f : (-1f));
				for (int num2 = 0; num2 < list2.Count; num2++)
				{
					Player player = list2[num2];
					NCreature creatureNode = instance.GetCreatureNode(player.Creature);
					if (creatureNode == null)
					{
						continue;
					}
					List<NCreature> list3 = instance.CreatureNodes.Where((NCreature c) => c.Entity.PetOwner == player).ToList();
					NCreature val = ((player == me) ? list3.FirstOrDefault((NCreature c) => c.Entity.Monster is Osty) : null);
					if (val != null)
					{
						list3.Remove(val);
					}
					float num3 = creatureNode.Visuals.Bounds.Size.X * 0.5f;
					Vector2 target = new Vector2(num * (15f + num3 + (float)num2 * 50f), 200f - (float)num2 * 60f);
					Place(creatureNode, list3, target);
					if (val != null)
					{
						PlaceOsty(val);
					}
					FaceToward(creatureNode, side);
					foreach (NCreature item4 in list3)
					{
						FaceToward(item4, side);
					}
					if (val != null)
					{
						FaceToward(val, side);
					}
					bool backRow = num2 > 0;
					List<NCreature> list5;
					if (val != null)
					{
						List<NCreature> list4 = list3;
						int num4 = 1 + list4.Count;
						list5 = new List<NCreature>(num4);
						CollectionsMarshal.SetCount(list5, num4);
						Span<NCreature> span = CollectionsMarshal.AsSpan(list5);
						int num5 = 0;
						Span<NCreature> span2 = CollectionsMarshal.AsSpan(list4);
						span2.CopyTo(span.Slice(num5, span2.Length));
						num5 += span2.Length;
						span[num5] = val;
					}
					else
					{
						list5 = list3;
					}
					Tint(backRow, creatureNode, list5);
					list.Add((creatureNode, list3, val, num2));
				}
			}
			foreach (var item5 in list.OrderBy(((NCreature Node, List<NCreature> Pets, NCreature Osty, int Row) r) => r.Row))
			{
				NCreature item = item5.Item1;
				List<NCreature> item2 = item5.Item2;
				NCreature item3 = item5.Item3;
				Node parent = ((Node)item).GetParent();
				parent.MoveChild((Node)(object)item, 0);
				for (int num6 = 0; num6 < item2.Count; num6++)
				{
					if (((Node)item2[num6]).GetParent() == parent)
					{
						parent.MoveChild((Node)(object)item2[num6], num6 + 1);
					}
				}
				if (((item3 != null) ? ((Node)item3).GetParent() : null) == parent)
				{
					parent.MoveChild((Node)(object)item3, 0);
				}
			}
			TintIntents(instance, state);
			UpdateNavigation?.Invoke(instance, null);
		}
		catch (Exception value)
		{
			MainFile.Logger.Warn($"[Columns] laying out the columns failed: {value}", 1);
		}
	}

	internal static Vector2? OstyTarget(NCreature owner, Creature osty)
	{
		if (!Placements.TryGetValue(owner, out Placement value))
		{
			return null;
		}
		Vector2 ostyOffsetFromPlayer = NCreature.GetOstyOffsetFromPlayer(osty);
		if (ColumnCombat.ColumnOf(owner.Entity) == ColumnSide.Left)
		{
			ostyOffsetFromPlayer.X = 0f - ostyOffsetFromPlayer.X;
		}
		return value.Target + ostyOffsetFromPlayer;
	}

	private static void PlaceOsty(NCreature osty)
	{
		if (!osty.Entity.IsAlive)
		{
			return;
		}
		if (ScaleTween != null)
		{
			Tween obj = ScaleTween.Invoke(osty);
			if (obj != null)
			{
				obj.Kill();
			}
		}
		osty.OstyScaleToSize((float)osty.Entity.MaxHp, 0.35);
	}

	internal static void TintIntent(NIntent intent, Creature owner)
	{
		if (ColumnCombat.Of(owner.CombatState) == null)
		{
			return;
		}
		Player me = LocalContext.GetMe(owner.CombatState);
		Creature val = ((me != null) ? me.Creature : null);
		AccessTools.FieldRef<NIntent, AbstractIntent?>? intentRef = IntentRef;
		AbstractIntent val2 = ((intentRef != null) ? intentRef.Invoke(intent) : null);
		IReadOnlyList<Creature> readOnlyList = ((val2 != null && ColumnCombat.IsPartyWide(val2)) ? null : ColumnCombat.TargetsOf(owner));
		int num;
		if (readOnlyList != null)
		{
			if (val != null)
			{
				AttackIntent val3 = (AttackIntent)(object)((val2 is AttackIntent) ? val2 : null);
				num = (((val3 != null && !(val3 is ITargetedIntent)) ? ColumnAttacks.Reaches(readOnlyList, val) : readOnlyList.Contains(val)) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		bool flag = (byte)num != 0;
		((CanvasItem)intent).Modulate = (flag ? Colors.White : OtherColumnIntent);
	}

	private static void TintIntents(NCombatRoom room, ICombatState state)
	{
		foreach (Creature enemy in state.Enemies)
		{
			NCreature creatureNode = room.GetCreatureNode(enemy);
			if (creatureNode == null)
			{
				continue;
			}
			foreach (NIntent item in ((IEnumerable)((Node)creatureNode.IntentContainer).GetChildren(false)).OfType<NIntent>())
			{
				TintIntent(item, enemy);
			}
		}
	}

	public static void FaceToward(Creature creature, ColumnSide side)
	{
		if (!TestMode.IsOn)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(creature) : null);
			if (val != null)
			{
				FaceToward(val, side);
			}
		}
	}

	private static void FaceToward(NCreature node, ColumnSide side)
	{
		bool flag = (int)node.Entity.Side == 1;
		bool positive = side == ColumnSide.Right == flag;
		Flip(node.Body, positive);
		if (Beta.IsOn)
		{
			FlipFormVfx(node, positive);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static void FlipFormVfx(NCreature node, bool positive)
	{
		Control formVfxHolder = node.Visuals.FormVfxHolder;
		if (formVfxHolder != null && formVfxHolder.Scale.X > 0f != positive)
		{
			formVfxHolder.Scale *= new Vector2(-1f, 1f);
		}
	}

	private static void Flip(Node2D? body, bool positive)
	{
		if (body != null && body.Scale.X > 0f != positive)
		{
			body.Scale *= new Vector2(-1f, 1f);
		}
	}

	private static void Place(NCreature node, List<NCreature> pets, Vector2 target)
	{
		Vector2 val = target - Placements.GetValue(node, Settled).Target;
		Slide(node, target);
		foreach (NCreature pet in pets)
		{
			Slide(pet, Placements.GetValue(pet, Settled).Target + val);
		}
	}

	private static Placement Settled(NCreature node)
	{
		return new Placement
		{
			Target = ((Control)node).Position
		};
	}

	private static void Slide(NCreature node, Vector2 target)
	{
		Placement value = Placements.GetValue(node, Settled);
		Tween? tween = value.Tween;
		if (tween != null)
		{
			tween.Kill();
		}
		value.Target = target;
		value.Tween = ((Node)node).CreateTween();
		value.Tween.TweenProperty((GodotObject)(object)node, (NodePath)("position"), (Variant)(target), 0.35).SetTrans((TransitionType)7).SetEase((EaseType)1);
	}

	private static void Tint(bool backRow, NCreature node, List<NCreature> pets)
	{
		Color modulate = (backRow ? BackRowTint : Colors.White);
		((CanvasItem)node.Visuals).Modulate = modulate;
		foreach (NCreature pet in pets)
		{
			((CanvasItem)pet.Visuals).Modulate = modulate;
		}
	}

	static ColumnView()
	{
	}
}
