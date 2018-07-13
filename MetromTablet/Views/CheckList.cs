using MetromTablet.Models;
using MetromTablet.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace MetromTablet
{
    public partial class MetromRailPage : Page
    {
        #region Fields

        private const int fiftyHours = 180000; //seconds;
        private const int twoHundredHours = 720000; //seconds; 
        private const string vDaily = "Daily Checklist";
        private const string v50hours = "50 Hours Checklist";
        private const string v200hours = "200 Hours Checklist";
        private const string vSeason = "Seasonal Checklist";

        private ObservableCollection<CheckedListItem<Task>> Tasks { get; set; }
        private ObservableCollection<CheckedListItem<Task>> CheckedTasks { get; set; }
        private ObservableCollection<CheckList> LogTasks { get; set; }

        private DateTime dtDaily;
        private DateTime dt50Hours;
        private DateTime dt200Hours;
        private DateTime dtSeason;

        private TaskList list;

        private string selectedVersion;
        private string machineType;
        private Visibility checklistVisible;

        private ICommand checkTaskCommand;
        private ICommand unCheckTaskCommand;
        private ICommand skipCheckListCommand;

        #endregion Fields


        #region Properties

        public string SelectedVersion
        {
            get { return selectedVersion; }

            set
            {
                if (selectedVersion != value)
                {
                    selectedVersion = value;
                    NotifyPropertyChanged("SelectedVersion");
                }
            }
        }


        public string MachineType
        {
            get { return machineType; }

            set
            {
                if (machineType != value)
                {
                    machineType = value;
                    NotifyPropertyChanged("MachineType");
                }
            }
        }


        public Visibility ChecklistVisible
        {
            get
            {
                return checklistVisible;
            }
            set
            {
                checklistVisible = value;
                NotifyPropertyChanged("ChecklistVisible");
            }
        }

        #endregion Properties


        #region Commands

        public ICommand CheckTaskCommand
        {
            get
            {
                if (checkTaskCommand == null)
                {
                    checkTaskCommand = new RelayCommand(CheckTask);
                }
                return checkTaskCommand;
            }
        }


        public ICommand UnCheckTaskCommand
        {
            get
            {
                if (unCheckTaskCommand == null)
                {
                    unCheckTaskCommand = new RelayCommand(UnCheckTask);
                }
                return unCheckTaskCommand;
            }
        }


        public ICommand SkipCheckListCommand
        {
            get
            {
                if (skipCheckListCommand == null)
                {
                    skipCheckListCommand = new RelayCommand(SkipCheckList);
                }
                return skipCheckListCommand;
            }
        }

        #endregion Commands


        #region Methods 

        public void InitCheckList()
        {
            DataContext = this;

            DisplayChecklist(Visibility.Visible);
            machineType = LogInPage.machineType;           
            list = new TaskList();

            Tasks = list.GenerateList(vDaily, machineType);
            if (!EveryItemChecked(Tasks))
            {
                SelectedVersion = vDaily;
            }
            else
            {
                Tasks = list.GenerateList(v50hours, machineType);
                if (!EveryItemChecked(Tasks))
                {
                    SelectedVersion = v50hours;
                }
                else
                {
                    Tasks = list.GenerateList(v200hours, machineType);
                    if (!EveryItemChecked(Tasks))
                    {
                        SelectedVersion = v200hours;
                    }
                    else
                    {
                        Tasks = list.GenerateList(vSeason, machineType);
                        if (!EveryItemChecked(Tasks))
                        {
                            SelectedVersion = vSeason;
                            DisplayChecklist(Visibility.Hidden);
                        }
                    }
                }
            }

            CheckedTasks = new ObservableCollection<CheckedListItem<Task>>();
            LogTasks = new ObservableCollection<CheckList>();

            lbTasks.ItemsSource = Tasks;
            lbTasks.SelectedIndex = lbTasks.Items.Count - 1;
            lbTasks.ScrollIntoView(lbTasks.SelectedItem);
            lbTasks.SelectedIndex = 0;
            lbTasks.ScrollIntoView(lbTasks.SelectedItem);
            lbTasks.SelectedIndex = -1;
        }


        private void DisplayChecklist(Visibility v)
        {
            ChecklistVisible = v;
        }


        private void CheckTask()
        {
            GetCheckedTasks();
            if (CheckedTasks.Count == Tasks.Count)
            {
                GetTasks();
                if (SelectedVersion == vDaily)
                {
                    list.UpdateCheckList(vDaily, machineType, Tasks);
                    Tasks.Clear();
                    CheckedTasks.Clear();
                    if (dt50Hours != null && DateTime.Now.Subtract(dt50Hours).TotalSeconds > fiftyHours)
                    {
                        Tasks = list.GenerateList(v50hours, machineType);

                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = v50hours;
                    }
                    else if (dt200Hours != null && DateTime.Now.Subtract(dt200Hours).TotalSeconds > twoHundredHours)
                    {
                        Tasks = list.GenerateList(v200hours, machineType);
                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = v200hours;
                    }
                    else if ((dtSeason != null && Season(DateTime.Now) != Season(dtSeason)) || dtSeason.Year != DateTime.Now.Year)
                    {
                        Tasks = list.GenerateList(vSeason, machineType);
                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = vSeason;
                    }
                    else
                    {
                        NavigateToMetromRailScreen();
                    }
                }
                else if (SelectedVersion == v50hours)
                {
                    list.UpdateCheckList(v50hours, machineType, Tasks);
                    Tasks.Clear();
                    CheckedTasks.Clear();
                    if (dt200Hours != null && DateTime.Now.Subtract(dt200Hours).TotalSeconds > twoHundredHours)
                    {
                        Tasks = list.GenerateList(v200hours, machineType);
                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = v200hours;
                    }
                    else if ((dtSeason != null && Season(DateTime.Now) != Season(dtSeason)) || dtSeason.Year != DateTime.Now.Year)
                    {
                        Tasks = list.GenerateList(vSeason, machineType);
                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = vSeason;
                    }
                    else
                    {
                        NavigateToMetromRailScreen();
                    }
                }
                else if (SelectedVersion == v200hours)
                {
                    list.UpdateCheckList(v200hours, machineType, Tasks);
                    Tasks.Clear();
                    CheckedTasks.Clear();
                    if ((dtSeason != null && Season(DateTime.Now) != Season(dtSeason)) || dtSeason.Year != DateTime.Now.Year)
                    {
                        Tasks = list.GenerateList(vSeason, machineType);
                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = vSeason;
                    }
                    else
                    {
                        NavigateToMetromRailScreen();
                    }
                }
                else if (SelectedVersion == vSeason)
                {
                    list.UpdateCheckList(vSeason, machineType, Tasks);
                    Tasks.Clear();
                    CheckedTasks.Clear();
                    NavigateToMetromRailScreen();
                }
            }
        }


        private void UnCheckTask()
        {
            if (CheckedTasks != null && CheckedTasks.Count > 0)
                CheckedTasks.RemoveAt(0);
        }


        private void SkipCheckList()
        {
            GetTasks();
            if (SelectedVersion == vDaily)
            {
                list.UpdateCheckList(vDaily, machineType, Tasks);
                Tasks.Clear();
                CheckedTasks.Clear();
                Tasks = list.GenerateList(v50hours, machineType);
                if (!EveryItemChecked(Tasks))
                {
                    lbTasks.ItemsSource = Tasks;
                    SelectedVersion = v50hours;
                }
                else
                {
                    Tasks.Clear();
                    CheckedTasks.Clear();
                    Tasks = list.GenerateList(v200hours, machineType);
                    if (!EveryItemChecked(Tasks))
                    {
                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = v200hours;
                    }
                    else
                    {
                        Tasks.Clear();
                        CheckedTasks.Clear();
                        Tasks = list.GenerateList(vSeason, machineType);
                        if (!EveryItemChecked(Tasks))
                        {
                            lbTasks.ItemsSource = Tasks;
                            SelectedVersion = vSeason;
                        }
                        else
                        {
                            NavigateToMetromRailScreen();
                        }
                    }
                }
            }
            else if (SelectedVersion == v50hours)
            {
                list.UpdateCheckList(v50hours, machineType, Tasks);
                Tasks.Clear();
                CheckedTasks.Clear();
                Tasks = list.GenerateList(v200hours, machineType);
                if (!EveryItemChecked(Tasks))
                {
                    lbTasks.ItemsSource = Tasks;
                    SelectedVersion = v200hours;
                }
                else
                {
                    Tasks.Clear();
                    CheckedTasks.Clear();
                    Tasks = list.GenerateList(vSeason, machineType);
                    if (!EveryItemChecked(Tasks))
                    {
                        lbTasks.ItemsSource = Tasks;
                        SelectedVersion = vSeason;
                    }
                    else
                    {
                        NavigateToMetromRailScreen();
                    }
                }
            }
            else if (SelectedVersion == v200hours)
            {
                list.UpdateCheckList(v200hours, machineType, Tasks);
                Tasks.Clear();
                CheckedTasks.Clear();
                Tasks = list.GenerateList(vSeason, machineType);
                if (!EveryItemChecked(Tasks))
                {
                    lbTasks.ItemsSource = Tasks;
                    SelectedVersion = vSeason;
                }
                else
                {
                    NavigateToMetromRailScreen();
                }
            }
            else if (SelectedVersion == vSeason)
            {
                list.UpdateCheckList(vSeason, machineType, Tasks);
                Tasks.Clear();
                CheckedTasks.Clear();
                NavigateToMetromRailScreen();
            }
        }


        private void NavigateToMetromRailScreen()
        {
            CompleteLogTask(vDaily);
            CompleteLogTask(v50hours);
            CompleteLogTask(v200hours);
            CompleteLogTask(vSeason);
            DisplayChecklist(Visibility.Hidden);
        }


        private void CompleteLogTask(string version)
        {
            if (ChecklistCompleted(version))
            {
                List<Task> tList = new List<Task>();
                foreach (CheckedListItem<Task> item in list.GenerateList(version, machineType))
                    tList.Add(new Task(item.Item.Name, true));
                LogTasks.Add(new CheckList(version, machineType, tList));
            }
        }


        private bool EveryItemChecked(ObservableCollection<CheckedListItem<Task>> tasks)
        {
            foreach (CheckedListItem<Task> t in tasks)
                if (!t.IsChecked)
                    return false;
            return true;
        }


        private void checkBoxTask_Checked(object sender, RoutedEventArgs e)
        {
            CheckTask();
        }


        private void checkBoxTask_Unchecked(object sender, RoutedEventArgs e)
        {
            UnCheckTask();
        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            List<T> foundChilds = new List<T>();
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                T childType = child as T;
                if (childType == null)
                {
                    foreach (var other in FindVisualChildren<T>(child))
                        yield return other;
                }
                else
                {
                    yield return (T)child;
                }
            }
        }


        private void GetTasks()
        {
            // find all checkboxes in the window
            IEnumerable<CheckBox> myBoxes = FindVisualChildren<CheckBox>(this);

            List<Task> tList = new List<Task>();
            foreach (CheckBox cb in myBoxes)
            {
                if (cb.Name != "checkBoxTask")
                    continue;

                string taskDescription = string.Empty;
                if (cb.IsChecked == true)
                {
                    taskDescription = cb.Content.ToString().Replace("System.Windows.Controls.ContentControl: ", "");
                    tList.Add(new Task(taskDescription, true));
                    //LogTasks.Add(new CheckList(selectedVersion, machineType, (new Task(taskDescription, true), true));
                }
                else
                {
                    taskDescription = cb.Content.ToString().Replace("System.Windows.Controls.ContentControl: ", "");
                    tList.Add(new Task(taskDescription, false));
                }
            }
            LogTasks.Add(new CheckList(SelectedVersion, machineType, tList));
        }


        private bool ChecklistCompleted(string version)
        {
            foreach (CheckList list in LogTasks)
            {
                if (list.Version == version)
                    return false;
            }
            return true;
        }


        private void GetCheckedTasks()
        {
            // find all checkboxes in my window
            CheckedTasks.Clear();
            IEnumerable<CheckBox> myBoxes = FindVisualChildren<CheckBox>(this);
            foreach (CheckBox cb in myBoxes)
            {
                if (cb.Name != "checkBoxTask")
                    continue;
                if (cb.IsChecked == true)
                {
                    //CheckedTasks.Add(new CheckedListItem<Task>(new Task() { IsChecked = true }, true));
                    CheckedTasks.Add(new CheckedListItem<Task>(new Task() { ExpiryDate = DateTime.Now }, true));
                }
            }
        }


        public enum Seasons
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }

        // Get the season
        public static Seasons Season(DateTime date)
        {
            int doy = date.DayOfYear - Convert.ToInt32((DateTime.IsLeapYear(date.Year)) && date.DayOfYear > 59);
            if (doy < 60 || doy >= 325) return Seasons.Winter;
            if (doy >= 60 && doy < 152) return Seasons.Spring;
            if (doy >= 152 && doy < 244) return Seasons.Summer;
            return Seasons.Autumn;
        }


        public static DateTime EndOfSeason(Seasons season)
        {
            DateTime dateTime = DateTime.Today;
            switch (season)
            {
                case Seasons.Winter:
                    dateTime = DateTime.ParseExact("03-01-" + dateTime.Year + "00:00:00", "MM-dd-yyyy HH:mm:ss", null);
                    return dateTime;
                case Seasons.Spring:
                    dateTime = DateTime.ParseExact("06-01-" + dateTime.Year + "00:00:00", "MM-dd-yyyy HH:mm:ss", null);
                    return dateTime;
                case Seasons.Summer:
                    dateTime = DateTime.ParseExact("09-01-" + dateTime.Year + " 00:00:00", "MM-dd-yyyy HH:mm:ss", null);
                    return dateTime;
                case Seasons.Autumn:
                    dateTime = DateTime.ParseExact("12-01-" + dateTime.Year + "00:00:00", "MM-dd-yyyy HH:mm:ss", null);
                    return dateTime;
                default:
                    return DateTime.Today;
            }
        }

        #endregion Methods 
    }
}
