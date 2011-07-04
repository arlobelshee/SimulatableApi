using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace EventBasedProgramming.Tests
{
	[TestFixture]
	public class TrackingNonNullProperty
	{
		[SetUp]
		public void Setup()
		{
			_testSubject = new _Notifier();
			_changedProperties = new List<string>();
			_testSubject.PropertyChanged += (s, evt) => _changedProperties.Add(evt.PropertyName);
		}

		[Test]
		public void DisallowsBeingSetToNull()
		{
			Assert.Throws<ArgumentNullException>(() => _testSubject.Name = null);
		}

		[Test]
		public void FiresPropertyChangedForAllDependantProperties()
		{
			_testSubject.Name = "irrelevant";
			Assert.That(_changedProperties,
				Is.EquivalentTo(new[] {Extract.PropertyNameFrom(() => _testSubject.Name), Extract.PropertyNameFrom(() => _testSubject.FullName)}));
		}

		private class _Notifier : IFirePropertyChanged
		{
			[NotNull] private readonly TrackingNonNullProperty<string> _name;

			public _Notifier()
			{
				_name = new TrackingNonNullProperty<string>(string.Empty, this, () => Name, () => FullName);
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public void FirePropertyChanged(Expression<Func<object>> propertyThatChanged)
			{
				PropertyChanged.Raise(this, propertyThatChanged);
			}

			[NotNull]
			public string Name
			{
				get { return _name.Value; }
				set { _name.Value = value; }
			}

			[NotNull]
			public string FullName
			{
				get { return _name.Value; }
			}
		}

		private List<string> _changedProperties;
		private _Notifier _testSubject;
	}
}
