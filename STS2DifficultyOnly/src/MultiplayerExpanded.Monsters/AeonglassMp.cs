using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

public sealed class AeonglassMp : MonsterRework
{
	public override Type BaseMonster => typeof(Aeonglass);

	public override async Task AfterAddedToRoom(MonsterModel monster)
	{
		Creature creature = monster.Creature;
		await PowerCmd.Apply<TeamWitheringPresencePower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), creature, 6m, creature, (CardModel)null, false);
	}
}
