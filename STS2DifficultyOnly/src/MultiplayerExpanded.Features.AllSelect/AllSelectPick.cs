using MegaCrit.Sts2.Core.Entities.Players;

namespace MultiplayerExpanded.Features.AllSelect;

public readonly record struct AllSelectPick(Player Player, int SlotIndex, int OptionIndex);
