using System;
using MultiplayerExpanded.Cards;

namespace MultiplayerExpanded.Features.Reworks;

public sealed record ReworkInfo(Type BaseType, CardRework Rework, string LocKey);
