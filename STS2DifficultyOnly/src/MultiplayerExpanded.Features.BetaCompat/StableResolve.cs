using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;

namespace MultiplayerExpanded.Features.BetaCompat;

public static class StableResolve
{
	public static readonly List<string> Unresolved = new List<string>();

	public static T? Method<T>(Type type, string name, params Type[] parameters) where T : Delegate
	{
		MethodInfo methodInfo = AccessTools.Method(type, name, parameters, (Type[])null);
		if (methodInfo == null)
		{
			return Missing<T>($"{type.Name}.{name}({string.Join(", ", parameters.Select((Type p) => p.Name))})");
		}
		return Compile<T>(methodInfo, methodInfo.IsStatic ? null : type, from p in methodInfo.GetParameters()
			select p.ParameterType);
	}

	public static T? Constructor<T>(Type type, params Type[] parameters) where T : Delegate
	{
		ConstructorInfo constructorInfo = AccessTools.Constructor(type, parameters, false);
		if (constructorInfo == null)
		{
			return Missing<T>(type.Name + "..ctor(" + string.Join(", ", parameters.Select((Type p) => p.Name)) + ")");
		}
		ParameterExpression[] array = parameters.Select(Expression.Parameter).ToArray();
		Expression[] arguments = array;
		return Expression.Lambda<T>(Expression.New(constructorInfo, arguments), array).Compile();
	}

	public static T? Getter<T>(Type type, string property) where T : Delegate
	{
		MethodInfo methodInfo = AccessTools.PropertyGetter(type, property);
		if (methodInfo == null)
		{
			return Missing<T>(type.Name + "." + property);
		}
		return Compile<T>(methodInfo, type, Array.Empty<Type>());
	}

	private static T Compile<T>(MethodInfo mi, Type? instanceType, IEnumerable<Type> parameterTypes) where T : Delegate
	{
		MethodInfo method = typeof(T).GetMethod("Invoke");
		ParameterExpression[] delegateParams = (from p in method.GetParameters()
			select Expression.Parameter(p.ParameterType)).ToArray();
		int offset = ((!(instanceType == null)) ? 1 : 0);
		Expression[] arguments = parameterTypes.Select((Type t, int i) => Convert(delegateParams[i + offset], t)).ToArray();
		Expression expression = ((instanceType == null) ? Expression.Call(mi, arguments) : Expression.Call(Convert(delegateParams[0], instanceType), mi, arguments));
		if (method.ReturnType != typeof(void) && expression.Type != method.ReturnType)
		{
			expression = Expression.Convert(expression, method.ReturnType);
		}
		return Expression.Lambda<T>(expression, delegateParams).Compile();
	}

	private static Expression Convert(ParameterExpression p, Type to)
	{
		if (!to.IsByRef)
		{
			if (!(p.Type == to))
			{
				return Expression.Convert(p, to);
			}
			return p;
		}
		if (p.IsByRef && p.Type == to.GetElementType())
		{
			return p;
		}
		throw new ArgumentException($"by-ref parameter {to} needs a matching delegate parameter, got {p.Type}");
	}

	public static FieldInfo? Field(Type type, string name)
	{
		FieldInfo fieldInfo = AccessTools.Field(type, name);
		if (fieldInfo == null)
		{
			Missing<Delegate>(type.Name + "." + name);
		}
		return fieldInfo;
	}

	public static ConstructorInfo SoleConstructor(Type type)
	{
		return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Single();
	}

	private static T? Missing<T>(string what) where T : Delegate
	{
		Unresolved.Add(what);
		MainFile.Logger.Error("[BetaCompat] stable member not found: " + what, 1);
		return null;
	}
}
