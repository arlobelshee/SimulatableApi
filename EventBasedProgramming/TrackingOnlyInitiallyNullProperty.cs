using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace EventBasedProgramming
{
	public class TrackingOnlyInitiallyNullProperty<TProperty> : IEquatable<TrackingOnlyInitiallyNullProperty<TProperty>> where TProperty : class
	{
		[NotNull] private readonly Expression<Func<object>>[] _enclosingProperties;
		[NotNull] private readonly IFirePropertyChanged _owner;
		private TProperty _value;

		public TrackingOnlyInitiallyNullProperty([NotNull] IFirePropertyChanged owner, [NotNull] params Expression<Func<object>>[] enclosingProperties)
		{
			_value = null;
			_owner = owner;
			_enclosingProperties = enclosingProperties;
		}

		public bool Equals(TrackingOnlyInitiallyNullProperty<TProperty> other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other._value, _value);
		}

		[NotNull]
		public TProperty Value
		{
			get { return _value; }
			set
			{
				if (!ReferenceEquals(_value, null) && _value.Equals(value))
					return;
				if (value == null)
					throw new ArgumentNullException("Value", "This property is not allowed to be null.");
				_value = value;
				foreach (var property in _enclosingProperties)
					_owner.FirePropertyChanged(property);
			}
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TrackingOnlyInitiallyNullProperty<TProperty>);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(TrackingOnlyInitiallyNullProperty<TProperty> left, TrackingOnlyInitiallyNullProperty<TProperty> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(TrackingOnlyInitiallyNullProperty<TProperty> left, TrackingOnlyInitiallyNullProperty<TProperty> right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return _value.ToString();
		}
	}
}
