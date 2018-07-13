using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MetromTablet.Models
{
	public class MachineConfig
	{

		public string MachineName { get; set; }
		public List<Node> Nodes { get; set; }
		public int EncoderResolution { get; set; }
		public float WheelDiameter { get; set; }
		public byte Direction { get; set; }
		public float ClarDistance { get; set; }
		public byte ProdReportInterval { get; set; }


		public MachineConfig() { }


		public MachineConfig(MachineConfig _machineConfig)
		{
			MachineName = _machineConfig.MachineName;
			Nodes = new List<Node>();

			foreach (Node n in _machineConfig.Nodes)
			{
				Nodes.Add(n);
			}

			EncoderResolution = _machineConfig.EncoderResolution;
			WheelDiameter = _machineConfig.WheelDiameter;
			Direction = _machineConfig.Direction;
			ClarDistance = _machineConfig.ClarDistance;
			ProdReportInterval = _machineConfig.ProdReportInterval;
		}
	}
}
