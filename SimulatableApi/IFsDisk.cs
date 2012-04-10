using JetBrains.Annotations;

namespace SimulatableApi
{
	internal interface IFsDisk
	{
		bool DirExists([NotNull] FSPath path);
		bool FileExists([NotNull] FSPath path);
		string TextContents([NotNull] FSPath path);
	    byte[] RawContents([NotNull] FSPath path);
	    void CreateDir([NotNull] FSPath path);
	    void Overwrite([NotNull] FSPath path, [NotNull] string newContents);
	    void DeleteDir([NotNull] FSPath path);
	    void DeleteFile([NotNull] FSPath path);
	    void MoveFile([NotNull] FSPath src, [NotNull] FSPath dest);
	}
}
