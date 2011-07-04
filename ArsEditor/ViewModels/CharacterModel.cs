using System.ComponentModel;
using SimulatableApi;

namespace ArsEditor.ViewModels
{
	public class CharacterModel : INotifyPropertyChanged
	{
		public CharacterModel(FsFile charFile)
		{
			_File = charFile;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public FsFile _File { get; set; }
	}

	public class CharacterModelDesignData : CharacterModel
	{
		public CharacterModelDesignData() : base(DefaultCharacterFile) {}

		protected static FsFile DefaultCharacterFile
		{
			get
			{
				var charFile = FileSystem.Simulated().TempDirectory.File("ModelCharacter.ars");
				charFile.Overwrite(@"");
				return charFile;
			}
		}
	}
}
