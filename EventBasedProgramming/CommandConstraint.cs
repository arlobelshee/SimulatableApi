using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EventBasedProgramming
{
	public static class Command
	{
		public static ResolvableConstraintExpression DelegatesTo([NotNull] Expression<Func<bool>> enabled, [NotNull] Expression<Action> call)
		{
			return Is.InstanceOf<SimpleCommand>().And.Property("MethodHandlingCanExecute").Calls(enabled, "CanExecute").And.Property("MethodHandlingExecute").Calls(call, "Execute");
		}

		public static ResolvableConstraintExpression Calls<T>([NotNull] this ResolvableConstraintExpression expression, [NotNull] Expression<Func<T>> call, string functionName = null)
		{
			expression.Append(new CallToConstraint(Extract.BindingInfoFrom(call), functionName));
			return expression;
		}

		public static ResolvableConstraintExpression Calls([NotNull] this ResolvableConstraintExpression expression, [NotNull] Expression<Action> call, string functionName = null)
		{
			expression.Append(new CallToConstraint(Extract.BindingInfoFrom(call), functionName));
			return expression;
		}
	}
}
