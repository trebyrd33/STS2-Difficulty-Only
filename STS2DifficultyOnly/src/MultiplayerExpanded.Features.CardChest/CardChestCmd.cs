using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.TestSupport;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.CardPicking;

namespace MultiplayerExpanded.Features.CardChest;

public static class CardChestCmd
{
	public sealed record Options
	{
		public CardPickLayout Layout { get; init; }

		public IReadOnlyList<Player>? Participants { get; init; }

		public bool AllowSkip { get; init; } = true;

		public bool RepickLosers { get; init; }

		public bool PaysNormalRewards { get; init; }

		public PlayerChoiceContext? ChoiceContext { get; init; }

		public Func<CardPickingResult, Task>? Award { get; init; }

		public string RngKey { get; init; } = "mpx_card_chest";
	}

	private static readonly Logger _logger = new Logger("CardChestCmd", (LogType)3);

	public static Task<IReadOnlyList<CardPickingResult>> Offer(IRunState runState, IReadOnlyList<CardModel> cards, LocString prompt, bool paysNormalRewards = false)
	{
		return Pick(runState, cards, prompt, new Options
		{
			PaysNormalRewards = paysNormalRewards,
			Award = (CardPickingResult result) => AwardTo(runState, result)
		});
	}

	public static async Task<IReadOnlyList<CardPickingResult>> Pick(IRunState runState, IReadOnlyList<CardModel> cards, LocString prompt, Options options)
	{
		if (cards.Count == 0)
		{
			return Array.Empty<CardPickingResult>();
		}
		PlayerChoiceContext context = options.ChoiceContext;
		ICardPickTransport cardPickTransport2;
		if ((context != null && !(context is BlockingPlayerChoiceContext)) || 1 == 0)
		{
			ICardPickTransport cardPickTransport = new PlayerChoicePickTransport();
			cardPickTransport2 = cardPickTransport;
		}
		else
		{
			ICardPickTransport cardPickTransport = new ActionQueuePickTransport(RunManager.Instance.ActionQueueSynchronizer);
			cardPickTransport2 = cardPickTransport;
		}
		ICardPickTransport transport = cardPickTransport2;
		CardPickingSession session = new CardPickingSession(cards, (IPlayerCollection)(object)runState, options.Participants, LocalContext.GetMe((IPlayerCollection)(object)runState).NetId, transport, GameApi.NewRng(GameApi.Seed(runState.Rng), $"{options.RngKey}_{runState.TotalFloor}"), options.AllowSkip, options.RepickLosers);
		CardChestRoom room = new CardChestRoom(runState.CurrentActIndex)
		{
			Cards = cards,
			Session = session,
			Layout = options.Layout,
			PaysNormalRewards = options.PaysNormalRewards
		};
		CardChestView view = null;
		try
		{
			CardChestRoom.Active = room;
			session.Begin();
			if (context != null)
			{
				await context.SignalPlayerChoiceBegunAsOwner(((IPlayerCollection)runState).Players, (PlayerChoiceOptions)0);
				if (session.LocalPlayerParticipates)
				{
					NPlayerHand instance = NPlayerHand.Instance;
					if (instance != null)
					{
						instance.CancelAllCardPlay();
					}
				}
			}
			view = await CardChestView.Show(room, runState, prompt);
			IReadOnlyList<CardPickingResult> results;
			if (view == null)
			{
				if (TestMode.IsOn && options.AllowSkip)
				{
					session.Abort();
					return Array.Empty<CardPickingResult>();
				}
				_logger.Warn("Card picker could not be shown; skipping locally so the party is not blocked.", 1);
				session.SkipLocally();
				results = await session.Completion;
			}
			else
			{
				results = await view.Run();
			}
			if (options.Award != null)
			{
				foreach (CardPickingResult item in results)
				{
					await options.Award(item);
				}
			}
			if (view != null)
			{
				await view.Close();
				view = null;
			}
			if (context != null)
			{
				await context.SignalPlayerChoiceEnded();
			}
			return results;
		}
		finally
		{
			session.Abort();
			view?.Discard();
			if (CardChestRoom.Active == room)
			{
				CardChestRoom.Active = null;
			}
		}
	}

	private static async Task AwardTo(IRunState runState, CardPickingResult result)
	{
		Player player = result.Player;
		if (player == null)
		{
			return;
		}
		CardModel card = result.Card;
		Player owner = card.Owner;
		if (owner == player)
		{
			return;
		}
		_logger.Debug($"Card {((AbstractModel)card).Id.Entry} passes from player {((owner != null) ? new ulong?(owner.NetId) : ((ulong?)null))} to {player.NetId}", 1);
		if (owner != null)
		{
			MapPointHistoryEntry currentMapPointHistoryEntry = runState.CurrentMapPointHistoryEntry;
			if (currentMapPointHistoryEntry != null)
			{
				currentMapPointHistoryEntry.GetEntry(owner.NetId).CardsRemoved.Add(card.ToSerializable());
			}
		}
		card.RemoveFromCurrentPile(false);
		card.Owner = null;
		((ICardScope)runState).AddCard(card, player);
		await CardPileCmd.Add(card, (PileType)6, (CardPilePosition)1, (AbstractModel)null, false);
	}

	static CardChestCmd()
	{
	}
}
