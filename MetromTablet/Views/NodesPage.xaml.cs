using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MetromTablet.Models;
using Newtonsoft.Json;

namespace MetromTablet.Views
{
	/// <summary>
	/// Interaction logic for NodesPage.xaml
	/// </summary>
	public partial class NodesPage : Page
	{
		#region Fields

		public ObservableCollection<Node> NodesList { get; set; }
		private Node selectedNode;
		private NodePin selectedInput;
		private DataGridCellInfo _cellInfo;
		private string inputInfo;
		public static string machineType;
		private const int maxNodesCount = 10;
		private ICommand addNodeCommand;
		private ICommand backCommand;
		private ICommand deleteCommand;
		private ICommand configInputCommand;

		#endregion Fields


		#region Properties

		public Node SelectedNode
		{
			get { return selectedNode; }
			set
			{
				selectedNode = value;
				NotifyPropertyChanged("SelectedNode");
			}
		}

		public DataGridCellInfo CellInfo
		{
			get { return _cellInfo; }
			set
			{
				_cellInfo = value;
				NotifyPropertyChanged("CellInfo");
				//MessageBox.Show(string.Format("Column: {0}",
				//	  _cellInfo.Column.DisplayIndex != null ? _cellInfo.Column.DisplayIndex.ToString() : "Index out of range!"));
				//SelectedNode = (Node)_cellInfo.Item;
			}
		}

		public string InputInfo
		{
			get { return inputInfo; }
			set
			{
				inputInfo = value;
				NotifyPropertyChanged("InputInfo");
			}
		}



		#endregion Properties


		#region Commands

		public ICommand AddNodeCommand
		{
			get
			{
				if (addNodeCommand == null)
				{
					addNodeCommand = new RelayCommand(AddNode);
				}
				return addNodeCommand;
			}
		}

		public ICommand BackCommand
		{
			get
			{
				if (backCommand == null)
				{
					backCommand = new RelayCommand(Back);
				}
				return backCommand;
			}
		}

		public ICommand DeleteCommand
		{
			get
			{
				if (deleteCommand == null)
				{
					deleteCommand = new RelayCommand(DeleteNode);
				}
				return deleteCommand;
			}
		}

		public ICommand ConfigInputCommand
		{
			get
			{
				if (configInputCommand == null)
				{
					configInputCommand = new RelayCommand(ConfigInput);
				}
				return configInputCommand;
			}
		}

		#endregion Commands


		#region Constructor

		public NodesPage()
		{
			InitializeComponent();
			this.DataContext = this;

			NodesList = new ObservableCollection<Node>();
			ConfigHCTREASJson();
			//NodesList.Add(new Node("node0", new NodePin[10] { new NodePin(),new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin() }));
			//NodesList.Add(new Node("node1", new NodePin[10] { new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin() }));
			
			InputInfo = "InputInfo";
		}

		#endregion Constructor

		

		#region Methods

		private void AddNode()
		{
			int nodesCount = NodesList.Count;
			if (nodesCount < maxNodesCount)
			{
				NodesList.Add(new Node("node" + nodesCount, new NodePin[10] { new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin(), new NodePin() }));
			}
		}


		private void DeleteNode()
		{
			if (SelectedNode != null)
			{
				NodesList.Remove(SelectedNode);
			}
		}

		private void Back(object obj)
		{
			this.NavigationService.GoBack();
		}


		public void ConfigHCTREASJson()
		{
			try
			{
                using (StreamReader r = new StreamReader(@"C:\METROM\hctreasNodes.json"))
				{
					string json = r.ReadToEnd();
					List<Node> items = JsonConvert.DeserializeObject<List<Node>>(json);
					foreach (Node n in items)
					{
						NodesList.Add(n);
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}


		private void ConfigInput()
		{
			SelectedNode = (Node)_cellInfo.Item;
			configPanel.Visibility = Visibility.Visible;
			InputInfo = DetectInput();
			lblInfo.Content = DetectInput();
			//MessageBox.Show(SelectedNode.Name + ", " + string.Format("IN{0}",
			//		  _cellInfo.Column.DisplayIndex != null ? _cellInfo.Column.DisplayIndex.ToString() : "Index out of range!") +
			//		  );
			//SelectedNode = (Node)_cellInfo.Item;
			//MessageBox.Show(SelectedNode.Name );
		}



		private string DetectInput()
		{
			switch(CellInfo.Column.DisplayIndex)
			{
				case 1:
					SelectedNode.In1 = ((Node)_cellInfo.Item).In1;
					return SelectedNode.Id + ": " + " INPUT 1, " + SelectedNode.In1.Id;
					//InputInfo = str;
					//MessageBox.Show(InputInfo);
					//break;
				case 2:
					SelectedNode.In2 = ((Node)_cellInfo.Item).In2;
					return SelectedNode.Id + ": " + " INPUT 2, " + SelectedNode.In2.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 3:
					SelectedNode.In3 = ((Node)_cellInfo.Item).In3;
					return SelectedNode.Id + ": " + " INPUT 3, " + SelectedNode.In3.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 4:
					SelectedNode.In4 = ((Node)_cellInfo.Item).In4;
					return SelectedNode.Id + ": " + " INPUT 4, " + SelectedNode.In4.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 5:
					SelectedNode.In5 = ((Node)_cellInfo.Item).In5;
					return SelectedNode.Id + ": " + " INPUT 5, " + SelectedNode.In5.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 6:
					SelectedNode.In6 = ((Node)_cellInfo.Item).In6;
					return SelectedNode.Id + ": " + " INPUT 6, " + SelectedNode.In6.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 7:
					SelectedNode.In7 = ((Node)_cellInfo.Item).In7;
					return SelectedNode.Id + ": " + " INPUT 7, " + SelectedNode.In7.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 8:
					SelectedNode.In8 = ((Node)_cellInfo.Item).In8;
					return SelectedNode.Id + ": " + " INPUT 8, " + SelectedNode.In8.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 9:
					SelectedNode.In9 = ((Node)_cellInfo.Item).In9;
					return SelectedNode.Id + ": " + " INPUT 9, " + SelectedNode.In9.Id;
					//MessageBox.Show(InputInfo);
					//break;
				case 10:
					SelectedNode.In0 = ((Node)_cellInfo.Item).In0;
					return SelectedNode.Id + ": " + " INPUT 10, " + SelectedNode.In0.Id;
					//MessageBox.Show(InputInfo);
					//break;
			}
			return SelectedNode.Id + ": " + " INPUT 1, " + SelectedNode.In1.Id;
		}

		#endregion Methods


		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			configPanel.Visibility = Visibility.Hidden;
		}
	}
}
