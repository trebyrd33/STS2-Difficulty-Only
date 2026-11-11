using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MultiplayerExpanded.Extensions;

namespace MultiplayerExpanded.Features.DeathMechanic;

internal sealed class ReviveAllRestSiteOption : CustomRestSiteOption
{
	public override string OptionId => "REVIVE_ALL";

	public override string? CustomIconPath => "ui/rest_site/option_revive_all.png".ImagePath();

	public override bool IsEnabled => DeadPlayers().Any();

    public override LocString Description {
        get { var description = base.Description; description.Add("ExtraText", ""); return description; }
    }

	public ReviveAllRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		List<Player> list = DeadPlayers().ToList();
		if (list.Count == 0)
		{
			return false;
		}
		
		foreach (Player target in list)
		{
			decimal healAmount = MendRestSiteOption.GetHealAmount(target);
			await CreatureCmd.Heal(target.Creature, healAmount, true);
			await Hook.AfterRestSiteHeal(target.RunState, target, false);
			if (TestMode.IsOff)
			{
				string scenePath = SceneHelper.GetScenePath("vfx/vfx_cross_heal");
				Node2D val = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>((PackedScene.GenEditState)0);
				NRestSiteRoom instance = NRestSiteRoom.Instance;
				NRestSiteCharacter obj = ((instance != null) ? instance.GetCharacterForPlayer(target) : null);
				if (obj != null)
				{
					GodotTreeExtensions.AddChildSafely((Node)(object)obj, (Node)(object)val);
				}
				val.Position = Vector2.Zero;
			}
		}
		return true;
	}

	private IEnumerable<Player> DeadPlayers()
	{
		return ((IPlayerCollection)this.Owner.RunState).Players.Where((Player p) =>
		{
			Creature creature = p.Creature;
			return creature != null && creature.IsDead;
		});
	}
}
