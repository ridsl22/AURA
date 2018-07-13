using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetromTablet.Models
{
	public class Node
	{
		private string id;
		private NodePin[] pin;
		private NodePin in0 = new NodePin();
		private NodePin in1 = new NodePin();
		private NodePin in2 = new NodePin();
		private NodePin in3 = new NodePin();
		private NodePin in4 = new NodePin();
		private NodePin in5 = new NodePin();
		private NodePin in6 = new NodePin();
		private NodePin in7 = new NodePin();
		private NodePin in8 = new NodePin();
		private NodePin in9 = new NodePin();	


		public string Id
		{
			get { return id; }
			set 
			{
				if (id != value)
				{
					id = value;
				}
			}
		}

		public NodePin[] Pin
		{
			get { return pin; }
			set
			{
				if (pin != value)
				{
					pin = value;
				}
			}
		}

		public NodePin In0
		{
			get { return in0; }
			set
			{
				if (in0 != value)
				{
					in0 = value;
				}
			}
		}

		public NodePin In1
		{
			get { return in1; }
			set
			{
				if (in1 != value)
				{
					in1 = value;
				}
			}
		}
		public NodePin In2
		{
			get { return in2; }
			set
			{
				if (in2 != value)
				{
					in2 = value;
				}
			}
		}
		public NodePin In3
		{
			get { return in3; }
			set
			{
				if (in3 != value)
				{
					in3 = value;
				}
			}
		}
		public NodePin In4
		{
			get { return in4; }
			set
			{
				if (in4 != value)
				{
					in4 = value;
				}
			}
		}
		public NodePin In5
		{
			get { return in5; }
			set
			{
				if (in5 != value)
				{
					in5 = value;
				}
			}
		}
		public NodePin In6
		{
			get { return in6; }
			set
			{
				if (in6 != value)
				{
					in6 = value;
				}
			}
		}
		public NodePin In7
		{
			get { return in7; }
			set
			{
				if (in7 != value)
				{
					in7 = value;
				}
			}
		}
		public NodePin In8
		{
			get { return in8; }
			set
			{
				if (in8 != value)
				{
					in8 = value;
				}
			}
		}
		public NodePin In9
		{
			get { return in9; }
			set
			{
				if (in9 != value)
				{
					in9 = value;
				}
			}
		}


		public Node()
		{
		}
		

		public Node(string _name, NodePin[] _pins)
		{
			Id = _name;
			In0 = _pins[0];
			In1 = _pins[1];
			In2 = _pins[2];
			In3 = _pins[3];
			In4 = _pins[4];
			In5 = _pins[5];
			In6 = _pins[6];
			In7 = _pins[7];
			In8 = _pins[8];
			In9 = _pins[9];
		}
	}
}
