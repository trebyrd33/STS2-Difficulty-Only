using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class TeamWitheringPresencePower : MultiplayerExpandedPower
{
	public const int CardsPerWither = 6;

	private static readonly string _iconEntry = ModelDb.GetEntry(typeof(WitheringPresencePower)).ToLowerInvariant();

	public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + _iconEntry + ".tres");

	public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/" + _iconEntry + ".png");

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			Wither val = (Wither)((CardModel)ModelDb.Card<Wither>()).ToMutable();
			if (this.IsMutable)
			{
				MonsterModel monster = this.Owner.Monster;
				Aeonglass val2 = (Aeonglass)(object)((monster is Aeonglass) ? monster : null);
				if (val2 != null)
				{
					val2.MatchWitherToUpgradeCount(val);
				}
			}
			List<IHoverTip> list = new List<IHoverTip>();
			list.Add(HoverTipFactory.FromCard((CardModel)(object)val, false));
			list.AddRange(((CardModel)ModelDb.Card<Wither>()).HoverTips);
			return list;
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		Player owner = cardPlay.Card.Owner;
		Creature player = ((owner != null) ? owner.Creature : null);
		if (player != null && player.IsPlayer)
		{
			int num = this.Amount - 1;
			if (num > 0)
			{
				this.SetAmount(num, false);
				return;
			}
			this.SetAmount(6, false);
			await Cmd.Wait(0.5f, false);
			await CardPileCmd.AddToCombatAndPreview<Wither>(player, (PileType)2, 1, (Player)null, (CardPilePosition)1);
			this.Flash();
		}
	}
}
