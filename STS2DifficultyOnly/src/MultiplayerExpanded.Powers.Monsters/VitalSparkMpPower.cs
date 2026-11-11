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
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerExpanded.Powers.Monsters;

public sealed class VitalSparkMpPower : MultiplayerExpandedPower
{
	private const int EnergyGain = 1;

	private static readonly string _iconEntry = ModelDb.GetEntry(typeof(VitalSparkPower)).ToLowerInvariant();

	public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/" + _iconEntry + ".tres");

	public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/" + _iconEntry + ".png");

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new DynamicVar[1] { (DynamicVar)new EnergyVar(1) };
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Tainted>(this.Amount).Append(HoverTipFactory.ForEnergy((PowerModel)(object)this));

	public override async Task BeforeCombatStart()
	{
		ICombatState combatState = this.Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		foreach (Creature item in combatState.Allies.ToList())
		{
			if (!item.IsPlayer)
			{
				continue;
			}
			foreach (CardModel item2 in item.Player.PlayerCombatState.AllCards.Where((CardModel c) =>
			{
				return (int)c.Type == 2;
			}).ToList())
			{
				await CardCmd.Afflict<Tainted>(item2, (decimal)this.Amount);
			}
		}
	}

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Affliction == null && (int)card.Type == 2)
		{
			await CardCmd.Afflict<Tainted>(card, (decimal)this.Amount);
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Affliction is Tainted)
		{
			this.Flash();
			await PowerCmd.Apply<TaintedPower>(choiceContext, cardPlay.Card.Owner.Creature, (decimal)this.Amount, (Creature)null, (CardModel)null, false);
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		if (oldOwner.CombatState == null)
		{
			return Task.CompletedTask;
		}
		foreach (Creature item in oldOwner.CombatState.Allies.ToList())
		{
			if (!item.IsPlayer)
			{
				continue;
			}
			foreach (CardModel item2 in item.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Affliction is Tainted).ToList())
			{
				CardCmd.ClearAffliction(item2);
			}
		}
		return Task.CompletedTask;
	}

	public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if ((object)power == this)
		{
			ICombatState combatState = this.Owner.CombatState;
			if (combatState != null)
			{
				foreach (Creature item in combatState.Allies.ToList())
				{
					if (!item.IsPlayer)
					{
						continue;
					}
					foreach (CardModel item2 in item.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Affliction is Tainted))
					{
						item2.Affliction.Amount = this.Amount;
					}
				}
				return Task.CompletedTask;
			}
		}
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if ((int)side != 1)
		{
			return;
		}
		ICombatState combatState = this.Owner.CombatState;
		if (combatState == null)
		{
			return;
		}
		List<Player> list = combatState.Players.Where((Player p) => p.Creature.IsAlive && p.Creature.GetPowerAmount<TaintedPower>() > 0).ToList();
		if (list.Count != 0)
		{
			int most = list.Max((Player p) => p.Creature.GetPowerAmount<TaintedPower>());
			List<Creature> list2 = (from p in list
				where p.Creature.GetPowerAmount<TaintedPower>() == most
				select p.Creature).ToList();
			this.Flash();
			await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, (IEnumerable<Creature>)list2, (decimal)((DynamicVar)this.DynamicVars.Energy).IntValue, this.Owner, (CardModel)null, false);
		}
	}
}
