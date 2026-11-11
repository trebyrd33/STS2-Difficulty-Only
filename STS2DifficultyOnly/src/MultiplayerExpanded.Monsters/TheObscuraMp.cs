using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

public sealed class TheObscuraMp : MonsterRework
{
	public override Type BaseMonster => typeof(TheObscura);

	public override async Task AfterAddedToRoom(MonsterModel monster)
	{
		await PowerCmd.Apply<ParanoiaPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), monster.Creature, 1m, monster.Creature, (CardModel)null, false);
	}
}
