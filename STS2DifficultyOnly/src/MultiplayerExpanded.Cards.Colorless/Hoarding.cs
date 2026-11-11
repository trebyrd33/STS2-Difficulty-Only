using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Cards.Tokens;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

public sealed class Hoarding : TokenCard, IChoosable
{
	public override bool CanBeGeneratedInCombat => false;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1] { (DynamicVar)new PowerVar<HoardingPower>(1m) };

	public Hoarding()
		: base(-1, (CardType)4, (CardRarity)8, (TargetType)0)
	{
	}

	public async Task OnChosen()
	{
		await PowerCmd.Apply<HoardingPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner.Creature, (decimal)this.DynamicVars["HoardingPower"].IntValue, this.Owner.Creature, (CardModel)(object)this, false);
	}
}
