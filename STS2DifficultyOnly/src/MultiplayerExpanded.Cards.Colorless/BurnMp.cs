using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MultiplayerExpanded.Features.Reworks;
using MultiplayerExpanded.Powers.Colorless;

namespace MultiplayerExpanded.Cards.Colorless;

public sealed class BurnMp : CardRework
{
	public override Type BaseCard => typeof(Burn);

	public override int Cost => -1;

	public override CardType Type => (CardType)4;

	public override CardRarity Rarity => (CardRarity)8;

	public override TargetType Target => (TargetType)0;

	public override IEnumerable<CardKeyword> Keywords => new CardKeyword[1] { (CardKeyword)4 };

	public override IEnumerable<DynamicVar> Vars
	{
		get
		{
			return new DynamicVar[4]
			{
				(DynamicVar)new DamageVar(2m, (ValueProp)12),
				(DynamicVar)new CalculationBaseVar(0m),
				(DynamicVar)new CalculationExtraVar(1m),
				(DynamicVar)new CalculatedVar("TotalDamage").WithMultiplier((Func<CardModel, Creature, decimal>)((CardModel card, Creature? _) => CurrentTotal(card.CombatState)))
			};
		}
	}

	public override Task OnTurnEndInHand(CardModel card, PlayerChoiceContext choiceContext)
	{
		return Task.CompletedTask;
	}

	internal static int HandTotal(ICombatState combatState)
	{
		int num = 0;
		foreach (Player player in combatState.Players)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			CardPile val = ((playerCombatState != null) ? playerCombatState.Hand : null);
			if (val == null || !player.Creature.IsAlive)
			{
				continue;
			}
			foreach (CardModel card in val.Cards)
			{
				if (card is Burn)
				{
					num += (int)((DynamicVar)card.DynamicVars.Damage).BaseValue;
				}
			}
		}
		return num;
	}

	internal static int CurrentTotal(ICombatState combatState)
	{
		BurningPower burningPower = combatState.Players.Select((Player p) => p.Creature.GetPower<BurningPower>()).FirstOrDefault((BurningPower p) => p != null);
		if (burningPower == null || !burningPower.LockedIn)
		{
			return HandTotal(combatState);
		}
		return ((PowerModel)burningPower).Amount;
	}

	private static bool IsTurnOver(ICombatState combatState)
	{
		if ((int)combatState.CurrentSide != 1)
		{
			return true;
		}
		foreach (Player player in combatState.Players)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			if (playerCombatState != null && (int)playerCombatState.Phase == 5)
			{
				return true;
			}
		}
		return false;
	}

	internal static async Task SyncBurning(ICombatState combatState)
	{
		if (!ReworkPatches.IsMp() || IsTurnOver(combatState))
		{
			return;
		}
		int num = HandTotal(combatState);
		List<Creature> list = (from p in combatState.Players
			select p.Creature into c
			where c.IsAlive
			select c).ToList();
		if (num > 0)
		{
			List<Creature> list2 = list.Where((Creature c) => c.GetPower<BurningPower>() == null).ToList();
			if (list2.Count > 0)
			{
				await PowerCmd.Apply<BurningPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)list2, (decimal)num, (Creature)null, (CardModel)null, false);
			}
			return;
		}
		foreach (Creature item in list)
		{
			await PowerCmd.Remove((PowerModel)(object)item.GetPower<BurningPower>());
		}
	}

	internal static async Task SyncAfter(Task original, ICombatState combatState)
	{
		await original;
		try
		{
			await SyncBurning(combatState);
		}
		catch (Exception value)
		{
			MainFile.Logger.Error($"[Reworks] Burn: syncing Burning failed: {value}", 1);
		}
	}
}
