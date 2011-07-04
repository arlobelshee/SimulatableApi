using System;
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace EventBasedProgramming
{
	public interface IFirePropertyChanged : INotifyPropertyChanged
	{
		void FirePropertyChanged([NotNull] Expression<Func<object>> propertyThatChanged);
	}
}