using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Features.BetaCompat;

public static class GameApi
{
	private delegate AttackCommand FromCardFn(AttackCommand c, CardModel card);

	private delegate AttackCommand FromOstyFn(AttackCommand c, Creature osty, CardModel card);

	private delegate Task<IEnumerable<DamageResult>> DamageFn(PlayerChoiceContext ctx, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource);

	private delegate decimal ModifyDamageFn(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer, decimal damage, ValueProp props, CardModel? cardSource, ModifyDamageHookType hookType, CardPreviewMode previewMode, out IEnumerable<AbstractModel> modifiers);

	private delegate Task LoseBlockFn(Creature creature, decimal amount);

	private delegate Task ExhaustFn(PlayerChoiceContext ctx, CardModel card, bool causedByEthereal, bool skipVisuals);

	private delegate Task OrbPassiveFn(PlayerChoiceContext ctx, OrbModel orb, Creature? target);

	private delegate Task SignalChoiceFn(PlayerChoiceContext ctx, PlayerChoiceOptions options);

	private delegate object? CustomPoolFn(CardCreationOptions options);

	private delegate CardCreationOptions WithPoolsFn(CardCreationOptions options, IEnumerable<CardPoolModel> pools, Func<CardModel, bool>? filter);

	private delegate uint SeedFn(RunRngSet rng);

	private delegate Rng NewRngFn(uint seed, string name);

	private delegate Rng NewRngNoNameFn(uint seed, int counter);

	private static FromCardFn? _fromCard;

	private static FromOstyFn? _fromOsty;

	private static DamageFn? _damage;

	private static ModifyDamageFn? _modifyDamage;

	private static LoseBlockFn? _loseBlock;

	private static ExhaustFn? _exhaust;

	private static OrbPassiveFn? _orbPassive;

	private static SignalChoiceFn? _signalChoice;

	private static CustomPoolFn? _customPool;

	private static WithPoolsFn? _withPools;

	private static SeedFn? _seed;

	private static NewRngFn? _newRng;

	private static NewRngNoNameFn? _newRngNoName;

	private static FieldInfo? _modAssembly;

	public static void Init()
	{
		if (!Beta.IsOn)
		{
			_fromCard = StableResolve.Method<FromCardFn>(typeof(AttackCommand), "FromCard", new Type[1] { typeof(CardModel) });
			_fromOsty = StableResolve.Method<FromOstyFn>(typeof(AttackCommand), "FromOsty", new Type[2]
			{
				typeof(Creature),
				typeof(CardModel)
			});
			_damage = StableResolve.Method<DamageFn>(typeof(CreatureCmd), "Damage", new Type[6]
			{
				typeof(PlayerChoiceContext),
				typeof(IEnumerable<Creature>),
				typeof(decimal),
				typeof(ValueProp),
				typeof(Creature),
				typeof(CardModel)
			});
			_modifyDamage = StableResolve.Method<ModifyDamageFn>(typeof(Hook), "ModifyDamage", new Type[10]
			{
				typeof(IRunState),
				typeof(ICombatState),
				typeof(Creature),
				typeof(Creature),
				typeof(decimal),
				typeof(ValueProp),
				typeof(CardModel),
				typeof(ModifyDamageHookType),
				typeof(CardPreviewMode),
				typeof(IEnumerable<AbstractModel>).MakeByRefType()
			});
			_loseBlock = StableResolve.Method<LoseBlockFn>(typeof(CreatureCmd), "LoseBlock", new Type[2]
			{
				typeof(Creature),
				typeof(decimal)
			});
			_exhaust = StableResolve.Method<ExhaustFn>(typeof(CardCmd), "Exhaust", new Type[4]
			{
				typeof(PlayerChoiceContext),
				typeof(CardModel),
				typeof(bool),
				typeof(bool)
			});
			_orbPassive = StableResolve.Method<OrbPassiveFn>(typeof(OrbCmd), "Passive", new Type[3]
			{
				typeof(PlayerChoiceContext),
				typeof(OrbModel),
				typeof(Creature)
			});
			_signalChoice = StableResolve.Method<SignalChoiceFn>(typeof(PlayerChoiceContext), "SignalPlayerChoiceBegun", new Type[1] { typeof(PlayerChoiceOptions) });
			_customPool = StableResolve.Getter<CustomPoolFn>(typeof(CardCreationOptions), "CustomCardPool");
			_withPools = StableResolve.Method<WithPoolsFn>(typeof(CardCreationOptions), "WithCardPools", new Type[2]
			{
				typeof(IEnumerable<CardPoolModel>),
				typeof(Func<CardModel, bool>)
			});
			_seed = StableResolve.Getter<SeedFn>(typeof(RunRngSet), "Seed");
			_newRng = StableResolve.Constructor<NewRngFn>(typeof(Rng), new Type[2]
			{
				typeof(uint),
				typeof(string)
			});
			_newRngNoName = StableResolve.Constructor<NewRngNoNameFn>(typeof(Rng), new Type[2]
			{
				typeof(uint),
				typeof(int)
			});
			_modAssembly = StableResolve.Field(typeof(Mod), "assembly");
		}
	}

