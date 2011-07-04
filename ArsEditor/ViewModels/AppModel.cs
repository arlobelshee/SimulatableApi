using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Input;
using EventBasedProgramming;
using Microsoft.Win32;
using SimulatableApi;

namespace ArsEditor.ViewModels
{
	public class AppModel : IFirePropertyChanged
	{
		protected readonly FileSystem FileSystem;
		private readonly TrackingOnlyInitiallyNullProperty<CharacterModel> _character;

		public AppModel(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
			OpenChar = new SimpleCommand(Always.Enabled, ChangeCurrentCharacter);
			_character = new TrackingOnlyInitiallyNullProperty<CharacterModel>(this, () => Character);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void FirePropertyChanged(Expression<Func<object>> propertyThatChanged)
		{
			PropertyChanged.Raise(this, propertyThatChanged);
		}

		public CharacterModel Character
		{
			get { return _character.Value; }
			set { _character.Value = value; }
		}

		public ICommand OpenChar { get; private set; }

		public void ChangeCurrentCharacter()
		{
			var dialog = new OpenFileDialog { Filter = "Ars Magica Character (*.ars)|*.ars", DefaultExt = "ars", CheckFileExists = false, Multiselect = false };
			if (Character != null)
				dialog.InitialDirectory = Character._File.ContainingFolder.Path.Absolute;
			if (dialog.ShowDialog() == true)
				SelectCharacter(FileSystem.File(dialog.FileName));
		}

		public void SelectCharacter(FsFile charFile)
		{
			Character = new CharacterModel(charFile);
		}
	}

	public class AppViewModelDesignData : AppModel
	{
		public AppViewModelDesignData() : base(FileSystem.Simulated()) {}
	}
}
