using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MultiplayerExpanded.Features.DoubleBoss;

public sealed class TestSubjectAndAeonglassBoss : DoubleBossEncounter
{
	public override EncounterModel Primary => (EncounterModel)(object)ModelDb.Encounter<TestSubjectBoss>();

	public override EncounterModel Secondary => (EncounterModel)(object)ModelDb.Encounter<AeonglassBoss>();

	public override string MusicProgressParameter => "test_subject_progress";

	public override string CustomScenePath => "res://STS2DifficultyOnly/scenes/encounters/double_boss.tscn";

	protected override IReadOnlyList<(MonsterModel Monster, string Slot)> Lineup => new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new (MonsterModel, string)[2]
	{
		((MonsterModel)(object)ModelDb.Monster<Aeonglass>(), "left"),
		((MonsterModel)(object)ModelDb.Monster<TestSubject>(), "right")
	});
}
