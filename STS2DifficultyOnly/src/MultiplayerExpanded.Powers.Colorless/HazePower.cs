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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class HazePower : MultiplayerExpandedPower
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.FromCard<Dazed>(false) };

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(this.Owner) && this.Owner.IsAlive && this.Owner.Player != null && this.Amount > 0)
		{
			this.Flash();
			List<CardModel> list = new List<CardModel>();
			for (int i = 0; i < this.Amount; i++)
			{
				list.Add((CardModel)(object)this.CombatState.CreateCard<Dazed>(this.Owner.Player));
			}
			CardPreviewExtensions.PreviewAdds(await CardPileCmd.AddGeneratedCardsToCombat((IEnumerable<CardModel>)list, (PileType)1, (Player)null, (CardPilePosition)3));
			await Cmd.Wait(0.5f, false);
		}
	}
}
