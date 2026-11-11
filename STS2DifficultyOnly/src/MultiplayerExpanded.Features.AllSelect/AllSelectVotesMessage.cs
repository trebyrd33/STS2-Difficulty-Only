using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerExpanded.Features.AllSelect;

internal sealed class AllSelectVotesMessage : ICustomTargetedMessage, IPacketSerializable
{
	private const int OptionBits = 5;

	public uint PageIndex;

	public uint?[] Votes = Array.Empty<uint?>();

	public bool[] StillPlaying = Array.Empty<bool>();

	public RunLocation Location { get; set; }

	public bool IsRewardMessage => false;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)0;

	public static AllSelectVotesMessage From(IReadOnlyList<uint?> votes, IReadOnlyList<bool> stillPlaying, uint pageIndex, RunLocation location)
	{
		int count = stillPlaying.Count;
		uint?[] array = new uint?[count];
		for (int i = 0; i < count; i++)
		{
			uint? num = ((i < votes.Count) ? votes[i] : ((uint?)null));
			array[i] = ((num == (uint?)uint.MaxValue) ? ((uint?)null) : num);
		}
		return new AllSelectVotesMessage
		{
			PageIndex = pageIndex,
			Votes = array,
			StillPlaying = stillPlaying.ToArray(),
			Location = location
		};
	}

	public void Serialize(PacketWriter writer)
	{
		writer.Write<RunLocation>(Location);
		writer.WriteUInt(PageIndex, 8);
		writer.WriteByte((byte)StillPlaying.Length, 8);
		for (int i = 0; i < StillPlaying.Length; i++)
		{
			writer.WriteBool(StillPlaying[i]);
			bool flag = i < Votes.Length && Votes[i].HasValue;
			writer.WriteBool(flag);
			if (flag)
			{
				writer.WriteUInt(Votes[i].Value, 5);
			}
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Location = reader.Read<RunLocation>();
		PageIndex = reader.ReadUInt(8);
		int num = reader.ReadByte(8);
		StillPlaying = new bool[num];
		Votes = new uint?[num];
		for (int i = 0; i < num; i++)
		{
			StillPlaying[i] = reader.ReadBool();
			Votes[i] = (reader.ReadBool() ? new uint?(reader.ReadUInt(5)) : ((uint?)null));
		}
	}

	public void HandleMessage(ulong senderId)
	{
		AllSelectSession.Active?.ReceiveVotes(this);
	}
}
