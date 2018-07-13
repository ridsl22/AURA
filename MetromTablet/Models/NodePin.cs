using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MetromTablet.Models
{
	public class NodePin
	{
		private string id;
		
		public int IdleTime { get; set; }
		public float LogicLevel { get; set; }
		public int Offset { get; set; }
		public int Gain { get; set; }
		public Edge edge { get; set; }
		public InputType PinType { get; set; }
		public DigWorkCycleMode CycleType { get; set; }

		public string Id
		{ 
			get { return id; } 
			set 
			{
				id = value;
				NotifyPropertyChanged("Id");
			} 
		}

		public NodePin() {}


		public NodePin(string name, float _logicLevel, int _idleTime,  Edge _nodeEdge, InputType _inType, int _offset, int _gain,
			DigWorkCycleMode _cycleType)
		{
			Id = name;
			LogicLevel = _logicLevel;
			IdleTime = _idleTime;
			Offset = _offset;
			Gain = _gain;
			edge = _nodeEdge;
			PinType = _inType;
			CycleType = _cycleType;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
	

	public enum InputType
	{
		NotInUse = 0,
		DigVehicleMode,
		DigEncoderMode,
		DigWorkCycleMode,
		DigEngineMode,
		AnalogInputMode
	}


	public enum Edge
	{
 		Rising = 1,
		Falling,
		RisingFalling
	}

	public enum DigWorkCycleMode
	{
		WorkCycleMain = 2,
		WorkCycleRight,
		WorkCycleLeft,
		WorkCycleFoot
	}
}
