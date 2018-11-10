using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
