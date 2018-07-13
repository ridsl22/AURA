using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MetromTablet.Views
{
	/// <summary>
	/// Interaction logic for EnterDataPopup.xaml
	/// </summary>
	public partial class EnterDataPopup : Window
	{

		public string Data { get; set; }


		public EnterDataPopup()
		{
			InitializeComponent();
			Owner = MainWindow.mainWindow;
		}


		private void textBoxData_TextChanged(object sender, TextChangedEventArgs e)
		{
            if (Title.Equals("Enter Address"))
			{
				textBoxData.MaxLength = 6;
				string item = textBoxData.Text;
				string lastChar = String.Empty;
				if (item != String.Empty)
					lastChar= item.Substring(item.Length-1);
				Regex regexHexMatch = new Regex("[a-fA-F0-9]");

				if (!regexHexMatch.IsMatch(lastChar) && item != String.Empty)
				{
					textBoxData.Text = item.Remove(item.Length - 1, 1);
					textBoxData.SelectionStart = textBoxData.Text.Length;
				}
			}
            else if (Title.Equals("Enter Device Name"))
            {
                textBoxData.MaxLength = 11;
                string item = textBoxData.Text;
                string lastChar = String.Empty;
                if (item != String.Empty)
                    lastChar = item.Substring(item.Length - 1);
				//Regex regexPrefix = new Regex("[a-zA-Z]");//[&_-.#]");
				Regex regexPrefix = new Regex("[a-zA-Z&#_./-]");//[&_-.#]");
                Regex regexNumber = new Regex("[0-9]");

                //if (item.Length <= 2 && item != String.Empty)
				if (item.Length < 3 && item != String.Empty)
                {
                    if (!regexPrefix.IsMatch(lastChar) && item != String.Empty)
                    {
                        textBoxData.Text = item.Remove(item.Length - 1, 1);
                        textBoxData.SelectionStart = textBoxData.Text.Length;
                    }
                }
				else if (item.Length == 3 && item != String.Empty)
				{
					if (lastChar != " " && item != String.Empty)
					{
						textBoxData.Text = item.Remove(item.Length - 1, 1);
						textBoxData.SelectionStart = textBoxData.Text.Length;
					}
				}
				//else if (item.Length > 3 && item != String.Empty)
				else if (item.Length > 3 && item != String.Empty)
				{
					if (!regexNumber.IsMatch(lastChar) && item != String.Empty)
					{
						textBoxData.Text = item.Remove(item.Length - 1, 1);
						textBoxData.SelectionStart = textBoxData.Text.Length;
					}
				}
            }
		}


		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
            if (Title.Equals("Enter Address"))
            {         
                //if (textBoxData.Text.Length < 6)
				if (textBoxData.Text.Length < 10)
                {
					MessageBox.Show("Invalid Address\nPlease enter 6 hex characters");
                }
                else
                {
                    Data = textBoxData.Text;
                    Close();
                }
            }
            else if (Title.Equals("Enter Device Name"))
            {
                if (textBoxData.Text.Length < 11)
                {
					MessageBox.Show("Invalid Device Name\nAsset names can be \"CCCC NNNNNN\" \nwhere all \'C\' are any of: \n\"ABCDEFGHIJKLMNOPQRSTUVWXYZ&_-.#\" \nand NNNNNN is an up to 6 digit number.");
                }
                else
                {
                    Data = textBoxData.Text;
                    Close();
                }
            }
		}
	}
}
