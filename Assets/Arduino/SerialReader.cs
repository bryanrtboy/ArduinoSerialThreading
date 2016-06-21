using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.IO.Ports;

public class SerialReader : MonoBehaviour
{

	public string m_name = "COM3";
	public int m_baudRate = 115200;
	public float m_updateRate = .1f;

	Thread thread;
	bool runThread = true;
	bool updateThread;
		
	SerialPort sp;

	void Start ()
	{
		foreach (string s in SerialPort.GetPortNames())
			Debug.Log (s);

		sp = new SerialPort (m_name, m_baudRate);
		sp.Open ();

		thread = new Thread (ThreadUpdate); 
		thread.Start (sp);

		InvokeRepeating ("UpdateInterval", .5f, m_updateRate);
	}

	void OnEnable ()
	{
		//StartThread ();

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
			if (updateThread) {
				updateThread = false;
				//some crazy ass function that takes forever to do here
				string inData = serialPort.ReadLine ();
				print (inData);
				Thread.Sleep (10);
			}
		}  
	}

	void OnApplicationQuit ()
	{
		EndThreads ();  
	}

	//This must be called from OnApplicationQuit AND before the loading of a new level.
	//Threads spawned from this class must be ended when this class is destroyed in level changes.
	void EndThreads ()
	{
		runThread = false;
		//you could use thread.abort() but that has issues on iOS

		while (thread.IsAlive) {
			//simply have main loop wait till thread ends
		}
	}
}