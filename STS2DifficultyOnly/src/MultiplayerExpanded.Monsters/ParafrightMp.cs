using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class ParafrightMp : MonsterRework
{
	public override Type BaseMonster => typeof(Parafright);

	public override Task AfterAddedToRoom(MonsterModel monster)
	{
		return CreatureCmd.SetMaxAndCurrentHp(monster.Creature, Math.Ceiling((decimal)monster.Creature.MaxHp * 0.75m));
	}
}
