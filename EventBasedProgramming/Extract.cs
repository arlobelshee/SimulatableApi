using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace EventBasedProgramming
{
	public static class Extract
	{
		private const string MethodCall = "call to the appropriate method";
		private const string PropertyAccess = "access to the appropriate property";
		private const BindingFlags Anything = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		[NotNull]
		public static string PropertyNameFrom([NotNull] Expression<Func<object>> propertyExpression)
		{
			return _ExtractPropertyName(_As<MemberExpression>(propertyExpression.Body, PropertyAccess));
		}

		[NotNull]
		public static string PropertyNameFrom<T>([NotNull] Expression<Func<T, object>> propertyExpression)
		{
			return _ExtractPropertyName(_As<MemberExpression>(propertyExpression.Body, PropertyAccess));
		}

		[NotNull]
		public static BindingInfo BindingInfoFrom<T>([NotNull] Expression<Func<T>> methodCallExpression)
		{
			return _ExtractBindingInfo(_As<MethodCallExpression>(methodCallExpression.Body, MethodCall), MethodCall);
		}

		[NotNull]
		public static BindingInfo BindingInfoFrom([NotNull] Expression<Action> methodCallExpression)
		{
			return _ExtractBindingInfo(_As<MethodCallExpression>(methodCallExpression.Body, MethodCall), MethodCall);
		}

		[NotNull]
		private static BindingInfo _ExtractBindingInfo([NotNull] MethodCallExpression call, [NotNull] string expectedType)
		{
			return new BindingInfo(call.Method, _ExtractValueInstance(call.Object, expectedType));
		}

		private static object _ExtractValueInstance(Expression val, [NotNull] string expectedType)
		{
			if (val == null)
				return null; // static method call.
			if (val is MemberExpression)
			{
				var closure = _ExtractConstant((val as MemberExpression).Expression, expectedType);
				var field = (val as MemberExpression).Member;
				return field.ReflectedType.GetField(field.Name, Anything).GetValue(closure);
			}
			return _ExtractConstant(val, expectedType);
		}

		private static object _ExtractConstant([NotNull] Expression val, [NotNull] string expectedType)
		{
			if (val is ConstantExpression)
				return (val as ConstantExpression).Value;
			throw _TooComplicated(expectedType);
		}

		[NotNull]
		private static string _ExtractPropertyName([NotNull] MemberExpression body)
		{
			return body.Member.Name;
		}

		[NotNull]
		private static T _As<T>([NotNull] Expression expressionBody, string expectedType) where T : Expression
		{
			var body = _StripConversionNode<T>(expressionBody) ?? expressionBody as T;
			if (body == null)
				throw _TooComplicated(expectedType);
			return body;
		}

		private static T _StripConversionNode<T>([NotNull] Expression expressionBody) where T : Expression
		{
			return (expressionBody.NodeType == ExpressionType.Convert) ? ((UnaryExpression) expressionBody).Operand as T : null;
		}

		private static ArgumentException _TooComplicated(string expectedType)
		{
			return new ArgumentException(string.Format("The expression body should be a simple direct {0}. Please simplify your lambda.", expectedType));
		}
	}
}
