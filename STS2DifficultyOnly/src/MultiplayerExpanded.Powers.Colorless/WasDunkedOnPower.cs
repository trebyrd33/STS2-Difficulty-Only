using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Powers.Colorless;

[BetaOnly]
public sealed class WasDunkedOnPower : MultiplayerExpandedPower
{
	private CardModel? _ball;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

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

	public static async Task Dunk(PlayerChoiceContext choiceContext, CardModel ball, Creature holder)
	{
		WasDunkedOnPower power = (WasDunkedOnPower)(object)((PowerModel)ModelDb.Power<WasDunkedOnPower>()).ToMutable(0);
		power.Ball = ball;
		await PowerCmd.Apply(choiceContext, (PowerModel)(object)power, holder, 1m, ball.Owner.Creature, ball, false);
		if (!holder.Powers.Contains((PowerModel)(object)power))
		{
			power.Ball = null;
			await TheBallPower.GiveBack(ball);
		}
	}

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if ((int)side == 1)
		{
			CardModel ball = Ball;
			if (ball != null)
			{
				Ball = null;
				Creature holder = ((PowerModel)this).Owner;
				await PowerCmd.Remove((PowerModel)(object)this);
				await TheBallPower.Hold(choiceContext, ball, holder);
			}
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		CardModel ball = Ball;
		if (ball != null)
		{
			Ball = null;
			await TheBallPower.GiveBack(ball);
		}
	}
}
