using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MetromTablet.Views
{
	/// <summary>
	/// Interaction logic for NumKeyboardPopup.xaml
	/// </summary>
	public partial class NumKeyboardPopup : Window, INotifyPropertyChanged
	{

		private string empID = LogInPage.logIn;
		private string confirmContent;
		private string cancelContent;
		private string title;
		private int maxLen;
		public bool confirmed;
		private static NumKeyboardPopup instance;
		private DispatcherTimer mIdle;
		private ICommand enterNumberCommand;
		private ICommand loginCommand;
		private ICommand deleteCommand;


		public NumKeyboardPopup()
		{
			InitializeComponent();
			this.Owner = MainWindow.mainWindow;
			DataContext = this;
			Title = "Title";
			MaxLen = 30;

			//user inactivity
			InputManager.Current.PreProcessInput += Idle_PreProcessInput;
			mIdle = new DispatcherTimer();
			mIdle.Interval = new TimeSpan(0, 0, 30);
			mIdle.IsEnabled = true;
			mIdle.Tick += Idle_Tick;
		}


		public static NumKeyboardPopup Instance
		{
			get
			{
				if (instance == null)
					instance = new NumKeyboardPopup();
				return instance;
			}
		}


		public string ID
		{
			get
			{
				return empID;
			}
			set
			{
				empID = value;
				NotifyPropertyChanged("ID");
			}
		}


		public string ConfirmContent
		{
			get
			{
				return confirmContent;
			}
			set
			{
				confirmContent = value;
				NotifyPropertyChanged("ConfirmContent");
			}
		}

		public string CancelContent
		{
			get
			{
				return cancelContent;
			}
			set
			{
				cancelContent = value;
				NotifyPropertyChanged("CancelContent");
			}
		}


		public string PopupTitle
		{
			get
			{
				return title;
			}
			set
			{
				title = value;
				NotifyPropertyChanged("PopupTitle");
			}
		}


		public int MaxLen
		{
			get
			{
				return maxLen;
			}
			set
			{
				maxLen = value;
				NotifyPropertyChanged("MaxLen");
			}
		}


		public ICommand EnterNumberCommand
		{
			get
			{
				if (enterNumberCommand == null)
				{
					enterNumberCommand = new RelayCommand((parameter) => EnterNumber(parameter));
				}
				return enterNumberCommand;
			}
		}


		public ICommand LoginCommand
		{
			get
			{
				if (loginCommand == null)
				{
					loginCommand = new RelayCommand(Login);
				}
				return loginCommand;
			}
		}


		public ICommand DeleteCommand
		{
			get
			{
				if (deleteCommand == null)
				{
					deleteCommand = new RelayCommand(Delete);
				}
				return deleteCommand;
			}
		}


		void Idle_Tick(object sender, EventArgs e)
		{
			this.Close();
		}


		void Idle_PreProcessInput(object sender, PreProcessInputEventArgs e)
		{
			mIdle.IsEnabled = false;
			mIdle.IsEnabled = true;
		}


		private void Delete()
		{
			if (CancelContent == "Del")
			{
				if (ID.Length > 0)
				{
					ID = ID.Remove(ID.Length - 1);
				}
			}
			else if (CancelContent == "Cancel")
			{
				ID = String.Empty;
				//this.Close();
			}
		}


		private void Login()
		{
			if (ConfirmContent == "OK")
			{
				confirmed = true;
				this.Close();
			}
			else if (ConfirmContent == "Unlock")
			{
				if (ID.Length == 3 && ID == MetromRailPage.cCode.ToString())
				{
					confirmed = true;
					//MetromRailPage.Unlock(2822);
					//MetromRailPage.numberOfMenuStates = 5;
					this.Close();
				}
				else if (ID.Length == 4 && ID == MetromRailPage.mCode.ToString())
				{
					confirmed = true;
					//MetromRailPage.Unlock(2822);
					//MetromRailPage.numberOfMenuStates = 7;
					this.Close();
				}
				else
				{
					confirmed = false;
					MessageBox.Show("Incorrect security code!");
				}
			}
		}


		private void EnterNumber(object number)
		{
			ID += number;
		}



		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//Thread.Sleep(50);
            mrFrame.Refresh();
        }


		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(string name)
		{
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
	}
}
