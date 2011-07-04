using System;
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace EventBasedProgramming
{
	public static class PropertyChangedExtensions
	{
		public static void Raise(this PropertyChangedEventHandler handler, object sender,
			[NotNull] Expression<Func<object>> propertyExpression)
		{
			if (handler == null)
				return;
			var e = new PropertyChangedEventArgs(Extract.PropertyNameFrom(propertyExpression));
			handler(sender, e);
		}

		public static void PropagateFrom<TSource>(this IFirePropertyChanged sender,
			[NotNull] Expression<Func<object>> propertyExpression,
			[NotNull] TSource originator, [NotNull] Expression<Func<TSource, object>> whenThisFiresExpression)
			where TSource : INotifyPropertyChanged
		{
			var propertyToPropagate = Extract.PropertyNameFrom(whenThisFiresExpression);
			originator.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == propertyToPropagate)
					sender.FirePropertyChanged(propertyExpression);
			};
		}
	}
}