	public static AttackCommand FromCardCompat(this AttackCommand command, CardModel card, CardPlay? cardPlay)
	{
		if (!Beta.IsOn)
		{
			return _fromCard(command, card);
		}
		return FromCardBeta(command, card, cardPlay);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static AttackCommand FromCardBeta(AttackCommand command, CardModel card, CardPlay? cardPlay)
	{
		return command.FromCard(card, cardPlay);
	}

	public static AttackCommand FromOstyCompat(this AttackCommand command, Creature osty, CardModel card, CardPlay? cardPlay)
	{
		if (!Beta.IsOn)
		{
			return _fromOsty(command, osty, card);
		}
		return FromOstyBeta(command, osty, card, cardPlay);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static AttackCommand FromOstyBeta(AttackCommand command, Creature osty, CardModel card, CardPlay? cardPlay)
	{
		return command.FromOsty(osty, card, cardPlay);
	}

	public static Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		return Damage(choiceContext, (IEnumerable<Creature>)new _003C_003Ez__ReadOnlySingleElementList<Creature>(target), amount, props, dealer, cardSource, cardPlay);
	}

	public static Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (!Beta.IsOn)
		{
			return _damage(choiceContext, targets, amount, props, dealer, cardSource);
		}
		return DamageBeta(choiceContext, targets, amount, props, dealer, cardSource, cardPlay);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Task<IEnumerable<DamageResult>> DamageBeta(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		return CreatureCmd.Damage(choiceContext, targets, amount, props, dealer, cardSource, cardPlay);
	}

	public static decimal ModifyDamage(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer, decimal damage, ValueProp props, CardModel? cardSource, CardPlay? cardPlay, ModifyDamageHookType hookType, CardPreviewMode previewMode, out IEnumerable<AbstractModel> modifiers)
	{
		if (!Beta.IsOn)
		{
			return _modifyDamage(runState, combatState, target, dealer, damage, props, cardSource, hookType, previewMode, out modifiers);
		}
		return ModifyDamageBeta(runState, combatState, target, dealer, damage, props, cardSource, cardPlay, hookType, previewMode, out modifiers);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static decimal ModifyDamageBeta(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer, decimal damage, ValueProp props, CardModel? cardSource, CardPlay? cardPlay, ModifyDamageHookType hookType, CardPreviewMode previewMode, out IEnumerable<AbstractModel> modifiers)
	{
		return Hook.ModifyDamage(runState, combatState, target, dealer, damage, props, cardSource, cardPlay, hookType, previewMode, out modifiers);
	}

	public static Task LoseBlock(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? remover)
	{
		if (!Beta.IsOn)
		{
			return _loseBlock(target, amount);
		}
		return LoseBlockBeta(choiceContext, target, amount, remover);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Task LoseBlockBeta(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? remover)
	{
		return CreatureCmd.LoseBlock(choiceContext, target, amount, remover);
	}

	public static Task Exhaust(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal = false, bool skipVisuals = false)
	{
		if (!Beta.IsOn)
		{
			return _exhaust(choiceContext, card, causedByEthereal, skipVisuals);
		}
		return ExhaustBeta(choiceContext, card, causedByEthereal, skipVisuals);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Task ExhaustBeta(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, bool skipVisuals)
	{
		return CardCmd.Exhaust(choiceContext, card, causedByEthereal, skipVisuals);
	}

	public static Task OrbPassive(PlayerChoiceContext choiceContext, OrbModel orb, Creature? target)
	{
		if (!Beta.IsOn)
		{
			return _orbPassive(choiceContext, orb, target);
		}
		return OrbPassiveBeta(choiceContext, orb, target);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Task OrbPassiveBeta(PlayerChoiceContext choiceContext, OrbModel orb, Creature? target)
	{
		return OrbCmd.Passive(choiceContext, orb, target, false);
	}

	public static Task SignalPlayerChoiceBegunAsOwner(this PlayerChoiceContext context, IReadOnlyList<Player> players, PlayerChoiceOptions options)
	{
		if (!Beta.IsOn)
		{
			return _signalChoice(context, options);
		}
		return SignalChoiceAsOwnerBeta(context, players, options);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Task SignalChoiceAsOwnerBeta(PlayerChoiceContext context, IReadOnlyList<Player> players, PlayerChoiceOptions options)
	{
		ulong? ownerId = context.OwnerId;
		Player val = null;
		foreach (Player player in players)
		{
			if (player.NetId == ownerId)
			{
				val = player;
				break;
			}
		}
		return context.SignalPlayerChoiceBegun(val ?? players[0], options);
	}

	public static Task RunBranched(AbstractModel source, PlayerChoiceContext context, Func<PlayerChoiceContext, Task> effect)
	{
		if (!Beta.IsOn || !LocalContext.NetId.HasValue)
		{
			return effect(context);
		}
		return RunBranchedBeta(source, context, effect);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Task RunBranchedBeta(AbstractModel source, PlayerChoiceContext context, Func<PlayerChoiceContext, Task> effect)
	{
		BranchingPlayerChoiceContext val = new BranchingPlayerChoiceContext(source, LocalContext.NetId.Value, (GameActionType)1, context);
		((PlayerChoiceContext)val).PushModel(source);
		return val.AssignTaskAndWaitForPauseOrCompletion(effect((PlayerChoiceContext)(object)val));
	}

	public static bool HasCustomCardPool(CardCreationOptions options)
	{
		if (!Beta.IsOn)
		{
			return _customPool(options) != null;
		}
		return false;
	}

	public static CardCreationOptions WithCardPools(CardCreationOptions options, IEnumerable<CardPoolModel> pools)
	{
		if (!Beta.IsOn)
		{
			return _withPools(options, pools, options.CardPoolFilter);
		}
		return WithCardPoolsBeta(options, pools);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static CardCreationOptions WithCardPoolsBeta(CardCreationOptions options, IEnumerable<CardPoolModel> pools)
	{
		return options.WithCardPools(pools);
	}

	public static ulong Seed(RunRngSet rng)
	{
		if (!Beta.IsOn)
		{
			return _seed(rng);
		}
		return SeedBeta(rng);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static ulong SeedBeta(RunRngSet rng)
	{
		return rng.Seed;
	}

	public static Rng NewRng(ulong seed, string name)
	{
		if (!Beta.IsOn)
		{
			return _newRng((uint)seed, name);
		}
		return NewRngBeta(seed, name);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Rng NewRngBeta(ulong seed, string name)
	{
		return new Rng(seed, name);
	}

	public static Rng NewRng(ulong seed)
	{
		if (!Beta.IsOn)
		{
			return _newRngNoName((uint)seed, 0);
		}
		return NewRngBeta(seed);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static Rng NewRngBeta(ulong seed)
	{
		return new Rng(seed);
	}

	public static IReadOnlyList<Assembly> ModAssemblies(Mod mod)
	{
		if (!Beta.IsOn)
		{
			if (!(_modAssembly?.GetValue(mod) is Assembly item))
			{
				return Array.Empty<Assembly>();
			}
			return new _003C_003Ez__ReadOnlySingleElementList<Assembly>(item);
		}
		return ModAssembliesBeta(mod);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[BetaPath]
	private static IReadOnlyList<Assembly> ModAssembliesBeta(Mod mod)
	{
		return mod.assemblies;
	}
}
