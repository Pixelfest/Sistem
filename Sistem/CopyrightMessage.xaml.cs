using System.Windows;

namespace Sistem
{
	/// <summary>
	/// Interaction logic for CopyrightMessage.xaml
	/// </summary>
	public partial class CopyrightMessage : Window
	{
		public string Message { get; set; }

		public CopyrightMessage(string message)
		{
			InitializeComponent();

			MessageTextBox.Text = message;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			Message = MessageTextBox.Text;
			this.Close();
		}
	}
}
