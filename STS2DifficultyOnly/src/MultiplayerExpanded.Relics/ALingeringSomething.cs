using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace MultiplayerExpanded.Relics;

[Pool(typeof(EventRelicPool))]
public sealed class ALingeringSomething : MultiplayerExpandedRelic
{
	public override RelicRarity Rarity => (RelicRarity)6;

	protected override IEnumerable<DynamicVar> CanonicalVars
	{
		get
		{
			return new _003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
			{
				(DynamicVar)new EnergyVar(1),
				(DynamicVar)new CardsVar(1)
			});
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy((RelicModel)(object)this));

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != ((RelicModel)this).Owner)
		{
			return amount;
		}
		return amount + ((DynamicVar)((RelicModel)this).DynamicVars.Energy).BaseValue;
	}

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != ((RelicModel)this).Owner)
		{
			return count;
		}
		return count + ((DynamicVar)((RelicModel)this).DynamicVars.Cards).BaseValue;
	}

	public override Task AfterModifyingHandDraw()
	{
		this.Flash();
		return Task.CompletedTask;
	}
}
