using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerExpanded.Features.BetaCompat;

[StableOnly]
[HarmonyPatch(typeof(Hook), "ModifyDamageInternal")]
internal static class StableDamageHookForwarder
{
	private delegate decimal AmountFn(AbstractModel m, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource);

	private delegate decimal CapFn(AbstractModel m, Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource);

	private delegate decimal ModAmountFn(AbstractModel m, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay);

	private delegate decimal ModCapFn(AbstractModel m, Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay);

	private static readonly Assembly ModAssembly = typeof(StableDamageHookForwarder).Assembly;

	private static readonly Type[] StableAmountParams = new Type[5]
	{
		typeof(Creature),
		typeof(decimal),
		typeof(ValueProp),
		typeof(Creature),
		typeof(CardModel)
	};

	private static readonly Type[] StableCapParams = new Type[4]
	{
		typeof(Creature),
		typeof(ValueProp),
		typeof(Creature),
		typeof(CardModel)
	};

	private static AmountFn? _vanillaAdditive;

	private static AmountFn? _vanillaMultiplicative;

	private static CapFn? _vanillaCap;

	private static readonly ConcurrentDictionary<Type, ModAmountFn?> ModAdditive = new ConcurrentDictionary<Type, ModAmountFn>();

	private static readonly ConcurrentDictionary<Type, ModAmountFn?> ModMultiplicative = new ConcurrentDictionary<Type, ModAmountFn>();

	private static readonly ConcurrentDictionary<Type, ModCapFn?> ModCap = new ConcurrentDictionary<Type, ModCapFn>();

	private static bool Prepare()
	{
		_vanillaAdditive = Compile<AmountFn>("ModifyDamageAdditive", StableAmountParams);
		_vanillaMultiplicative = Compile<AmountFn>("ModifyDamageMultiplicative", StableAmountParams);
		_vanillaCap = Compile<CapFn>("ModifyDamageCap", StableCapParams);
		if (_vanillaAdditive != null && _vanillaMultiplicative != null && _vanillaCap != null)
		{
			return true;
		}
		MainFile.Logger.Error("[BetaCompat] stable AbstractModel.ModifyDamage* not found; mod damage modifiers inactive", 1);
		return false;
	}

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		int replaced = 0;
		foreach (CodeInstruction instruction in instructions)
		{
			if (instruction.opcode == OpCodes.Callvirt && instruction.operand is MethodInfo methodInfo)
			{
				Type declaringType = methodInfo.DeclaringType;
				if (declaringType == typeof(AbstractModel))
				{
					string text = methodInfo.Name switch
					{
						"ModifyDamageAdditive" => "Additive", 
						"ModifyDamageMultiplicative" => "Multiplicative", 
						"ModifyDamageCap" => "Cap", 
						_ => null, 
					};
					if (text != null)
					{
						instruction.opcode = OpCodes.Call;
						instruction.operand = AccessTools.Method(typeof(StableDamageHookForwarder), text, (Type[])null, (Type[])null);
						replaced++;
					}
				}
			}
			yield return instruction;
		}
		if (replaced != 3)
		{
			MainFile.Logger.Error($"[BetaCompat] ModifyDamageInternal: rewrote {replaced} of 3 damage hook calls", 1);
		}
	}

	private static decimal Additive(AbstractModel m, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return ModMethod(ModAdditive, m, "ModifyDamageAdditive")?.Invoke(m, target, amount, props, dealer, cardSource, null) ?? _vanillaAdditive(m, target, amount, props, dealer, cardSource);
	}

	private static decimal Multiplicative(AbstractModel m, Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return ModMethod(ModMultiplicative, m, "ModifyDamageMultiplicative")?.Invoke(m, target, amount, props, dealer, cardSource, null) ?? _vanillaMultiplicative(m, target, amount, props, dealer, cardSource);
	}

	private static decimal Cap(AbstractModel m, Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		return ModMethod(ModCap, m, "ModifyDamageCap")?.Invoke(m, target, props, dealer, cardSource, null) ?? _vanillaCap(m, target, props, dealer, cardSource);
	}

	private static T? ModMethod<T>(ConcurrentDictionary<Type, T?> cache, AbstractModel m, string name) where T : Delegate
	{
		Type type = ((object)m).GetType();
		if (type.Assembly != ModAssembly)
		{
			return null;
		}
		return cache.GetOrAdd(type, (Type t) =>
		{
			Type[] array = ((name == "ModifyDamageCap") ? StableCapParams : StableAmountParams);
			string name2 = name;
			Type[] array2 = array;
			int num = 0;
			Type[] array3 = new Type[1 + array2.Length];
			ReadOnlySpan<Type> readOnlySpan = new ReadOnlySpan<Type>(array2);
			readOnlySpan.CopyTo(new Span<Type>(array3).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			array3[num] = typeof(CardPlay);
			MethodInfo method = t.GetMethod(name2, BindingFlags.Instance | BindingFlags.Public, null, array3, null);
			return (!(method != null) || !(method.DeclaringType.Assembly == ModAssembly)) ? null : Compile<T>(method);
		});
	}

	private static T? Compile<T>(string name, Type[] parameters) where T : Delegate
	{
		MethodInfo methodInfo = AccessTools.Method(typeof(AbstractModel), name, parameters, (Type[])null);
		if ((object)methodInfo == null)
		{
			return null;
		}
		return Compile<T>(methodInfo);
	}

	private static T Compile<T>(MethodInfo mi) where T : Delegate
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(AbstractModel));
		ParameterExpression[] array = (from p in mi.GetParameters()
			select Expression.Parameter(p.ParameterType)).ToArray();
		UnaryExpression instance = Expression.Convert(parameterExpression, mi.DeclaringType);
		Expression[] arguments = array;
		MethodCallExpression body = Expression.Call(instance, mi, arguments);
		ParameterExpression parameterExpression2 = parameterExpression;
		ParameterExpression[] array2 = array;
		int num = 0;
		ParameterExpression[] array3 = new ParameterExpression[1 + array2.Length];
		array3[num] = parameterExpression2;
		num++;
		ReadOnlySpan<ParameterExpression> readOnlySpan = new ReadOnlySpan<ParameterExpression>(array2);
		readOnlySpan.CopyTo(new Span<ParameterExpression>(array3).Slice(num, readOnlySpan.Length));
		num += readOnlySpan.Length;
		return Expression.Lambda<T>(body, array3).Compile();
	}
}
