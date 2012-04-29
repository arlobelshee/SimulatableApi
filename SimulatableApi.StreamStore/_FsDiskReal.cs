using System.IO;

namespace Simulated
{
	internal class _FsDiskReal : IFsDisk
	{
		public bool DirExists(FsPath path)
		{
			return Directory.Exists(path.Absolute);
		}

		public bool FileExists(FsPath path)
		{
			return File.Exists(path.Absolute);
		}

		public string TextContents(FsPath path)
		{
			return File.ReadAllText(path.Absolute);
		}

	    public byte[] RawContents(FsPath path)
	    {
	        return File.ReadAllBytes(path.Absolute);
	    }

	    public void CreateDir(FsPath path)
		{
			Directory.CreateDirectory(path.Absolute);
		}

		public void Overwrite(FsPath path, string newContents)
		{
			File.WriteAllText(path.Absolute, newContents);
		}

		public void Overwrite(FsPath path, byte[] newContents)
		{
			File.WriteAllBytes(path.Absolute, newContents);
		}

		public void DeleteDir(FsPath path)
		{
			Directory.Delete(path.Absolute);
		}

		public void DeleteFile(FsPath path)
		{
			File.Delete(path.Absolute);
		}

		public void MoveFile(FsPath src, FsPath dest)
		{
			File.Move(src.Absolute, dest.Absolute);
		}
	}
}
