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

namespace MetromTablet
{

	/// <summary>
	/// Interaction logic for CalibDataPopUp.xaml
	/// </summary>
	public partial class CalibDataPopUp : Window
	{

		private IDI.Math.Vector gravityOffset_ = null;
		private IDI.Math.Vector frontOffset_ = null;


		public IDI.Math.Vector GravityOffset
		{
			get { return gravityOffset_; }
			set
			{
                gravityOffset_ = value ?? throw new InvalidOperationException("GravityOffset must be a Vector");

				if (IsLoaded)
				{
					tbGravOfsX_.Text = GravityOffset[0].ToString("f3");
					tbGravOfsY_.Text = GravityOffset[1].ToString("f3");
					tbGravOfsZ_.Text = GravityOffset[2].ToString("f3");
				}
			}
		}

		public IDI.Math.Vector FrontVector
		{
			get { return frontOffset_; }
			set
			{
                frontOffset_ = value ?? throw new InvalidOperationException("FrontVector must be a Vector");

				if (IsLoaded)
				{
					tbFrontVecX_.Text = FrontVector[0].ToString("f3");
					tbFrontVecY_.Text = FrontVector[1].ToString("f3");
					tbFrontVecZ_.Text = FrontVector[2].ToString("f3");
				}
			}
		}


		public CalibDataPopUp()
		{
			InitializeComponent();
		}


		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
		  if (GravityOffset != null)
		  {
			tbGravOfsX_.Text = GravityOffset[0].ToString("f3");
			tbGravOfsY_.Text = GravityOffset[1].ToString("f3");
			tbGravOfsZ_.Text = GravityOffset[2].ToString("f3");
		  }

		  if (FrontVector != null)
		  {
			tbFrontVecX_.Text = FrontVector[0].ToString("f3");
			tbFrontVecY_.Text = FrontVector[1].ToString("f3");
			tbFrontVecZ_.Text = FrontVector[2].ToString("f3");
		  }
		}
	}
}
