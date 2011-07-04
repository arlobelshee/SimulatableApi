using System;
using System.Reflection;

namespace EventBasedProgramming
{
	public class BindingInfo : IEquatable<BindingInfo>
	{
		public BindingInfo(MethodInfo method, object target)
		{
			Method = method;
			Target = target;
		}

		public MethodInfo Method { get; set; }
		public object Target { get; set; }
		public string BoundAs { get; set; }

		public bool Equals(BindingInfo other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other.Method, Method) && Equals(other.Target, Target);
		}

		public override string ToString()
		{
			var boundTo = string.IsNullOrEmpty(BoundAs) ? string.Empty : BoundAs + " bound to ";
			return Target == null
				? string.Format("{1}static method {0}", Method, boundTo)
				: string.Format("{2}method {0} on target {1}", Method, Target, boundTo);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as BindingInfo);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Method.GetHashCode() * 397) ^ (Target != null ? Target.GetHashCode() : 0);
			}
		}

		public static bool operator ==(BindingInfo left, BindingInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(BindingInfo left, BindingInfo right)
		{
			return !left.Equals(right);
		}
	}
}
