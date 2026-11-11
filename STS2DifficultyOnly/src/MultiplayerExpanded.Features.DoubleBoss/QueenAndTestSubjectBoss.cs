using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MultiplayerExpanded.Features.DoubleBoss;

public sealed class QueenAndTestSubjectBoss : DoubleBossEncounter
{
	public override EncounterModel Primary => (EncounterModel)(object)ModelDb.Encounter<QueenBoss>();

	public override EncounterModel Secondary => (EncounterModel)(object)ModelDb.Encounter<TestSubjectBoss>();

	public override string MusicProgressParameter => "queen_progress";

	public override string CustomScenePath => "res://STS2DifficultyOnly/scenes/encounters/double_boss_queen.tscn";

	protected override IReadOnlyList<(MonsterModel Monster, string Slot)> Lineup => new _003C_003Ez__ReadOnlyArray<(MonsterModel, string)>(new (MonsterModel, string)[3]
	{
		((MonsterModel)(object)ModelDb.Monster<TestSubject>(), "left"),
		((MonsterModel)(object)ModelDb.Monster<TorchHeadAmalgam>(), "amalgam"),
		((MonsterModel)(object)ModelDb.Monster<Queen>(), "queen")
	});
}
