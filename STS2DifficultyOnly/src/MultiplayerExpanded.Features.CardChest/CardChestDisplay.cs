using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Runs;
using MultiplayerExpanded.Features.CardPicking;

namespace MultiplayerExpanded.Features.CardChest;

public sealed class CardChestDisplay
{
	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, List<NTreasureRoomRelicHolder>> MultiplayerHolders = AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, List<NTreasureRoomRelicHolder>>("_multiplayerHolders");

	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, List<NTreasureRoomRelicHolder>> HoldersInUse = AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, List<NTreasureRoomRelicHolder>>("_holdersInUse");

	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, NHandImageCollection> Hands = AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, NHandImageCollection>("_hands");

	private static readonly AccessTools.FieldRef<NTreasureRoomRelicCollection, Control> FightBackstop = AccessTools.FieldRefAccess<NTreasureRoomRelicCollection, Control>("_fightBackstop");

	private static readonly AccessTools.FieldRef<NHandImage, TextureRect> HandTextureRect = AccessTools.FieldRefAccess<NHandImage, TextureRect>("_textureRect");

	private readonly NTreasureRoomRelicCollection _collection;

	private readonly CardChestRoom _room;

	private readonly IRunState _runState;

	private List<NTreasureRoomRelicHolder> _slots = new List<NTreasureRoomRelicHolder>();

	public const float RowCardScale = 0.8f;

	public const float RowHoverScale = 1.25f;

	private const float RowCardSpacing = 340f;

	private const float RowOffsetY = 60f;

	public CardChestDisplay(NTreasureRoomRelicCollection collection, CardChestRoom room, IRunState runState)
	{
		_collection = collection;
		_room = room;
		_runState = runState;
	}

	public void PlaceCards()
	{
		if (_room.Layout == CardPickLayout.ChooseACard)
		{
			PlaceInRow();
		}
		else
		{
			PlaceInChest();
		}
		for (int i = 0; i < _slots.Count; i++)
		{
			Dress(_slots[i], _room.Cards[i], i);
		}
		HoldersInUse.Invoke(_collection) = _slots;
		HideSittingOutHands();
	}

	private void PlaceInRow()
	{
		IReadOnlyList<CardModel> cards = _room.Cards;
		List<NTreasureRoomRelicHolder> list = MultiplayerHolders.Invoke(_collection);
		NTreasureRoomRelicHolder val = list[0];
		Control parent = ((Node)val).GetParent<Control>();
		((CanvasItem)_collection.SingleplayerRelicHolder).Visible = false;
		foreach (NTreasureRoomRelicHolder item in list)
		{
			((CanvasItem)item).Visible = false;
		}
		Vector2 val2 = NCard.defaultSize * 0.8f;
		Rect2 viewportRect = ((CanvasItem)_collection).GetViewportRect();
		Vector2 val3 = viewportRect.Size * 0.5f + Vector2.Down * 60f;
		_slots = new List<NTreasureRoomRelicHolder>();
		for (int i = 0; i < cards.Count; i++)
		{
			NTreasureRoomRelicHolder val4;
			if (i < list.Count)
			{
				val4 = list[i];
			}
			else
			{
				val4 = (NTreasureRoomRelicHolder)((Node)val).Duplicate(15);
				GodotTreeExtensions.AddChildSafely((Node)(object)parent, (Node)(object)val4);
			}
			((CanvasItem)val4).Visible = true;
			NTreasureRoomRelicHolder val5 = val4;
			NTreasureRoomRelicHolder val6 = val4;
			NTreasureRoomRelicHolder val7 = val4;
			float num = (((Control)val4).AnchorBottom = 0f);
			float num3 = (((Control)val7).AnchorRight = num);
			float anchorLeft = (((Control)val6).AnchorTop = num3);
			((Control)val5).AnchorLeft = anchorLeft;
			((Control)val4).CustomMinimumSize = val2;
			((Control)val4).Size = val2;
			((Control)val4).PivotOffset = val2 * 0.5f;
			float num6 = 340f * ((float)i - (float)(cards.Count - 1) * 0.5f);
			((Control)val4).GlobalPosition = val3 + Vector2.Right * num6 - val2 * 0.5f;
			NMultiplayerVoteContainer voteContainer = val4.VoteContainer;
			num = (((Control)voteContainer).AnchorBottom = 0f);
			num3 = (((Control)voteContainer).AnchorRight = num);
			anchorLeft = (((Control)voteContainer).AnchorTop = num3);
			((Control)voteContainer).AnchorLeft = anchorLeft;
			float num10 = (val2.Y + NCard.defaultSize.Y) * 0.5f;
			((Control)voteContainer).Position = new Vector2((val2.X - ((Control)voteContainer).Size.X) * 0.5f, num10 + 12f);
			_slots.Add(val4);
		}
	}

