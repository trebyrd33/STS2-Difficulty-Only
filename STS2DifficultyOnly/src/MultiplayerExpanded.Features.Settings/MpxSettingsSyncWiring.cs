using System;
using BaseLib.Config;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MultiplayerExpanded.Features.Settings;

internal static class MpxSettingsSyncWiring
{
	private static INetGameService? _hostNet;
	private static INetClientGameService? _clientNet;

	private static bool _handlingHostChange;

	public static void AttachClient(INetGameService net)
	{
		_clientNet = net as INetClientGameService;
		net.UnregisterMessageHandler<MpxConfigSyncMessage>((MessageHandlerDelegate<MpxConfigSyncMessage>)OnConfigMessage);
		net.RegisterMessageHandler<MpxConfigSyncMessage>((MessageHandlerDelegate<MpxConfigSyncMessage>)OnConfigMessage);
		var config = ModConfigRegistry.Get(MainFile.ModId);
        if (config != null) { config.ConfigChanged -= OnAnyConfigChanged; config.ConfigChanged += OnAnyConfigChanged; }
        net.Disconnected -= OnNetDisconnected;
		net.Disconnected += OnNetDisconnected;
	}

	public static void AttachHost(INetGameService net)
	{
		_hostNet = net;
		var config = ModConfigRegistry.Get(MainFile.ModId);
        if (config != null) { config.ConfigChanged -= OnAnyConfigChanged; config.ConfigChanged += OnAnyConfigChanged; }
        net.Disconnected -= OnNetDisconnected;
		net.Disconnected += OnNetDisconnected;
		ModConfig val = ModConfigRegistry.Get("STS2DifficultyOnly");
		if (val != null)
		{
			val.ConfigChanged -= OnHostConfigChanged;
			val.ConfigChanged += OnHostConfigChanged;
		}
	}

	public static void SendSnapshotTo(INetHostGameService net, ulong peerId)
	{
		if (((INetGameService)net).IsConnected)
		{
			string text = MpxSettingsSync.Snapshot();
			if (text != null)
			{
				((INetGameService)net).SendMessage<MpxConfigSyncMessage>(new MpxConfigSyncMessage
				{
					json = text
				}, peerId);
			}
		}
	}

	private static void OnAnyConfigChanged(object? sender, EventArgs e) => MpxSettingsSync.EnforceCurrentSettings();

	private static void OnConfigMessage(MpxConfigSyncMessage message, ulong senderId)
	{
		if (_clientNet != null && senderId == _clientNet.NetClient.HostNetId)
			try { MpxSettingsSync.ApplyOverride(message.json); }
            catch (Exception ex) { MainFile.Logger.Error("[SettingsSync] Rejected host payload: " + ex.Message, 1); throw; }
	}

	private static void OnHostConfigChanged(object? sender, EventArgs e)
	{
		if (_handlingHostChange)
		{
			return;
		}
		_handlingHostChange = true;
		try
		{
			INetGameService hostNet = _hostNet;
			if (hostNet != null && hostNet.IsConnected)
			{
				MpxSettingsSync.EnforceCurrentSettings();
				string text = MpxSettingsSync.Snapshot();
				if (text != null)
				{
					hostNet.SendMessage<MpxConfigSyncMessage>(new MpxConfigSyncMessage
					{
						json = text
					});
				}
			}
			ModConfig val = ModConfigRegistry.Get("STS2DifficultyOnly");
			if (val != null)
			{
				val.SaveDebounced(1000);
				val.ConfigReloaded();
			}
		}
		catch (Exception ex)
		{
			MainFile.Logger.Warn("[SettingsSync] Host config re-broadcast failed: " + ex.Message, 1);
		}
		finally
		{
			_handlingHostChange = false;
		}
	}

	private static void OnNetDisconnected(NetErrorInfo info)
	{
		ModConfig val = ModConfigRegistry.Get("STS2DifficultyOnly");
		if (val != null)
		{
			val.ConfigChanged -= OnHostConfigChanged;
			val.ConfigChanged -= OnAnyConfigChanged;
		}
		_hostNet = null;
		_clientNet = null;
		MpxSettingsSync.Restore();
	}
}
