using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Cards.Curses;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class GluttonyHexPower : MultiplayerExpandedPower
{
	private static readonly string _hexIconEntry = ModelDb.GetEntry(typeof(HexPower)).ToLowerInvariant();

	public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + _hexIconEntry + ".tres");

	public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/" + _hexIconEntry + ".png");

	public override PowerType Type => (PowerType)2;

	public override bool CanBeCleansed => false;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			list.Add(HoverTipFactory.FromCard<Gluttony>(false));
			list.AddRange(HoverTipFactory.FromAffliction<Hexed>(this.Amount));
			return list.AsReadOnly();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new StringVar("Holder", "Someone") };
		}
	}

	public void SetHolder(Creature holder)
	{
		((StringVar)this.DynamicVars["Holder"]).StringValue = holder.Name;
	}

	public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
	{
		if (card.Owner != this.Owner.Player)
		{
			return false;
		}
		if (!(card.Affliction is Hexed))
		{
			return false;
		}
		return keywords.Add((CardKeyword)2);
	}

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		Player player = this.Owner.Player;
		object obj;
		if (player == null)
		{
			obj = null;
		}
		else
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			obj = ((playerCombatState != null) ? playerCombatState.AllCards : null);
		}
		IEnumerable<CardModel> enumerable = (IEnumerable<CardModel>)obj;
		if (enumerable == null)
		{
			return;
		}
		foreach (CardModel item in enumerable)
		{
			await Afflict(item);
		}
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Owner == this.Owner.Player)
		{
			await Afflict(card);
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		if (oldOwner.GetPower<HexPower>() != null)
		{
			return Task.CompletedTask;
		}
		Player player = this.Owner.Player;
		object obj;
		if (player == null)
		{
			obj = null;
		}
		else
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			obj = ((playerCombatState != null) ? playerCombatState.AllCards : null);
		}
		IEnumerable<CardModel> enumerable = (IEnumerable<CardModel>)obj;
		if (enumerable == null)
		{
			return Task.CompletedTask;
		}
		foreach (CardModel item in enumerable)
		{
			if (item.Affliction is Hexed)
			{
				CardCmd.ClearAffliction(item);
			}
		}
		return Task.CompletedTask;
	}

	private async Task Afflict(CardModel card)
	{
		if (card.Affliction == null)
		{
			await CardCmd.Afflict<Hexed>(card, (decimal)this.Amount);
		}
	}
}
