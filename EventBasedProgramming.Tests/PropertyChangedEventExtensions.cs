using System;
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace EventBasedProgramming.Tests
{
	[TestFixture]
	public
		class PropertyChangedEventExtensions
	{
		[SetUp]
		public void Setup()
		{
			_testSubject = new _ObjWithPropertyChangeNotification();
			_propThatChanged = "was never called";
		}

		[Test]
		public void PropertyChangedRaises()
		{
			_testSubject.PropertyChanged += (s, evt) => _propThatChanged = evt.PropertyName;
			_ListenTo(_testSubject);
			_testSubject.NotifyDescriptionChanged();
			Assert.That(_propThatChanged, Is.EqualTo("Description"));
		}

		[Test]
		public void PropagatePropertyChanged()
		{
			var listener = new _ObjWithPropagation(_testSubject);
			_ListenTo(listener);
			_testSubject.NotifyDescriptionChanged();
			Assert.That(_propThatChanged, Is.EqualTo("DependsOnDescription"));
		}

		private void _ListenTo([NotNull] INotifyPropertyChanged source)
		{
			source.PropertyChanged += (s, evt) => _propThatChanged = evt.PropertyName;
		}

		private class _ObjWithPropagation : IFirePropertyChanged
		{
			private readonly _ObjWithPropertyChangeNotification _source;

			public _ObjWithPropagation([NotNull] _ObjWithPropertyChangeNotification source)
			{
				_source = source;
				this.PropagateFrom(() => DependsOnDescription, source, src => src.Description);
			}

			public void FirePropertyChanged(Expression<Func<object>> propertyThatChanged)
			{
				PropertyChanged.Raise(this, propertyThatChanged);
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public string DependsOnDescription
			{
				get { return _source.Description; }
			}
		}

		private class _ObjWithPropertyChangeNotification : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public string Description
			{
				get { return "irrelevant"; }
			}

			public void NotifyDescriptionChanged()
			{
				PropertyChanged.Raise(this, () => Description);
			}
		}

		[NotNull] private string _propThatChanged;
		[NotNull] private _ObjWithPropertyChangeNotification _testSubject;
	}
}
