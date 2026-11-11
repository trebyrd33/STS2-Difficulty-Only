using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MultiplayerExpanded.Features.CardPicking;

public struct NetPickCardAction : INetAction, IPacketSerializable
{
	public int? cardIndex;

	public int round;

	public GameAction ToGameAction(Player player)
	{
		return (GameAction)(object)new PickCardAction(player, cardIndex, round);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(cardIndex.HasValue);
		if (cardIndex.HasValue)
		{
			writer.WriteInt(cardIndex.Value, 8);
		}
		writer.WriteInt(round, 8);
	}

	public void Deserialize(PacketReader reader)
	{
		if (reader.ReadBool())
		{
			cardIndex = reader.ReadInt(8);
		}
		round = reader.ReadInt(8);
	}

	public override string ToString()
	{
		return $"{"NetPickCardAction"} index: {cardIndex} round: {round}";
	}
}
