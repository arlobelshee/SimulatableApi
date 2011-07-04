using System.IO;

namespace SimulatableApi
{
	internal class _FsDiskReal : IFsDisk
	{
		public bool DirExists(FSPath path)
		{
			return Directory.Exists(path.Absolute);
		}

		public bool FileExists(FSPath path)
		{
			return File.Exists(path.Absolute);
		}

		public string TextContents(FSPath path)
		{
			return File.ReadAllText(path.Absolute);
		}

		public void CreateDir(FSPath path)
		{
			Directory.CreateDirectory(path.Absolute);
		}

		public void Overwrite(FSPath path, string newContents)
		{
			File.WriteAllText(path.Absolute, newContents);
		}

		public void DeleteDir(FSPath path)
		{
			Directory.Delete(path.Absolute);
		}

		public void DeleteFile(FSPath path)
		{
			File.Delete(path.Absolute);
		}

		public void MoveFile(FSPath src, FSPath dest)
		{
			File.Move(src.Absolute, dest.Absolute);
		}
	}
}
