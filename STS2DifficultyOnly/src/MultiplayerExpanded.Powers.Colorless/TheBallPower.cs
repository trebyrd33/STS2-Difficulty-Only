using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Extensions;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Powers.Colorless;

[BetaOnly]
public sealed class TheBallPower : MultiplayerExpandedPower
{
	private CardModel? _ball;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override PowerInstanceType InstanceType => (PowerInstanceType)1;

	public CardModel? Ball
	{
		get
		{
			return _ball;
		}
		private set
		{
			((AbstractModel)this).AssertMutable();
			_ball = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			if (Ball != null)
			{
				return new IHoverTip[1] { HoverTipFactory.FromCard(Ball, false) };
			}
			return Array.Empty<IHoverTip>();
		}
	}

	private static int Threshold(CardModel ball)
	{
		return (int)((DynamicVar)ball.DynamicVars.Damage).BaseValue;
	}

	public static async Task Hold(PlayerChoiceContext choiceContext, CardModel ball, Creature holder)
	{
		int threshold = Threshold(ball);
		TheBallPower power = (TheBallPower)(object)((PowerModel)ModelDb.Power<TheBallPower>()).ToMutable(0);
		power.Ball = ball;
		await PowerCmd.Apply(choiceContext, (PowerModel)(object)power, holder, (decimal)threshold, ball.Owner.Creature, (CardModel)null, false);
		if (holder.Powers.Contains((PowerModel)(object)power))
		{
			if (((PowerModel)power).Amount != threshold)
			{
				((PowerModel)power).SetAmount(threshold, false);
			}
		}
		else
		{
			power.Ball = null;
			await GiveBack(ball);
		}
	}

	public static async Task GiveBack(CardModel ball)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		CardPile pile = ball.Pile;
		if (pile != null && (int)pile.Type != 5)
		{
			return;
		}
		Player thrower = ball.Owner;
		ICombatState combatState = thrower.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}
		List<Player> list = combatState.Players.Where((Player p) => p != thrower && p.Creature.IsAlive).ToList();
		Player val = ((list.Count > 0) ? thrower.RunState.Rng.CombatTargets.NextItem<Player>((IEnumerable<Player>)list) : thrower);
		if (!val.Creature.IsAlive)
		{
			return;
		}
		ball.HasBeenRemovedFromState = false;
		ball.GiveToAnotherPlayer(val);
		CardPileAddResult val2 = await CardPileCmd.Add(ball, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
		if (val2.success)
		{
			CardPile pile2 = ball.Pile;
			if (pile2 == null || (int)pile2.Type != 2)
			{
				CardPreviewExtensions.PreviewAdd(val2);
			}
		}
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (target != ((PowerModel)this).Owner || Ball == null || result.UnblockedDamage <= 0)
		{
			return;
		}
		object obj = ((dealer != null) ? dealer.Player : null);
		if (obj == null)
		{
			obj = ((dealer != null) ? dealer.PetOwner : null);
		}
		if (obj != null)
		{
			int num = ((PowerModel)this).Amount - result.UnblockedDamage;
			if (num > 0)
			{
				((PowerModel)this).SetAmount(num, false);
				return;
			}
			this.Flash();
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if ((int)side == 1 && Ball != null && ((PowerModel)this).Amount != Threshold(Ball))
		{
			((PowerModel)this).SetAmount(Threshold(Ball), false);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		CardModel ball = Ball;
		if (ball != null)
		{
			Ball = null;
			await GiveBack(ball);
		}
	}
}
