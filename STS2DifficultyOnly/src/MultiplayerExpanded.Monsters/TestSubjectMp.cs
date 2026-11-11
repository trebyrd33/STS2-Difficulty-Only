using System;
using MegaCrit.Sts2.Core.Models.Monsters;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class TestSubjectMp : MonsterRework
{
	public override Type BaseMonster => typeof(TestSubject);
}
