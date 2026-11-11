using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Cards.Tokens;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

public sealed class Haze : TokenCard, IChoosable
{
	public override bool CanBeGeneratedInCombat => false;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1] { (DynamicVar)new PowerVar<HazePower>(1m) };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.FromCard<Dazed>(false) };

	public Haze()
		: base(-1, (CardType)4, (CardRarity)8, (TargetType)0)
	{
	}

	public async Task OnChosen()
	{
		await PowerCmd.Apply<HazePower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner.Creature, (decimal)this.DynamicVars["HazePower"].IntValue, this.Owner.Creature, (CardModel)(object)this, false);
	}
}
