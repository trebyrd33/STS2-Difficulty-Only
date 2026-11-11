using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MultiplayerExpanded.Cards.Tokens;
using MultiplayerExpanded.Extensions;
using MultiplayerExpanded.Features.MonsterReworks;
using MultiplayerExpanded.Powers.Monsters;

namespace MultiplayerExpanded.Monsters;

public sealed class TheInsatiableMp : MonsterRework
{
	private const int StatusesPerPile = 3;

	private const int SandpitTurns = 4;

	private static readonly MethodInfo? HasLiquifiedSetter = AccessTools.PropertySetter(typeof(TheInsatiable), "HasLiquified");

	public override Type BaseMonster => typeof(TheInsatiable);

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		TheInsatiable insatiable = (TheInsatiable)monster;
		int num = MonsterRework.Vanilla<int>((MonsterModel)(object)insatiable, "ThrashDamage");
		MoveState val = new MoveState("LIQUIFY_GROUND_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> targets) => LiquifyMove(insatiable, targets)), new AbstractIntent[2]
		{
			(AbstractIntent)new BuffIntent(),
			(AbstractIntent)new StatusIntent(6)
		});
		MoveState val2 = new MoveState("THRASH_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)insatiable, "ThrashMove"), new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(num, 2) });
		MoveState val3 = new MoveState("THRASH_MOVE_2", MonsterRework.VanillaMove((MonsterModel)(object)insatiable, "ThrashMove"), new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(num, 2) });
		MoveState val4 = new MoveState("LUNGING_BITE_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)insatiable, "BiteMove"), new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)insatiable, "BiteDamage")) });
		MoveState val5 = new MoveState("SALIVATE_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)insatiable, "SalivateMove"), new AbstractIntent[1] { (AbstractIntent)new BuffIntent() });
		val.FollowUpState = (MonsterState)(object)val2;
		val2.FollowUpState = (MonsterState)(object)val4;
		val4.FollowUpState = (MonsterState)(object)val5;
		val5.FollowUpState = (MonsterState)(object)val3;
		val3.FollowUpState = (MonsterState)(object)val2;
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val,
			(MonsterState)(object)val4,
			(MonsterState)(object)val2,
			(MonsterState)(object)val3,
			(MonsterState)(object)val5
		}, (MonsterState)(object)val);
	}

	private static async Task LiquifyMove(TheInsatiable monster, IReadOnlyList<Creature> targets)
	{
		Creature creature = ((MonsterModel)monster).Creature;
		SfxCmd.Play(MonsterRework.Vanilla<string>((MonsterModel)(object)monster, "_liquifyGroundSfx"), 1f);
		await CreatureCmd.TriggerAnim(creature, MonsterRework.Vanilla<string>((MonsterModel)(object)monster, "_liquifySandTrigger"), 0f);
		await Cmd.Wait(0.5f, false);
		VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_scream");
		await Cmd.Wait(0.75f, false);
		List<Creature> list = (from c in targets.Select((Creature t) =>
			{
				Player val2 = t.Player ?? t.PetOwner;
				return (val2 == null) ? null : val2.Creature;
			}).OfType<Creature>()
			where c.IsAlive
			select c).Distinct().ToList();
		await PowerCmd.Apply<SandpitMpPower>((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), (IEnumerable<Creature>)list, 4m, creature, (CardModel)null, false);
		ICombatState combatState = ((MonsterModel)monster).CombatState;
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		foreach (Creature target in targets)
		{
			Player player = target.Player ?? target.PetOwner;
			if (player == null || !player.Creature.IsAlive)
			{
				continue;
			}
			PileType[] array = new PileType[2]
			{
				(PileType)1,
				(PileType)3
			};
			foreach (PileType val in array)
			{
				List<CardModel> list2 = new List<CardModel>();
				for (int num2 = 0; num2 < 2; num2++)
				{
					list2.Add((CardModel)(object)combatState.CreateCard<FranticEscape>(player));
				}
				list2.Add((CardModel)(object)combatState.CreateCard<FranticPull>(player));
				List<CardPileAddResult> list3 = results;
				list3.AddRange(await CardPileCmd.AddGeneratedCardsToCombat((IEnumerable<CardModel>)list2, val, (Player)null, (CardPilePosition)3));
			}
		}
		CardPreviewExtensions.PreviewAdds(results);
		await Cmd.Wait(1f, false);
		if (HasLiquifiedSetter != null)
		{
			HasLiquifiedSetter.Invoke(monster, new object[1] { true });
		}
		else
		{
			MainFile.Logger.Error("[TheInsatiableMp] TheInsatiable.HasLiquified setter not found; hit animation stays on the intro variant.", 1);
		}
	}
}
