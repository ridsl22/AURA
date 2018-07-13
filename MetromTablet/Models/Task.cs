using System;

namespace MetromTablet
{
	public class Task
	{
        public string Name { get; set; }
        public bool IsChecked { get; set; }
		public DateTime ExpiryDate { get; set; }


		public Task(){}


		public Task(string _name, DateTime _expDate)
		{
			Name = _name;
			ExpiryDate = _expDate;
		}


		public Task(string _name, bool _isChecked)
		{
			Name = _name;
			IsChecked = _isChecked;
		}

	}
}
