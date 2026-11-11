using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Cards.Colorless;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class BurningPower : MultiplayerExpandedPower
{
	private sealed class LiveDamageVar : DynamicVar
	{
		public LiveDamageVar()
			: base("Damage", 0m)
		{
		}

		protected override decimal GetBaseValueForIConvertible()
		{
			if (!(base._owner is BurningPower burningPower))
			{
				return ((DynamicVar)this).BaseValue;
			}
			return ((PowerModel)burningPower).DisplayAmount;
		}

		public override string ToString()
		{
			return ((int)this.GetBaseValueForIConvertible()).ToString();
		}
	}

	private bool _lockedIn;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public bool LockedIn => _lockedIn;

	public override int DisplayAmount
	{
		get
		{
			if (_lockedIn || !((AbstractModel)this).IsMutable || ((PowerModel)this).Owner.CombatState == null)
			{
				return ((PowerModel)this).Amount;
			}
			return BurnMp.HandTotal(((PowerModel)this).Owner.CombatState);
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1]
	{
		new LiveDamageVar()
	};

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			((PowerModel)this).DynamicVars.AddTo(description);
			return description;
		}
	}

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (card is Burn)
		{
			if ((int)oldPileType != 2)
			{
				CardPile pile = card.Pile;
				if (pile == null || (int)pile.Type != 2)
				{
					goto IL_0029;
				}
			}
			this.InvokeDisplayAmountChanged();
		}
		goto IL_0029;
		IL_0029:
		return Task.CompletedTask;
	}

	public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if ((int)side == 1 && ((PowerModel)this).Owner.IsAlive)
		{
			int num = BurnMp.HandTotal(((PowerModel)this).Owner.CombatState) - ((PowerModel)this).Amount;
			_lockedIn = true;
			if (num != 0)
			{
				await PowerCmd.ModifyAmount(choiceContext, (PowerModel)(object)this, (decimal)num, (Creature)null, (CardModel)null, true);
			}
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if ((int)side == 1)
		{
			_lockedIn = false;
			this.InvokeDisplayAmountChanged();
		}
		else if (((PowerModel)this).Owner.IsAlive)
		{
			this.Flash();
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				GodotTreeExtensions.AddChildSafely((Node)(object)instance.CombatVfxContainer, (Node)(object)NGroundFireVfx.Create(((PowerModel)this).Owner, (VfxColor)0));
			}
			SfxCmd.Play("event:/sfx/characters/attack_fire", 1f);
			await GameApi.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), ((PowerModel)this).Owner, ((PowerModel)this).Amount, (ValueProp)12, null, null, null);
			if (((PowerModel)this).Owner.IsAlive)
			{
				await PowerCmd.Remove((PowerModel)(object)this);
			}
		}
	}
}
