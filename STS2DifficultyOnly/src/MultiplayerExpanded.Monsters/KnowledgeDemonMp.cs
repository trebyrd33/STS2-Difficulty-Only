using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MultiplayerExpanded.Cards.Colorless;
using MultiplayerExpanded.Features.CardChest;
using MultiplayerExpanded.Features.CardPicking;
using MultiplayerExpanded.Features.MonsterReworks;

namespace MultiplayerExpanded.Monsters;

public sealed class KnowledgeDemonMp : MonsterRework
{
	private const int SetCount = 3;

	public override Type BaseMonster => typeof(KnowledgeDemon);

	private static CardModel[] ExtraSet(int round)
	{
		return round switch
		{
			0 => new CardModel[2]
			{
				(CardModel)ModelDb.Card<MultiplayerExpanded.Cards.Colorless.Haze>(),
				(CardModel)ModelDb.Card<Hoarding>()
			}, 
			1 => new CardModel[2]
			{
				(CardModel)ModelDb.Card<Frustration>(),
				(CardModel)ModelDb.Card<InnerWorld>()
			}, 
			2 => new CardModel[2]
			{
				(CardModel)ModelDb.Card<ColdFeet>(),
				(CardModel)ModelDb.Card<Martyrdom>()
			}, 
			_ => throw new ArgumentOutOfRangeException("round"), 
		};
	}

	public override MonsterMoveStateMachine GenerateMoveStateMachine(MonsterModel monster)
	{
		KnowledgeDemon demon = (KnowledgeDemon)monster;
		MoveState val = new MoveState("CURSE_OF_KNOWLEDGE_MOVE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => CurseOfKnowledge(demon)), new AbstractIntent[1] { (AbstractIntent)new DebuffIntent(false) });
		MoveState val2 = new MoveState("SLAP_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)demon, "SlapMove"), new AbstractIntent[1] { (AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)demon, "SlapDamage")) });
		MoveState val3 = new MoveState("KNOWLEDGE_OVERWHELMING_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)demon, "KnowledgeOverwhelmingMove"), new AbstractIntent[1] { (AbstractIntent)new MultiAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)demon, "KnowledgeOverwhelmingDamage"), 3) });
		MoveState val4 = new MoveState("PONDER_MOVE", MonsterRework.VanillaMove((MonsterModel)(object)demon, "PonderMove"), new AbstractIntent[3]
		{
			(AbstractIntent)new SingleAttackIntent(MonsterRework.Vanilla<int>((MonsterModel)(object)demon, "PonderDamage")),
			(AbstractIntent)new HealIntent(),
			(AbstractIntent)new BuffIntent()
		});
		ConditionalBranchState val5 = new ConditionalBranchState("CurseOfKnowledgeBranch");
		val.FollowUpState = (MonsterState)(object)val2;
		val2.FollowUpState = (MonsterState)(object)val3;
		val3.FollowUpState = (MonsterState)(object)val4;
		val4.FollowUpState = (MonsterState)(object)val5;
		val5.AddState((MonsterState)(object)val, (Func<bool>)(() => Counter(demon) < 3));
		val5.AddState((MonsterState)(object)val2, (Func<bool>)(() => Counter(demon) >= 3));
		return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new List<MonsterState>
		{
			(MonsterState)(object)val5,
			(MonsterState)(object)val,
			(MonsterState)(object)val2,
			(MonsterState)(object)val4,
			(MonsterState)(object)val3
		}, (MonsterState)(object)val);
	}

	private static int Counter(KnowledgeDemon demon)
	{
		return MonsterRework.Vanilla<int>((MonsterModel)(object)demon, "_curseOfKnowledgeCounter");
	}

	private static async Task CurseOfKnowledge(KnowledgeDemon demon)
	{
		int round = Counter(demon);
		if (round >= 3)
		{
			throw new InvalidOperationException($"There are no valid sets at this index {round}");
		}
		TalkCmd.Play(MonsterRework.Vanilla<LocString>((MonsterModel)(object)demon, "_curseOfKnowledgeStartLine"), ((MonsterModel)demon).Creature, (VfxColor)7, (VfxDuration)3);
		await CreatureCmd.TriggerAnim(((MonsterModel)demon).Creature, "MindRotTrigger", 1f);
		ICombatState combatState = ((MonsterModel)demon).CombatState;
		List<Player> participants = combatState.Players.Where((Player p) => p.Creature.IsAlive).ToList();
		if (participants.Count > 0)
		{
			List<CardModel> pool = MonsterRework.Vanilla<IReadOnlyList<IReadOnlyList<IChoosable>>>((MonsterModel)(object)demon, "_curseOfKnowledgeSets")[round].Cast<CardModel>().Concat(ExtraSet(round)).ToList();
			int disintegrationDamage = MonsterRework.Vanilla<int[]>((MonsterModel)(object)demon, "_disintegrationDamageValues")[round];
			List<CardModel> shown = pool.Select((CardModel c) => CreateCurse(demon, c, participants[0], disintegrationDamage)).ToList();
			await CardChestCmd.Pick(((MonsterModel)demon).Creature.CombatState.RunState, shown, new LocString("gameplay_ui", "CHOOSE_CARD_HEADER"), new CardChestCmd.Options
			{
				Layout = CardPickLayout.ChooseACard,
				Participants = participants,
				AllowSkip = false,
				RepickLosers = true,
				ChoiceContext = (PlayerChoiceContext?)new BlockingPlayerChoiceContext(),
				RngKey = $"mpx_knowledge_demon_curse_{round}",
				Award = (CardPickingResult result) => Award(demon, pool, shown, result, disintegrationDamage)
			});
		}
		TalkCmd.Play(MonsterRework.Vanilla<LocString>((MonsterModel)(object)demon, "_curseOfKnowledgeDoneLine"), ((MonsterModel)demon).Creature, (VfxColor)7, (VfxDuration)3);
		SetCounter(demon, round + 1);
	}

	private static CardModel CreateCurse(KnowledgeDemon demon, CardModel canonical, Player owner, int disintegrationDamage)
	{
		CardModel val = ((MonsterModel)demon).CombatState.CreateCard(canonical, owner);
		if (val is Disintegration)
		{
			val.DynamicVars["DisintegrationPower"].BaseValue = disintegrationDamage;
		}
		return val;
	}

	private static async Task Award(KnowledgeDemon demon, List<CardModel> pool, List<CardModel> shown, CardPickingResult result, int disintegrationDamage)
	{
		Player player = result.Player;
		if (player != null && player.Creature.IsAlive)
		{
			int num = shown.IndexOf(result.Card);
			if (num >= 0)
			{
				await ((IChoosable)CreateCurse(demon, pool[num], player, disintegrationDamage)).OnChosen();
			}
		}
	}

	private static void SetCounter(KnowledgeDemon demon, int value)
	{
		AccessTools.PropertySetter(typeof(KnowledgeDemon), "CurseOfKnowledgeCounter").Invoke(demon, new object[1] { value });
	}
}
