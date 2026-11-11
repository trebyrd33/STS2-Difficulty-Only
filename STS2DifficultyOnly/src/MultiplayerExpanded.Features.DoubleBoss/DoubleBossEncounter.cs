using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MultiplayerExpanded.Features.Columns;

namespace MultiplayerExpanded.Features.DoubleBoss;

public abstract class DoubleBossEncounter : CustomEncounterModel, IColumnEncounter
{
	public const string BehindSlot = "left";

	public static IEnumerable<DoubleBossEncounter> All => new _003C_003Ez__ReadOnlyArray<DoubleBossEncounter>(new DoubleBossEncounter[3]
	{
		ModelDb.Encounter<QueenAndTestSubjectBoss>(),
		ModelDb.Encounter<QueenAndAeonglassBoss>(),
		ModelDb.Encounter<TestSubjectAndAeonglassBoss>()
	});

	public abstract EncounterModel Primary { get; }

	public abstract EncounterModel Secondary { get; }

	public abstract string MusicProgressParameter { get; }

	protected abstract IReadOnlyList<(MonsterModel Monster, string Slot)> Lineup { get; }

	public IReadOnlyList<ColumnSide> Columns
	{
		get
		{
			if (!this.HasScene)
			{
				return Array.Empty<ColumnSide>();
			}
			return new _003C_003Ez__ReadOnlyArray<ColumnSide>(new ColumnSide[2]
			{
				ColumnSide.Right,
				ColumnSide.Left
			});
		}
	}

	public override IEnumerable<MonsterModel> AllPossibleMonsters => Lineup.Select(((MonsterModel Monster, string Slot) l) => l.Monster);

	public override bool FullyCenterPlayers => true;

	public override string CustomBgm => Primary.CustomBgm;

	public override string BossNodePath => Primary.BossNodePath;

	public override MegaSkeletonDataResource? BossNodeSpineResource => Primary.BossNodeSpineResource;

	public override string? CustomRunHistoryIconPath => ImageHelper.GetRoomIconPath((MapPointType)7, (RoomType)3, ((AbstractModel)Primary).Id);

	public override string? CustomRunHistoryIconOutlinePath => ImageHelper.GetRoomIconOutlinePath((MapPointType)7, (RoomType)3, ((AbstractModel)Primary).Id);

	public override IEnumerable<string> ExtraAssetPaths => Primary.ExtraAssetPaths.Concat(Secondary.ExtraAssetPaths);

	protected DoubleBossEncounter()
		: base((RoomType)3, true)
	{
	}

	public static DoubleBossEncounter? For(EncounterModel a, EncounterModel b)
	{
		return All.FirstOrDefault((DoubleBossEncounter pair) => (((AbstractModel)pair.Primary).Id.Equals(((AbstractModel)a).Id) && ((AbstractModel)pair.Secondary).Id.Equals(((AbstractModel)b).Id)) || (((AbstractModel)pair.Primary).Id.Equals(((AbstractModel)b).Id) && ((AbstractModel)pair.Secondary).Id.Equals(((AbstractModel)a).Id)));
	}

	public bool IsBehindPlayers(MonsterModel monster)
	{
		return Lineup.Any(((MonsterModel Monster, string Slot) l) => l.Slot == "left" && ((AbstractModel)l.Monster).Id.Equals(((AbstractModel)monster).Id));
	}

	public ColumnSide? ColumnOf(MonsterModel monster)
	{
		return IsBehindPlayers(monster) ? ColumnSide.Left : ColumnSide.Right;
	}

	public override bool IsValidForAct(ActModel act)
	{
		return false;
	}

	public override BackgroundAssets CustomEncounterBackground(ActModel parentAct, Rng rng)
	{
		return new BackgroundAssets(((AbstractModel)Primary).Id.Entry.ToLowerInvariant(), rng);
	}

	public override float GetCameraScaling()
	{
		return 0.75f;
	}

	public override Vector2 GetCameraOffset()
	{
		return Vector2.Down * 35f;
	}

	protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
	{
        if (!this.HasScene)
            throw new InvalidOperationException("Difficulty Only encounter scene is missing: " + CustomScenePath);
        return Lineup.Select(l => (l.Monster.ToMutable(), (string?)l.Slot)).ToList();
	}
}