	private void PlaceInChest()
	{
		IReadOnlyList<CardModel> cards = _room.Cards;
		List<NTreasureRoomRelicHolder> list = MultiplayerHolders.Invoke(_collection);
		NTreasureRoomRelicHolder singleplayerRelicHolder = _collection.SingleplayerRelicHolder;
		if (cards.Count == 1)
		{
			int num = 1;
			List<NTreasureRoomRelicHolder> list2 = new List<NTreasureRoomRelicHolder>(num);
			CollectionsMarshal.SetCount(list2, num);
			Span<NTreasureRoomRelicHolder> span = CollectionsMarshal.AsSpan(list2);
			int index = 0;
			span[index] = singleplayerRelicHolder;
			_slots = list2;
			((CanvasItem)singleplayerRelicHolder).Visible = true;
			{
				foreach (NTreasureRoomRelicHolder item in list)
				{
					((CanvasItem)item).Visible = false;
				}
				return;
			}
		}
		_slots = new List<NTreasureRoomRelicHolder>();
		((CanvasItem)singleplayerRelicHolder).Visible = false;
		for (int i = 0; i < list.Count; i++)
		{
			NTreasureRoomRelicHolder val = list[i];
			((CanvasItem)val).Visible = i < cards.Count;
			if (i < cards.Count)
			{
				_slots.Add(val);
			}
		}
		if (cards.Count == 2)
		{
			((Control)list[1]).Position = ((Control)list[3]).Position;
		}
	}

	private void HideSittingOutHands()
	{
		NHandImageCollection val = Hands.Invoke(_collection);
		foreach (Player player in ((IPlayerCollection)_runState).Players)
		{
			Creature creature = player.Creature;
			if (creature != null && creature.IsDead)
			{
				NHandImage hand = val.GetHand(player.NetId);
				if (hand != null)
				{
					((CanvasItem)hand).Visible = false;
				}
			}
		}
	}

	private void Dress(NTreasureRoomRelicHolder holder, CardModel card, int index)
	{
		holder.Index = index;
		((CanvasItem)holder.Relic).Visible = false;
		NCard val = NCard.Create(card, (ModelVisibility)1);
		if (val != null)
		{
			float num = ((Control)holder).Size.Y / NCard.defaultSize.Y;
			((Control)val).Scale = Vector2.One * num;
			((Control)val).MouseFilter = (MouseFilterEnum)2;
			GodotTreeExtensions.AddChildSafely((Node)(object)holder, (Node)(object)val);
			((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)8, (LayoutPresetMode)3, 0);
			val.UpdateVisuals((PileType)0, (CardPreviewMode)1);
			if (_room.Layout == CardPickLayout.ChooseACard)
			{
				val.ActivateRewardScreenGlow();
				GodotTreeExtensions.MoveChildSafely((Node)(object)holder, (Node)(object)holder.VoteContainer, -1);
			}
		}
		holder.VoteContainer.Initialize(((Player player) =>
		{
			CardPickingSession.PlayerVote vote = _room.Session.GetVote(player);
			return vote.VoteReceived && vote.Index == index;
		}), _room.Session.Participants);
		holder.VoteContainer.RefreshPlayerVotes(false);
		if (!_room.Session.LocalPlayerParticipates)
		{
			((NClickableControl)holder).Disable();
		}
		((NClickableControl)holder).Released += (NClickableControl _) =>
		{
			OnSlotPressed(index);
		};
	}

	private void OnSlotPressed(int index)
	{
		_room.Session.PickLocally(index);
	}

	public void RefreshVotes()
	{
		foreach (NTreasureRoomRelicHolder slot in _slots)
		{
			if (((Node)slot).IsNodeReady())
			{
				slot.VoteContainer.RefreshPlayerVotes(true);
			}
		}
	}

