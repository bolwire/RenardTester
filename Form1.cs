using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;

namespace RenardTester
{
	public partial class Form1 : Form
	{

		private readonly object[] _baudRates = { "19200", "38400", "57600", "76800", "115200", "230400", "460800" };
		private SerialPort _port;
		private String[] _myPacket;

		public Form1()
		{
			InitializeComponent();
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			UpdateChannelList((int)numericUpDown1.Value);
		}

		private void UpdateChannelList(int channelCount)
		{
			checkedListBox1.BeginUpdate();
			checkedListBox1.Items.Clear();

			for (int i = 1; i <= channelCount; i++)
			{
				checkedListBox1.Items.Add("Channel " + i);
			}

			checkedListBox1.EndUpdate();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			UpdateChannelList((int)numericUpDown1.Value);
			comboBoxPort.Items.AddRange(items: SerialPort.GetPortNames());
			comboBoxBaudRate.Items.AddRange(_baudRates);
			textBox1.Text = trackBar1.Value.ToString("X");
		}

		private void trackBar1_ValueChanged(object sender, EventArgs e)
		{
			textBox1.Text = trackBar1.Value.ToString("X");
		}

		private void buttonStart_Click(object sender, EventArgs e)
		{
			if (_port != null && _port.IsOpen || comboBoxPort.Text == "" || comboBoxBaudRate.Text == "")
				return;

			_port = new SerialPort(comboBoxPort.Text, Convert.ToInt32(comboBoxBaudRate.Text), Parity.None, Convert.ToInt32(8),
				StopBits.One);


			try
			{
				_port.Handshake = Handshake.None;
				_port.Encoding = Encoding.UTF8;
				_port.RtsEnable = true;
				_port.DtrEnable = true;
				_port.Open();
				timer1.Enabled = true;

			}

			catch (IOException)
			{
				MessageBox.Show(String.Format("COM port: {0} could not be accessed.", _port.PortName), @"Write failed, IO Exception",
					MessageBoxButtons.OK);

			}
		}

		private void buttonStop_Click(object sender, EventArgs e)
		{
			if (_port == null)
				return;

			if (_port.IsOpen)
			{
				_myPacket = new String[(int)numericUpDown1.Value + 2];

				_myPacket[0] = "7E";
				_myPacket[1] = "80";

				for (int i = 0; i <= checkedListBox1.Items.Count - 1; i++)
				{
					_myPacket[i + 2] = "00";
				}

				byte[] myPacket = _myPacket.Select(s => Convert.ToByte(s, 16)).ToArray();

				_port.Write(myPacket, 0, myPacket.Length);
				_port.Close();
				_port.Dispose();
				timer1.Enabled = false;
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!_port.IsOpen)
				return;

			_myPacket = new String[(int)numericUpDown1.Value + 2];

			_myPacket[0] = "7E";
			_myPacket[1] = "80";

			for (int i = 0; i <= checkedListBox1.Items.Count - 1; i++)
			{
				_myPacket[i + 2] = checkedListBox1.CheckedIndices.Contains(i) ? trackBar1.Value.ToString("X") : "00";
			}

			byte[] myPacket = _myPacket.Select(s => Convert.ToByte(s, 16)).ToArray();

			_port.Write(myPacket, 0, myPacket.Length);
		}

		private void buttonAllOn_Click(object sender, EventArgs e)
		{
			SetAllChecks(true);
		}

		private void buttonAllOff_Click(object sender, EventArgs e)
		{
			SetAllChecks(false);
		}

		private void SetAllChecks(bool checkState)
		{
			for (int i = 0; i <= (checkedListBox1.Items.Count - 1); i++)
			{
				checkedListBox1.SetItemCheckState(i, checkState ? CheckState.Checked : CheckState.Unchecked);
			} 
		}


	}
}
