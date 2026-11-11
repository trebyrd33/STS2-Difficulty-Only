using MegaCrit.Sts2.Core.Models;
using MultiplayerExpanded.Events;

namespace MultiplayerExpanded.Features.AllSelect;

public abstract class AllSelectEvent : MultiplayerExpandedEvent, IAllSelectEvent
{
	private AllSelectRound? _round;

	public sealed override bool IsShared => true;

	public virtual int MaxRounds => 10;

	protected AllSelectRound? Round
	{
		get
		{
			return _round;
		}
		private set
		{
			((AbstractModel)this).AssertMutable();
			_round = value;
		}
	}

	int IAllSelectEvent.MaxRounds => MaxRounds;

	protected abstract AllSelectContention ContentionFor(int optionIndex);

	AllSelectContention IAllSelectEvent.ContentionFor(int optionIndex)
	{
		return ContentionFor(optionIndex);
	}

	void IAllSelectEvent.ApplyRound(AllSelectRound round)
	{
		Round = round;
	}
}