	public async Task PlayAwards(IReadOnlyList<CardPickingResult> results, bool final)
	{
		NHandImageCollection hands = Hands.Invoke(_collection);
		Control fightBackstop = FightBackstop.Invoke(_collection);
		foreach (NTreasureRoomRelicHolder slot2 in _slots)
		{
			((NClickableControl)slot2).Disable();
			((Control)slot2).SetFocusMode((FocusModeEnum)0);
		}
		HideSittingOutHands();
		if (final)
		{
			foreach (Player participant in _room.Session.Participants)
			{
				CardPickingSession.PlayerVote vote = _room.Session.GetVote(participant);
				if (_room.Session.AllowSkip && vote.VoteReceived && !vote.Index.HasValue)
				{
					NHandImage hand = hands.GetHand(participant.NetId);
					if (hand != null)
					{
						hand.SetSkipped();
					}
				}
			}
		}
		hands.BeforeRelicsAwarded();
		List<Task> grabs = new List<Task>();
		CardPickingResultType? cardPickingResultType = null;
		foreach (CardPickingResult result in results.OrderBy((CardPickingResult r) => (int)r.Type))
		{
			int num = IndexOf(result);
			if (num < 0 || num >= _slots.Count)
			{
				continue;
			}
			NTreasureRoomRelicHolder slot = _slots[num];
			slot.AnimateAwayVotes();
			if (cardPickingResultType.HasValue && result.Type != cardPickingResultType)
			{
				await Cmd.Wait(0.5f, false);
			}
			if (result.Type == CardPickingResultType.FoughtOver && result.Fight != null)
			{
				await PlayFight(result, slot, hands, fightBackstop);
			}
			else if (result.Type != CardPickingResultType.Skipped && result.Player != null)
			{
				NHandImage hand2 = hands.GetHand(result.Player.NetId);
				if (hand2 != null)
				{
					grabs.Add(TaskHelper.RunSafely(hand2.GrabRelic(slot)));
					await Cmd.Wait(0.25f, false);
				}
			}
			cardPickingResultType = result.Type;
		}
		await Task.WhenAll(grabs);
		if (!final)
		{
			ReopenForRepick(hands);
		}
		else if (grabs.Count == 0 && ((IPlayerCollection)_runState).Players.Count > 1)
		{
			await Cmd.Wait(0.7f, false);
		}
	}

	private void ReopenForRepick(NHandImageCollection hands)
	{
		CardPickingSession session = _room.Session;
		foreach (Player participant in session.Participants)
		{
			if (!session.IsVoting(participant))
			{
				continue;
			}
			NHandImage hand = hands.GetHand(participant.NetId);
			if (hand != null)
			{
				hand.SetIsInFight(false);
				hand.SetFrozenForRelicAwards(false);
				HandTextureRect.Invoke(hand).Texture = participant.Character.ArmPointingTexture;
				if (LocalContext.IsMe(participant))
				{
					((CanvasItem)hand).Modulate = Colors.White;
				}
				CardChestHandFingertipPatch.Apply(hand);
			}
		}
		if (!session.IsVoting(LocalContext.GetMe((IPlayerCollection)(object)_runState)))
		{
			return;
		}
		for (int i = 0; i < _slots.Count; i++)
		{
			if (session.IsAvailable(i))
			{
				((NClickableControl)_slots[i]).Enable();
				((Control)_slots[i]).SetFocusMode((FocusModeEnum)2);
			}
		}
	}

	private async Task PlayFight(CardPickingResult result, NTreasureRoomRelicHolder slot, NHandImageCollection hands, Control fightBackstop)
	{
		((CanvasItem)slot).ZIndex = 1;
		((CanvasItem)fightBackstop).Visible = true;
		Tween val = ((Node)_collection).CreateTween();
		val.TweenProperty((GodotObject)(object)slot, (NodePath)("global_position"), (Variant)((fightBackstop.Size - ((Control)slot).Size) * 0.5f), 0.25).SetTrans((TransitionType)10).SetEase((EaseType)0);
		val.TweenProperty((GodotObject)(object)fightBackstop, (NodePath)("modulate:a"), (Variant)(1f), 0.25);
		hands.BeforeFightStarted(result.Fight.playersInvolved);
		if (await TweenHelper.AwaitFinished(val, (Node)(object)_collection))
		{
			await Cmd.Wait(1f, false);
			await hands.DoFight(ToRelicResult(result), slot);
			Tween val2 = ((Node)_collection).CreateTween();
			val2.TweenProperty((GodotObject)(object)fightBackstop, (NodePath)("modulate:a"), (Variant)(0f), 0.25);
			await TweenHelper.AwaitFinished(val2, (Node)(object)_collection);
			((CanvasItem)fightBackstop).Visible = false;
			((CanvasItem)slot).ZIndex = 0;
		}
	}

	private int IndexOf(CardPickingResult result)
	{
		for (int i = 0; i < _room.Cards.Count; i++)
		{
			if (_room.Cards[i] == result.Card)
			{
				return i;
			}
		}
		return -1;
	}

	private static RelicPickingResult ToRelicResult(CardPickingResult result)
	{
		return new RelicPickingResult
		{
			type = (RelicPickingResultType)1,
			relic = null,
			player = result.Player,
			fight = result.Fight
		};
	}
}
