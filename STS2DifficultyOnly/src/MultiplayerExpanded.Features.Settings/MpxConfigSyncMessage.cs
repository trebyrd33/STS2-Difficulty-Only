using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MultiplayerExpanded.Features.Settings;

public struct MpxConfigSyncMessage : INetMessage, IPacketSerializable
{
	public string? json;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)0;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(json != null);
		if (json != null)
		{
			writer.WriteString(json);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		if (reader.ReadBool())
		{
			json = reader.ReadString();
		}
	}
}
