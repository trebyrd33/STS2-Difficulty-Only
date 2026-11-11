using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Cards.Tokens;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

public sealed class ColdFeet : TokenCard, IChoosable
{
	public override bool CanBeGeneratedInCombat => false;

	public override int MaxUpgradeLevel => 0;

	public ColdFeet()
		: base(-1, (CardType)4, (CardRarity)8, (TargetType)0)
	{
	}

	public async Task OnChosen()
	{
		await PowerCmd.Apply<ColdFeetPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner.Creature, 1m, this.Owner.Creature, (CardModel)(object)this, false);
	}
}
