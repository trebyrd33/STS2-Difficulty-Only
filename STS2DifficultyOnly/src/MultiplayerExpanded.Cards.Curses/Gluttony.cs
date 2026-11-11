using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Curses;

public sealed class Gluttony : CurseCard
{
	private const string _hexSfx = "event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_hex";

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1] { (CardKeyword)4 };

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword((CardKeyword)2));

	public Gluttony()
		: base(-1, (CardType)5, (CardRarity)9, (TargetType)0)
	{
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (!IsPlayerTurnEnding())
		{
			await ReconcileHex();
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if ((int)side == 1)
		{
			await ReconcileHex();
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if ((int)side == 1)
		{
			await ReconcileHex();
		}
	}

	private async Task ReconcileHex()
	{
		if (this.CombatState == null)
		{
			return;
		}
		Player holder = FindGluttonyHolder();
		bool shouldHaunt = holder != null;
		if (shouldHaunt && AnyPlayerMissingHex())
		{
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_hex", 1f);
		}
		foreach (Player player in this.CombatState.Players)
		{
			Creature creature = player.Creature;
			GluttonyHexPower gluttonyHexPower = creature.GetPower<GluttonyHexPower>();
			if (shouldHaunt)
			{
				if (gluttonyHexPower == null && creature.IsAlive && creature.GetPower<HexPower>() == null)
				{
					gluttonyHexPower = await PowerCmd.Apply<GluttonyHexPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), creature, 1m, (Creature)null, (CardModel)(object)this, true);
				}
				gluttonyHexPower?.SetHolder(holder.Creature);
			}
			else if (gluttonyHexPower != null)
			{
				await PowerCmd.Remove((PowerModel)(object)gluttonyHexPower);
			}
		}
	}

	private bool IsPlayerTurnEnding()
	{
		ICombatState combatState = this.CombatState;
		if (combatState == null || (int)combatState.CurrentSide != 1)
		{
			return false;
		}
		foreach (Player player in this.CombatState.Players)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			PlayerTurnPhase val = (PlayerTurnPhase)((playerCombatState != null) ? ((int)playerCombatState.Phase) : 0);
			if ((uint)((int)val - 4) <= 1u)
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyPlayerMissingHex()
	{
		foreach (Player player in this.CombatState.Players)
		{
			if (player.Creature.IsAlive && player.Creature.GetPower<GluttonyHexPower>() == null)
			{
				return true;
			}
		}
		return false;
	}

	private Player? FindGluttonyHolder()
	{
		foreach (Player player in this.CombatState.Players)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			CardPile val = ((playerCombatState != null) ? playerCombatState.Hand : null);
			if (val == null)
			{
				continue;
			}
			foreach (CardModel card in val.Cards)
			{
				if (card is Gluttony)
				{
					return player;
				}
			}
		}
		return null;
	}
}
