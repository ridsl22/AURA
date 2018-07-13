using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;


namespace MetromTablet.Views
{
	/// <summary>
	/// Interaction logic for LogInPage.xaml
	/// </summary>
	public partial class LogInPage : Page, INotifyPropertyChanged
	{

		private const string registryKey = "Software\\MetromRail\\";
		public static string machineType;
		public static string logIn = String.Empty;
        private ObservableCollection<string> Machines;
        public static LogInPage Instance { get; }
        public ICommand LogInCommand { get; private set; }


        public string LogIn
		{
			get
			{
				return logIn;
			}
			set
			{
				logIn = value;
				NotifyPropertyChanged("LogIn");
			}
		}


		public LogInPage()
		{
			InitializeComponent();
			DataContext = this;
			Machines = new ObservableCollection<string>();
			Machines.Add("HCTREAS");
			Machines.Add("JUPITER");
			cbMachines.ItemsSource = Machines;
            machineType = Properties.Settings.Default.MachineType;
            switch (machineType)
            {
                case "HCTREAS":
                    cbMachines.SelectedIndex = 0;
                    break;
                case "JUPITER":
                    cbMachines.SelectedIndex = 1;
                    break;
                default:
                    cbMachines.SelectedIndex = 0;
                    break;
            }
            
			SetUser(registryKey);
			//SetPassword(registryKey);
			InitCommands();
		}


        private void InitCommands()
		{
			LogInCommand = new RelayCommand(Login);
		}


		//public string GetPassword()
		//{
		//	return pbPassword.Password;
		//}


		public void Login()
		{
			string empID = GetUserFromRegistry(registryKey);

            //if (empID.Equals(LogIn))
            //{
            MetromRailPage p = new MetromRailPage();
			this.NavigationService.Navigate(p);
			//}
			//else
			//{
			//	MessageBox.Show("User id is incorrect. Try again");
			//}
		}



		private static void SetUser(string regKey)
		{
			RegistryKey crKey = Registry.CurrentUser.CreateSubKey(regKey);
			if (crKey.GetValue("Employee ID") == null)
			{
				crKey.SetValue("Employee ID", "1111");
			}
			crKey.Close();
		}


		private static void SetPassword(string regKey)
		{
			RegistryKey crKey = Registry.CurrentUser.CreateSubKey(regKey);
			if (crKey.GetValue("Pswd") == null)
			{
				crKey.SetValue("Pswd", "mrpswd");
			}
			crKey.Close();
		}


		private static string GetUserFromRegistry(string regKey)
		{
			RegistryKey rKey = Registry.CurrentUser.OpenSubKey(regKey);
			string user = (string)rKey.GetValue("Employee ID");
			rKey.Close();
			return user;
		}


		private static string GetPasswordFromRegistry(string regKey)
		{
			RegistryKey rKey = Registry.CurrentUser.OpenSubKey(regKey);
			string pswd = (string)rKey.GetValue("Pswd");
			rKey.Close();
			return pswd;
		}


		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			NumKeyboardPopup np = new NumKeyboardPopup();
			np.Title = "Enter Employee ID";
			np.ConfirmContent = "OK";
			np.CancelContent = "Del";
			np.ShowDialog();
			LogIn = np.ID;
			btnLogIn.Focus();
		}


		private void cbMachines_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			machineType = cbMachines.SelectedValue.ToString();
            Properties.Settings.Default.MachineType = machineType;
            Properties.Settings.Default.Save();
		}


        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
