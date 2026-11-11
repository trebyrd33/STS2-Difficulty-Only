using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Powers.Colorless;

public sealed class InfectedPower : DelayedSelfDamagePower
{
	private sealed class DazesVar : DynamicVar
	{
		public DazesVar()
			: base("Dazes", 0m)
		{
		}

		protected override decimal GetBaseValueForIConvertible()
		{
			AbstractModel owner = base._owner;
			if (!(owner is InfectedPower infectedPower) || !owner.IsMutable)
			{
				return ((DynamicVar)this).BaseValue;
			}
			return infectedPower.DazeCount;
		}

		public override string ToString()
		{
			return ((int)this.GetBaseValueForIConvertible()).ToString();
		}
	}

	private const int DamagePerDaze = 3;

	private int DazeCount => ((PowerModel)this).Amount / 3;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1]
	{
		new DazesVar()
	};

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.FromCard<Dazed>(false) };

	public override LocString Description
	{
		get
		{
			LocString description = base.Description;
			((PowerModel)this).DynamicVars.AddTo(description);
			return description;
		}
	}

	protected override void PlayTriggerVfx()
	{
		VfxCmd.PlayOnCreatureCenter(((PowerModel)this).Owner, "vfx/vfx_bloody_impact");
		NWormyImpactVfx val = NWormyImpactVfx.Create(((PowerModel)this).Owner);
		if (val != null)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				GodotTreeExtensions.AddChildSafely((Node)(object)instance.CombatVfxContainer, (Node)(object)val);
			}
		}
	}

	protected override async Task AfterSelfDamage(ICombatState combatState, int amount, IEnumerable<DamageResult> results)
	{
		int count = amount / 3;
		if (count <= 0 || !results.Any((DamageResult r) => r.UnblockedDamage > 0))
		{
			return;
		}
		foreach (Player item in combatState.Players.Where((Player p) => p.Creature.IsAlive))
		{
			List<CardModel> list = new List<CardModel>();
			for (int num = 0; num < count; num++)
			{
				list.Add((CardModel)(object)combatState.CreateCard<Dazed>(item));
			}
			CardPreviewExtensions.PreviewAdds(await CardPileCmd.AddGeneratedCardsToCombat((IEnumerable<CardModel>)list, (PileType)3, item, (CardPilePosition)3));
		}
	}
}
