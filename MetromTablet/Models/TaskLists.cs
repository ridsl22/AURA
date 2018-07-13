using MetromTablet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace MetromTablet
{
    public class TaskList
    {

        public ObservableCollection<CheckedListItem<Task>> Tasks { get; set; }
		

        public ObservableCollection<CheckedListItem<Task>> GenerateList(string version, string machineType)
        {
            Tasks = new ObservableCollection<CheckedListItem<Task>>();
            try
            {
                foreach (XElement versionElement in XElement.Load(@"C:\METROM\CheckList.xml").Elements("Version"))
                {
                    if (versionElement.Attribute("id").Value.Equals(version))
                    {
                        if (versionElement.Attribute("machineType").Value == machineType)
                        {
                            foreach (XElement taskElement in versionElement.Elements("Task"))
                            {
                                Tasks.Add(new CheckedListItem<Task>(new Task()
                                {
                                    Name = taskElement.Attribute("description").Value,
									ExpiryDate = DateTime.ParseExact(taskElement.Attribute("expiryDate").Value, "MM-dd-yyyy HH:mm:ss", null)
								},
								DateTime.ParseExact(taskElement.Attribute("expiryDate").Value, "MM-dd-yyyy HH:mm:ss", null) > DateTime.Now ? true : false));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return Tasks;
        }


		public void UpdateCheckList(string version, string machineType, ObservableCollection<CheckedListItem<Task>> tasks)
		{
			try
			{
				var doc = XElement.Load(@"C:\METROM\CheckList.xml");
				foreach (XElement versionElement in doc.Elements("Version"))
				{
					if (versionElement.Attribute("id").Value.Equals(version))
					{
						if (versionElement.Attribute("machineType").Value == machineType)
						{
							foreach (XElement taskElement in versionElement.Elements("Task"))
							{
								string res = tasks.First(s => s.Item.Name == taskElement.Attribute("description").Value).IsChecked ? "yes" : "no";
								if (res == "yes")
								{
									switch (version)
									{
										case "Daily Checklist":
											taskElement.Attribute("expiryDate").SetValue(DateTime.Today.AddDays(1).ToString("MM-dd-yyyy HH:mm:ss"));
											break;
										case "50 Hours Checklist":
											taskElement.Attribute("expiryDate").SetValue(DateTime.Now.AddHours(50).ToString("MM-dd-yyyy HH:mm:ss"));
											break;
										case "200 Hours Checklist":
											taskElement.Attribute("expiryDate").SetValue(DateTime.Now.AddHours(200).ToString("MM-dd-yyyy HH:mm:ss"));
											break;
										case "Seasonal Checklist":
											taskElement.Attribute("expiryDate").SetValue(MetromRailPage.EndOfSeason(MetromRailPage.Season(DateTime.Today)).ToString("MM-dd-yyyy HH:mm:ss"));
											break;
									}
								}
								else
								{
									taskElement.Attribute("expiryDate").SetValue(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"));
								}
							}
						}
					}
				}
				doc.Save(@"C:\METROM\CheckList.xml");
			}
			catch (Exception e)
			{
                MessageBox.Show(e.Message);
            }
		}
    }
}
