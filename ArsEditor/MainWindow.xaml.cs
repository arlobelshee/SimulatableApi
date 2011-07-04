using System.Windows;
using ArsEditor.ViewModels;
using SimulatableApi;

namespace ArsEditor
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void _EnsureViewModelIsSet(object sender, RoutedEventArgs e)
		{
			if (DataContext == null)
				DataContext = new AppModel(FileSystem.Real());
		}
	}
}
