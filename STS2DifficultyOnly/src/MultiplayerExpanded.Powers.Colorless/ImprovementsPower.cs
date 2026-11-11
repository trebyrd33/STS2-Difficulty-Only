using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class ImprovementsPower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	

	

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)6, ((PowerModel)this).Owner.Player).Cards.Where((CardModel c) => c.IsUpgradable).ToList();
		if (list.Count > 0)
		{
			this.Flash();
		}
		for (int num = 0; num < ((PowerModel)this).Amount; num++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			CardModel val = ((PowerModel)this).Owner.Player.RunState.Rng.CombatCardSelection.NextItem<CardModel>((IEnumerable<CardModel>)list);
			list.Remove(val);
			CardCmd.Upgrade(val, (CardPreviewStyle)1);
		}
		return Task.CompletedTask;
	}
}
