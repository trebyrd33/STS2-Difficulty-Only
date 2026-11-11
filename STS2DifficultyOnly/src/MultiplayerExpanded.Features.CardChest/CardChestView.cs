using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MultiplayerExpanded.Features.BetaCompat;
using MultiplayerExpanded.Features.CardPicking;

namespace MultiplayerExpanded.Features.CardChest;

public sealed class CardChestView
{
	private static readonly AccessTools.FieldRef<NTreasureRoom, NTreasureRoomRelicCollection> RelicCollection = AccessTools.FieldRefAccess<NTreasureRoom, NTreasureRoomRelicCollection>("_relicCollection");

	private static readonly Color RowBackdrop = new Color(0f, 0f, 0f, 0.8f);

	private readonly CardChestRoom _room;

	private readonly NTreasureRoom _node;

	private readonly NTreasureRoomRelicCollection _collection;

	private bool _openedWithoutChest;

	private Task _awards = Task.CompletedTask;

	public CardChestDisplay Display => _room.Display;

	private NCommonBanner Banner => ((Node)_node).GetNode<NCommonBanner>((NodePath)("%Banner"));

	private Node ChestVisual => ((Node)_node).GetNode((NodePath)("%ChestVisual"));

	private CardChestView(CardChestRoom room, NTreasureRoom node, NTreasureRoomRelicCollection collection)
	{
		_room = room;
		_node = node;
		_collection = collection;
	}

	private void AnimCollection(StringName method)
	{
		if (Beta.IsOn)
		{
			((GodotObject)_collection).Call(method, Array.Empty<Variant>());
			return;
		}
		((GodotObject)_collection).Call(method, new Variant[1] { (Variant)((GodotObject)(object)ChestVisual) });
	}

	public static async Task<CardChestView?> Show(CardChestRoom room, IRunState runState, LocString prompt)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		object obj;
		if (!CombatManager.Instance.IsInProgress)
		{
			NRun instance = NRun.Instance;
			obj = ((instance != null) ? instance.EventRoom : null);
			if (obj == null)
			{
				NRun instance2 = NRun.Instance;
				obj = ((instance2 != null) ? instance2.TreasureRoom : null);
			}
		}
		else
		{
			obj = NCombatRoom.Instance;
		}
		Control parent = (Control)obj;
		if (parent == null)
		{
			return null;
		}
		await PreloadManager.LoadRoomTreasureAssets(runState.Act);
		NTreasureRoom val = NTreasureRoom.Create((TreasureRoom)(object)room, runState);
		if (val == null)
		{
			return null;
		}
		GodotTreeExtensions.AddChildSafely((Node)(object)parent, (Node)(object)val);
		NTreasureRoomRelicCollection collection = RelicCollection.Invoke(val);
		CardChestView cardChestView = new CardChestView(room, val, collection);
		cardChestView.Banner.label.SetTextAutoSize(prompt.GetRawText());
		room.Display = new CardChestDisplay(collection, room, runState);
		room.Session.VotesChanged += room.Display.RefreshVotes;
		room.Session.RoundResolved += cardChestView.QueueAwards;
		if (room.Layout == CardPickLayout.ChooseACard)
		{
			cardChestView.OpenWithoutChest();
		}
		foreach (var (results, final) in room.Session.ResolvedRounds)
		{
			cardChestView.QueueAwards(results, final);
		}
		return cardChestView;
	}

	private void QueueAwards(IReadOnlyList<CardPickingResult> results, bool final)
	{
		_awards = PlayAfter(_awards, results, final);
	}

	private async Task PlayAfter(Task previous, IReadOnlyList<CardPickingResult> results, bool final)
	{
		await previous;
		await Display.PlayAwards(results, final);
	}

	private void OpenWithoutChest()
	{
		((CanvasItem)((Node)_node).GetNode<Control>((NodePath)("%Chest"))).Visible = false;
		NProceedButton node = ((Node)_node).GetNode<NProceedButton>((NodePath)("%ProceedButton"));
		((NClickableControl)node).Disable();
		((CanvasItem)node).Visible = false;
		((Node)_node).GetNode<ColorRect>((NodePath)("ColorRect")).Color = RowBackdrop;
		Banner.AnimateIn();
		_collection.InitializeRelics();
		AnimCollection(NTreasureRoomRelicCollection.MethodName.AnimIn);
		_openedWithoutChest = true;
	}

	public async Task<IReadOnlyList<CardPickingResult>> Run()
	{
		IReadOnlyList<CardPickingResult> results = await _room.Session.Completion;
		_room.PickingBegan.TrySetResult();
		await _awards;
		return results;
	}

	public async Task Close()
	{
		_room.Session.VotesChanged -= Display.RefreshVotes;
		_room.Session.RoundResolved -= QueueAwards;
		_room.PickingFinished.TrySetResult();
		if (_openedWithoutChest)
		{
			Banner.AnimateOut();
			AnimCollection(NTreasureRoomRelicCollection.MethodName.AnimOut);
		}
		await Cmd.Wait(0.5f, false);
		Discard();
	}

	public void Discard()
	{
		if (_openedWithoutChest && GodotObject.IsInstanceValid((GodotObject)(object)_node))
		{
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				instance.ScreenStateTracker.SetIsInSharedRelicPickingScreen(false);
			}
		}
		_openedWithoutChest = false;
		GodotTreeExtensions.QueueFreeSafely((Node)(object)_node);
		if (CardChestRoom.Active == _room)
		{
			CardChestRoom.Active = null;
		}
	}

	static CardChestView()
	{
	}
}
