using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;

namespace EventBasedProgramming
{
	public static class Calls
	{
		public static CallToConstraint To([NotNull] Expression<Func<object>> call, string functionName = null)
		{
			return new CallToConstraint(Extract.BindingInfoFrom(call), functionName);
		}

		public static CallToConstraint To([NotNull] Expression<Action> call, string functionName = null)
		{
			return new CallToConstraint(Extract.BindingInfoFrom(call), functionName);
		}
	}

	public class CallToConstraint : Constraint
	{
		private readonly string _functionName;

		public CallToConstraint(BindingInfo call, string functionName):base(call)
		{
			_functionName = functionName;
			CorrectCall = call;
		}

		protected BindingInfo CorrectCall { get; set; }
		protected string Message { get; set; }

		public override bool Matches(object val)
		{
			Message = null;
			actual = val;
			var bindingInfo = val as BindingInfo;
			if (bindingInfo != null)
			{
				if (CorrectCall == bindingInfo)
					return true;
				Message = CorrectCall.Method.Equals(bindingInfo.Method) ? "bound to incorrect object instance." : "bound to incorrect method.";
				bindingInfo.BoundAs = _functionName;
				return false;
			}
			Message = "was not a BindingInfo instance.";
			return false;
		}

		public override void WriteDescriptionTo([NotNull] MessageWriter writer)
		{
			writer.WritePredicate("calling");
			writer.WriteExpectedValue(CorrectCall);
		}

		public override void WriteMessageTo([NotNull] MessageWriter writer)
		{
			writer.WriteMessageLine(Message ?? "bound to incorrect method.");
			base.WriteMessageTo(writer);
		}

		public Constraint Resolve()
		{
			return this;
		}
	}
}
