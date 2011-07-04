using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using ArsEditor.ViewModels;
using EventBasedProgramming;
using NUnit.Framework;
using SimulatableApi;

namespace ArsEditor.Tests
{
	[TestFixture]
	public class AppModelHandlesSerialization
	{
		[SetUp]
		public void Setup()
		{
			var fileSystem = FileSystem.Simulated();
			_charFile = fileSystem.Directory(@"C:\The\Folder").File("CharName.ars");
			_charFile.Overwrite(FakeCharData);
			_testSubject = new AppModel(fileSystem);
		}

		[Test]
		public void ModelInitiallyHasNoCharacter()
		{
			Assert.That(_testSubject.Character, Is.Null);
		}

		[Test]
		public void ModelExposesACommandForOpeningACharacter()
		{
			Assert.That(_testSubject.OpenChar, Command.DelegatesTo(() => Always.Enabled(), () => _testSubject.ChangeCurrentCharacter()));
		}

		[Test]
		public void SelectingACharacterParsesIt()
		{
			string prop = null;
			_testSubject.PropertyChanged += (_, changedProperty) => prop = changedProperty.PropertyName;
			//var results = new List<string>();
			//var changes = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(f => _testSubject.PropertyChanged += f,
				//f => _testSubject.PropertyChanged -= f);
			//changes.Select(e => e.PropertyName).ForEach(p => results.Add(p));
			_testSubject.SelectCharacter(_charFile);
			Assert.That(prop, Is.EqualTo(Extract.PropertyNameFrom(() => _testSubject.Character)));
			//Assert.That(results, Is.EqualTo(new[] {Extract.PropertyNameFrom(() => _testSubject.Character)}));
			Assert.That(_testSubject.Character._File, Is.EqualTo(_charFile));
		}

		private const string FakeCharData = "name:value";
		private FsFile _charFile;
		private AppModel _testSubject;
	}
}
