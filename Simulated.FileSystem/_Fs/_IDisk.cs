using System.Collections.Generic;
using JetBrains.Annotations;

namespace Simulated._Fs
{
	internal interface _IFsDisk
	{
		bool DirExists([NotNull] FsPath path);
		bool FileExists([NotNull] FsPath path);
		string TextContents([NotNull] FsPath path);
		byte[] RawContents([NotNull] FsPath path);
		void CreateDir([NotNull] FsPath path);
		void Overwrite([NotNull] FsPath path, [NotNull] string newContents);
		void Overwrite([NotNull] FsPath path, [NotNull] byte[] newContents);
		void DeleteDir([NotNull] FsPath path);
		void DeleteFile([NotNull] FsPath path);
		void MoveFile([NotNull] FsPath src, [NotNull] FsPath dest);
		void MoveDir(FsPath src, FsPath dest);
		IEnumerable<FsPath> FindFiles([NotNull] FsPath path, [NotNull] string searchPattern);
	}
}
