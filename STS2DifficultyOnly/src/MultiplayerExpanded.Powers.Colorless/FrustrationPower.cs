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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.BetaCompat;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class FrustrationPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(this.Owner) || !this.Owner.IsAlive)
		{
			return;
		}
		Player player = this.Owner.Player;
		if (player == null || this.Amount <= 0)
		{
			return;
		}
		int count = PileTypeExtensions.GetPile((PileType)2, player).Cards.Count;
		if (count == 0)
		{
			return;
		}
		List<Creature> targets = this.CombatState.PlayerCreatures.Where((Creature c) => c.IsAlive).ToList();
		if (targets.Count == 0)
		{
			return;
		}
		this.Flash();
		await GameApi.Damage(choiceContext, (IEnumerable<Creature>)targets, (decimal)(this.Amount * count), (ValueProp)4, (Creature?)null, (CardModel?)null, (CardPlay?)null);
		foreach (Creature item in targets)
		{
			VfxCmd.PlayOnCreatureCenter(item, "vfx/vfx_attack_blunt");
		}
	}
}
