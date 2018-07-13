using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using IDI.Diagnostics;
using IDI.Logging;


namespace Metrom.AURA.ViewLog
{


  /// <summary>
  /// Interaction logic for SelectRangeDlg.xaml
  /// </summary>
  /// 
  public partial class SelectRangeDlg : Window
  {
    #region Instance Fields

    private DateTime startDate_;
    private bool startDateWasSet_;

    private DateTime endDate_;
    private bool endDateWasSet_;

	private bool hidden_;
	private DispatcherTimer mIdle;


    #endregion

    #region Properties

    public bool UseCount
    { get; private set; }

    public uint Count
    { get; private set; }

    public DateTime StartDate
    {
      get { return startDate_; }
      set
      {
        startDate_ = value;
        startDateWasSet_ = true;
      }
    }

    public DateTime EndDate
    {
      get { return endDate_; }
      set
      {
        endDate_ = value;
        endDateWasSet_ = true;
      }
    }

    #endregion

    #region Lifetime Management

    /// <summary>
    /// Default ctor.
    /// </summary>
    /// 
    public SelectRangeDlg(bool hidden)
    {
      InitializeComponent();
	  hidden_ = hidden;

	  //user inactivity
	  InputManager.Current.PreProcessInput += Idle_PreProcessInput;
	  mIdle = new DispatcherTimer();
	  mIdle.Interval = new TimeSpan(0, 0, 60);//appSettings.IdleIntervalSec);
	  mIdle.IsEnabled = true;
	  mIdle.Tick += Idle_Tick;
    }


	void Idle_Tick(object sender, EventArgs e)
	{
		this.Close();
	}


	void Idle_PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		mIdle.IsEnabled = false;
		mIdle.Stop();
		mIdle.IsEnabled = true;
		mIdle.Start();
	}

    #endregion

    #region UI Events

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      if (!startDateWasSet_ || !endDateWasSet_)
      {
        AppLogger.WriteToLog("SelectRangeDlg: Both StartDate and EndDate must be set! Closing dialog!", Severity.Fail);
        Close();
        return;  // EARLY RETURN!
		  
      }

      // Set the default choice in the Select Count groupbox.

      rbFixedCount_.IsChecked = true;

      rangeFromDate_.SelectedDate = startDate_;

      DateTime defaultEndDate = startDate_.AddDays(-7);

      if (endDate_ > defaultEndDate)
        defaultEndDate = endDate_;

      rangeToDate_.SelectedDate = defaultEndDate;

      rangeFromDate_.DisplayDateStart = endDate_;
      rangeFromDate_.DisplayDateEnd = startDate_;

      rangeToDate_.DisplayDateStart = endDate_;
      rangeToDate_.DisplayDateEnd = startDate_;

      // This forces our checked/unchecked code to do its job and disable controls inside the
      // Select Count groupbox:

      rbUseCount_.IsChecked = true;
      rbUseRange_.IsChecked = true;
	  if (hidden_)
		GenerateCasLog();
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      UseCount = rbUseCount_.IsChecked.GetValueOrDefault();

      if (UseCount)
      {
        if (rbFixedCount_.IsChecked.GetValueOrDefault())
          Count = 200;
        else if (rbCustomCount_.IsChecked.GetValueOrDefault())
        {
          uint val = 0;

          bool parseOk = uint.TryParse(tbCustomCount_.Text.Trim(), out val);

          if (parseOk && (val > 0) && (val < 62 * 1024))
            Count = val;
          else
          {
            MessageBox.Show("The custom download count must be an integer in the range [1, 63488].", "Custom Count Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;  // EARLY RETURN!
          }
        }
        else
          Count = 0;
      }
      else
      {
        if (rangeFromDate_.SelectedDate == null)
        {
          MessageBox.Show("Please select a Start date.", "Start Date Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
          return;  // EARLY RETURN!
        }
        else if (rangeToDate_.SelectedDate == null)
        {
          MessageBox.Show("Please select an End date.", "End Date Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
          return;  // EARLY RETURN!
        }
        else
        {
          DateTime start = rangeFromDate_.SelectedDate.Value;
          DateTime end = rangeToDate_.SelectedDate.Value;

          if (start < end)
          {
            MessageBox.Show("THe Start date must be more recent than or equal to the End date.", "Date Range Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;  // EARLY RETURN!
          }
          else
          {
            StartDate = start;
            EndDate = end;
          }
        }
      }

      DialogResult = true;
    }

	private void GenerateCasLog()
	{
		//UseCount = rbUseCount_.IsChecked.GetValueOrDefault();

		//if (UseCount)
		//{
		//	if (rbFixedCount_.IsChecked.GetValueOrDefault())
		//		Count = 200;
		//	else if (rbCustomCount_.IsChecked.GetValueOrDefault())
		//	{
		//		uint val = 0;

		//		bool parseOk = uint.TryParse(tbCustomCount_.Text.Trim(), out val);

		//		if (parseOk && (val > 0) && (val < 62 * 1024))
		//			Count = val;
		//		else
		//		{
		//			MessageBox.Show("The custom download count must be an integer in the range [1, 63488].", "Custom Count Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		//			return;  // EARLY RETURN!
		//		}
		//	}
		//	else
		//		Count = 0;
		//}
		//else
		//{
		//	if (rangeFromDate_.SelectedDate == null)
		//	{
		//		MessageBox.Show("Please select a Start date.", "Start Date Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		//		return;  // EARLY RETURN!
		//	}
		//	else if (rangeToDate_.SelectedDate == null)
		//	{
		//		MessageBox.Show("Please select an End date.", "End Date Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		//		return;  // EARLY RETURN!
		//	}
		//	else
		//	{
		//		DateTime start = DateTime.UtcNow; //rangeFromDate_.SelectedDate.Value;
		//		DateTime end = DateTime.UtcNow.AddDays(-1); //rangeToDate_.SelectedDate.Value;

		//		if (start < end)
		//		{
		//			MessageBox.Show("THe Start date must be more recent than or equal to the End date.", "Date Range Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		//			return;  // EARLY RETURN!
		//		}
		//		else
		//		{
					StartDate = DateTime.Now;
					EndDate = DateTime.Now.AddDays(-2);
		//		}
		//	}
		//}

		DialogResult = true;
	}


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void btnSetFromDateToLatest_Click(object sender, RoutedEventArgs e)
    {
      rangeFromDate_.SelectedDate = startDate_;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void btnSetToDateToOldest_Click(object sender, RoutedEventArgs e)
    {
      rangeToDate_.SelectedDate = endDate_;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void rbUseRange_CheckedChanged(object sender, RoutedEventArgs e)
    {
      gbSelectRange_.IsEnabled = rbUseRange_.IsChecked.GetValueOrDefault();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void rbUseCount_CheckedChanged(object sender, RoutedEventArgs e)
    {
      gbSelectCount_.IsEnabled = rbUseCount_.IsChecked.GetValueOrDefault();
    }

    #endregion
  
  }


}
