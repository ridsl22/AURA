using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{
    public class CheckList : IEnumerable<Task>
    {
        public string Version { get; set; }
        public string MachineType { get; set; }
        public List <Task> Tasks { get; set; }

        public CheckList() { }


		public CheckList(string _version, string _machineType, List<Task> _tasks)
		{
			Version = _version;
			MachineType = _machineType;
			Tasks = new List<Task>();
			foreach (Task t in _tasks)
			{
				Tasks.Add(t);
			}
		}


        public CheckList(CheckList _checklist)
		{
			Version = _checklist.Version;
            MachineType = _checklist.MachineType;
            Tasks = new List<Task>();
			foreach (Task t in _checklist.Tasks)
			{
                Tasks.Add(t);
			}
		}


		private IEnumerable<Task> Events()
		{
			yield return new Task();
			yield return new Task();
		}

		public IEnumerator<Task> GetEnumerator()
		{
			return Events().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
    }
}
