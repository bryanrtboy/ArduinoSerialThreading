//Bryan Leister - June 2016
//
//This script reads from the serial port on it's own thread and passes to the byteReciever (still in the same thread)
//NOTE:  You cannot run Unity operations off the main thread, and so you must be careful to pass data in such a way
//that is thread safe. I am approaching this by setting booleans in both the Reader and Reciever script, when true
//do the Unity stuff and making sure to not try to do 'Unity stuff' with data that is owned by the other thread.
//
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.IO.Ports;
using UnityEngine.UI;

namespace ArduinoSerialReader
{
	public class SerialReader : MonoBehaviour
	{
		public Dropdown m_dropDownList;
		//For Mac, my serial port name is /dev/cu.usbserial-AM01PK2W
		//For PC, my serial port is either COM3 or COM4
		public string m_name = "COM3";
		public int m_baudRate = 115200;
		public ByteReciever m_receiver;
		public float m_updateRate = .01f;
		public int m_bufferSize = 512;
		public Text m_debugMessages;

		Thread m_thread;
		bool runThread = true;
		bool updateThread;
		string m_storedPortName = "SerialPortName";
		List<string> m_comPortsFound = new List<string> (0);

		SerialPort sp;

		void Awake ()
		{
			if (PlayerPrefs.HasKey (m_storedPortName))
				m_name = PlayerPrefs.GetString (m_storedPortName);

			if (m_dropDownList != null) {
				m_dropDownList.ClearOptions ();

				#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
				Debug.Log ("Found " + GetPortNames ().Length + " serial ports");
				foreach (string s in GetPortNames()) { //Local method for getting Mac Port Names
					m_comPortsFound.Add (s);
				}
				#else
				foreach (string s in SerialPort.GetPortNames()) {
					m_comPortsFound.Add (s);
				}
				#endif

				m_comPortsFound.Add ("None"); //Put a default entry so we can 'change' ports
				m_dropDownList.AddOptions (m_comPortsFound);



			}
		}

		void Start ()
		{
			#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX

			string str = "Found " + GetPortNames ().Length + " serial ports";
			Debug.Log (str);

			if (m_debugMessages)
				m_debugMessages.text = str;

			foreach (string s in GetPortNames()) {
				Debug.Log (s);
				if (m_debugMessages)
					m_debugMessages.text += "\n" + s;
			}
			#else

			string str = "Found " + SerialPort.GetPortNames ().Length + " serial ports";
			Debug.Log (str);

			if (m_debugMessages)
				m_debugMessages.text = str;

			foreach (string s in SerialPort.GetPortNames()) {
				Debug.Log (s);
				if (m_debugMessages)
					m_debugMessages.text += "\n" + s;
			}
			#endif


			if (SerialPortNameExists ()) {
				OpenSerialPort ();
			} else {
				Debug.LogError ("No Serial port named " + m_name);

				if (m_debugMessages)
					m_debugMessages.text = "No Serial port named " + m_name;
			}
		}

		public void ChangeSerialPortName (int value)
		{
			m_name = m_comPortsFound [value];
			PlayerPrefs.SetString (m_storedPortName, m_name);

			if (SerialPortNameExists ()) {
				if (sp == null) {
					OpenSerialPort ();
				} else {
					if (sp.IsOpen) {
						Debug.Log ("Serial port already exists and is open!");
						if (m_debugMessages)
							m_debugMessages.text = "Serial port already exists and is open!";
					} else {
						Debug.Log ("Serial port already exists, but it's not open! Trying to open it now...");

						if (m_debugMessages)
							m_debugMessages.text = "Serial port already exists, but it's not open! Trying to open it now...";

						OpenSerialPort ();
					}
				}
			}
		}

		bool SerialPortNameExists ()
		{
			bool noMatches = true;
			#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			foreach (string s in GetPortNames()) {
				if (s == m_name)
					noMatches = false;  //At least one name matches
				Debug.Log (s);
			}

			#else
			foreach (string s in SerialPort.GetPortNames()) {
				if (s == m_name)
					noMatches = false;  //At least one name matches
				Debug.Log (s);
			}
			#endif

			return !noMatches; //Return true
		}

		void OpenSerialPort ()
		{
			sp = new SerialPort (m_name, m_baudRate);
			sp.Open ();
			m_thread = new Thread (ThreadUpdate);
			m_thread.Start (sp);
			InvokeRepeating ("UpdateInterval", .5f, m_updateRate);
		}

		void OnDisable ()
		{
			if (sp != null && sp.IsOpen)
				sp.Close ();
		}

		void UpdateInterval ()
		{
			updateThread = true;
		}

		void ThreadUpdate (object context)
		{

			SerialPort serialPort = context as SerialPort;

			while (runThread && serialPort.IsOpen) {
				if (updateThread) {						    //Only run the thread this often based on when it is set to true
					updateThread = false;
								    
					byte[] buffer = new byte[m_bufferSize];   //make a buffer to hold a chunk of the stream
					sp.Read (buffer, 0, m_bufferSize);		//Get the data

					if (buffer [0] != null) {
						if (m_receiver == null) {
							Debug.LogError ("No receiver!");
							return;
						}
						m_receiver.HandleBytes (buffer);	//Still running on this thread
					}
					//Thread.Sleep (1);  \\might prevent overflow of buffer
				}
			}  
		}


		void OnApplicationQuit ()
		{
			if (m_thread != null && m_thread.IsAlive)
				EndThreads ();  
		}

		void EndThreads ()
		{
			runThread = false;
			//you could use thread.abort() but that has issues on iOS
			Debug.Log ("Ending Thread");
			while (m_thread.IsAlive) {
				//simply have main loop wait till thread ends
			}
		}

		string[] GetPortNames ()
		{
			int p = (int)Environment.OSVersion.Platform;
			List<string> serial_ports = new List<string> ();
         
			// Are we on Unix?
			if (p == 4 || p == 128 || p == 6) {
				string[] ttys = Directory.GetFiles ("/dev/", "*");

				foreach (string dev in ttys) {
					if (dev.StartsWith ("/dev/tty.") || dev.StartsWith ("/dev/cu.")) {
						serial_ports.Add (dev);	
						Debug.Log (String.Format (dev));
					}
				}
			}
			return serial_ports.ToArray ();
		}
	}
}