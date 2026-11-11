using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class BeckonedPower : DelayedSelfDamagePower
{
	protected override ValueProp DamageProps => (ValueProp)14;
}
