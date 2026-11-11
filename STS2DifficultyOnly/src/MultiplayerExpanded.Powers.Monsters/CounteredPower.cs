using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class CounteredPower : MultiplayerExpandedPower
{
	private const string DamageIncrease = "DamageIncrease";

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1]
			{
				new DynamicVar("DamageIncrease", 1.5m)
			};
		}
	}

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			this.DynamicVars.AddTo(description);
			return description;
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (target != this.Owner || !ValuePropExtensions.IsPoweredAttack(props) || dealer == null || !dealer.HasPower<CounterpunchPower>())
		{
			return 1m;
		}
		return this.DynamicVars["DamageIncrease"].BaseValue;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if ((int)side == 2)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
