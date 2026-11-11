using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class SandpitMpPower : MultiplayerExpandedPower
{
	private const float PaddingDistanceFromMonster = 450f;

	private const float PaddingDistanceFromOriginal = 50f;

	private const float TweenTime = 0.25f;

	private static readonly string _iconEntry = ModelDb.GetEntry(typeof(SandpitPower)).ToLowerInvariant();

	private int _initialAmount;

	private float _initialTargetPosition;

	public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + _iconEntry + ".tres");

	public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/" + _iconEntry + ".png");

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override bool CanBeCleansed => false;

	private IReadOnlyList<Creature> AllAffectedCreatures => ((IEnumerable<Creature>)new Creature[1] { this.Owner }).Concat((IEnumerable<Creature>)this.Owner.Pets).ToList();

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (TestMode.IsOn || this.Applier == null)
		{
			return Task.CompletedTask;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(this.Owner) : null);
		if (val == null)
		{
			return Task.CompletedTask;
		}
		_initialAmount = this.Amount;
		_initialTargetPosition = ((Control)val).GlobalPosition.X;
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if ((int)side == 2)
		{
			await PowerCmd.Decrement((PowerModel)(object)this);
		}
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (TestMode.IsOn || (object)power != this)
		{
			return;
		}
		await UpdateCreaturePositions();
		if (LocalContext.IsMe(this.Owner))
		{
			int num = Mathf.Clamp(6 - this.Amount, 0, 5);
			NRunMusicController instance = NRunMusicController.Instance;
			if (instance != null)
			{
				instance.UpdateMusicParameter(TheInsatiable.TheInsatiableTrackName, (float)num);
			}
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature == this.Applier && !wasRemovalPrevented && this.Owner.HasPower<SandpitMpPower>())
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		if (this.Amount > 0 || oldOwner.IsDead)
		{
			return;
		}
		Creature applier = this.Applier;
		if (applier == null || !applier.IsAlive)
		{
			return;
		}
		if (TestMode.IsOff)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				NCreature creatureNode = instance.GetCreatureNode(applier);
				if (creatureNode != null)
				{
					Tween val = ((Node)instance).CreateTween();
					float num = ((Control)creatureNode).GlobalPosition.X - 450f;
					foreach (Creature allAffectedCreature in AllAffectedCreatures)
					{
						NCreature creatureNode2 = instance.GetCreatureNode(allAffectedCreature);
						if (creatureNode2 != null)
						{
							val.Parallel().TweenProperty((GodotObject)(object)creatureNode2, (NodePath)("global_position:x"), (Variant)(num), 0.699999988079071).SetEase((EaseType)2)
								.SetTrans((TransitionType)1);
						}
					}
				}
			}
		}
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_finisher", 1f);
		await CreatureCmd.TriggerAnim(applier, "EatPlayerTrigger", 0f);
		await Cmd.Wait(0.5f, false);
		foreach (Creature allAffectedCreature2 in AllAffectedCreatures)
		{
			if (TestMode.IsOff)
			{
				NCombatRoom instance2 = NCombatRoom.Instance;
				NCreature val2 = ((instance2 != null) ? instance2.GetCreatureNode(allAffectedCreature2) : null);
				if (val2 != null)
				{
					((CanvasItem)val2.Visuals).Visible = false;
				}
			}
			if (allAffectedCreature2.IsPlayer || allAffectedCreature2.Monster is Osty)
			{
				await CreatureCmd.Kill(allAffectedCreature2, true);
			}
		}
	}

	public override async Task AfterCreatureAddedToCombat(Creature creature)
	{
		if (creature.Side == this.Owner.Side)
		{
			await UpdateCreaturePositions();
		}
	}

	public override async Task AfterOstyRevived(Creature osty)
	{
		await UpdateCreaturePositions();
	}

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if ((int)side == 2)
		{
			await UpdateCreaturePositions();
		}
	}

	private async Task UpdateCreaturePositions()
	{
		if (TestMode.IsOn || _initialAmount <= 0)
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null || this.Applier == null)
		{
			return;
		}
		NCreature creatureNode = instance.GetCreatureNode(this.Applier);
		if (creatureNode == null)
		{
			return;
		}
		NCreature creatureNode2 = instance.GetCreatureNode(this.Owner);
		if (creatureNode2 == null)
		{
			return;
		}
		float num = ((Control)creatureNode).GlobalPosition.X - 450f;
		float num2 = _initialTargetPosition + 50f;
		float num3 = (num2 - num) / (float)_initialAmount;
		int num4 = Math.Min(this.Amount, _initialAmount);
		int num5 = Math.Max(this.Amount - _initialAmount, 0);
		NCreature val = creatureNode2;
		float num6 = 0f;
		Player player = this.Owner.Player;
		if (player != null && player.IsOstyAlive && LocalContext.IsMe(this.Owner))
		{
			NCreature creatureNode3 = instance.GetCreatureNode(player.Osty);
			if (creatureNode3 != null)
			{
				val = creatureNode3;
				float x = NCreature.GetOstyOffsetFromPlayer(creatureNode3.Entity).X;
				num2 += x;
				num3 = (num2 - num) / (float)_initialAmount;
				float num7 = 100f * (1f - (float)num4 / (float)_initialAmount);
				num6 = x - num7;
			}
		}
		float num8 = ((Control)creatureNode).GlobalPosition.X - 400f + num3 * (float)num4 + num3 * ((float)num5 / ((float)num5 + 2f));
		Tween val2 = null;
		foreach (Creature allAffectedCreature in AllAffectedCreatures)
		{
			float num9 = num8 - ((Control)val).GlobalPosition.X;
			if (allAffectedCreature != val.Entity)
			{
				num9 = num8 - num6 - ((Control)creatureNode2).GlobalPosition.X;
			}
			if (Math.Abs(num9) <= 5f || allAffectedCreature.IsDead)
			{
				continue;
			}
			NCreature creatureNode4 = instance.GetCreatureNode(allAffectedCreature);
			if (creatureNode4 != null)
			{
				if (val2 == null)
				{
					val2 = ((Node)instance).CreateTween().SetParallel(true).SetEase((EaseType)1)
						.SetTrans((TransitionType)7);
				}
				val2.TweenProperty((GodotObject)(object)creatureNode4, (NodePath)("global_position:x"), (Variant)(((Control)creatureNode4).GlobalPosition.X + num9), 0.25);
			}
		}
		if (val2 != null)
		{
			await TweenHelper.AwaitFinished(val2, (Node)(object)instance);
		}
	}
}
