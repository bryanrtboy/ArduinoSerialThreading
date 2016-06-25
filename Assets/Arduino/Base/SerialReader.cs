//Bryan Leister - June 2016
//
//This script reads from the serial port on it's own thread and passes to the byteReciever (still in the same thread)
//NOTE:  You cannot run Unity operations off the main thread, and so you must be careful to pass data in such a way
//that is thread safe. I am approaching this by setting booleans in both the Reader and Reciever script, when true
//do the Unity stuff and making sure to not try to do 'Unity stuff' with data that is owned by the other thread.
//
using UnityEngine;
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
		public string m_name = "COM3";
		public int m_baudRate = 115200;
		public ByteReciever m_receiver;
		public float m_updateRate = .01f;
		public int m_bufferSize = 512;

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
				foreach (string s in SerialPort.GetPortNames()) {
					m_comPortsFound.Add (s);
				}
				m_dropDownList.AddOptions (m_comPortsFound);

			}
		}

		void Start ()
		{
			if (SerialPortNameExists ())
				OpenSerialPort ();
			else
				Debug.LogError ("No Serial port of that name");
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
						Debug.Log ("serial port already exists and is open!");
					} else {
						Debug.Log ("serial port already exists, but it's not open...");
					}
				}
			}
		}

		bool SerialPortNameExists ()
		{
			bool noMatches = true;
			foreach (string s in SerialPort.GetPortNames()) {
				if (s == m_name)
					noMatches = false;  //At least one name matches
				Debug.Log (s);
			}
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
			if (sp.IsOpen)
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
			EndThreads ();  
		}

		void EndThreads ()
		{
			runThread = false;
			//you could use thread.abort() but that has issues on iOS

			while (m_thread.IsAlive) {
				//simply have main loop wait till thread ends
			}
		}
	}
}