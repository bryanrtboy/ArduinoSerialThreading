using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO.Ports;

public class SerialReader : MonoBehaviour
{

	public string m_name = "COM3";
	public int m_baudRate = 115200;
	public float m_updateRate = .1f;
	public LineGraph m_lineGraph;

	Thread m_thread;
	bool runThread = true;
	[HideInInspector]
	public bool updateThread;
	[HideInInspector]
	public bool freshData = false;

	SerialPort sp;

	int xValue, yValue, Command;
	bool Error = true;
	int ErrorCounter = 0;
	int TotalRecieved = 0;
	bool DataRecieved = false;

	int NumOfSerialBytes = 8;
	int[] serialInArray = new int[8];
	// Buffer array
	int serialCount = 0;
	// A count of how many bytes received
	int xMSB, xLSB, yMSB, yLSB;
	// Bytes of data
	[HideInInspector]
	public List<float> incomingXValues = new List<float> (0);
	[HideInInspector]
	public List<float> XPacket = new List<float> (0);
	[HideInInspector]
	public List<float> YPacket = new List<float> (0);
	[HideInInspector]
	public List<float> incomingYValues = new List<float> (0);


	void Start ()
	{
		foreach (string s in SerialPort.GetPortNames())
			Debug.Log (s);

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

				int bufferSize = 255;				    
				byte[] buffer = new byte[bufferSize];   //make a buffer to hold a chunk of the stream
				sp.Read (buffer, 0, bufferSize);		//Get the data

				for (int i = 0; i < bufferSize; i++) {

					int inByte = buffer [i];

					if (inByte == 0) {					//The Arduiono Touche program is using a 0 to signal the start of a packet
						serialCount = 0;
					}

					if (inByte > 255) {
						print (" inByte = " + inByte);
						return;
					}

					// Add the latest byte from the serial port to array:
					serialInArray [serialCount] = inByte;
					serialCount++;

					Error = true;
					if (serialCount >= NumOfSerialBytes) { //Once we have the 8 bites of a packet, reset
						serialCount = 0;

						TotalRecieved++;

						int Checksum = 0; //Error checking byte

						//    Checksum = (Command + yMSB + yLSB + xMSB + xLSB + zeroByte)%255;
						for (int x = 0; x < serialInArray.Length - 1; x++) {
							Checksum = Checksum + serialInArray [x];
						}

						Checksum = Checksum % 255;

						if (Checksum == serialInArray [serialInArray.Length - 1]) {
							Error = false;
							DataRecieved = true;
						} else {
							Error = true;
							//  println("Error:  "+ ErrorCounter +" / "+ TotalRecieved+" : "+float(ErrorCounter/TotalRecieved)*100+"%");
							DataRecieved = false;
							ErrorCounter++;
							float _error = (ErrorCounter / TotalRecieved) * 100;
							print ("Error:  " + ErrorCounter + " / " + TotalRecieved + " : " + _error + "%");
						}
					}

					if (!Error) {


						int zeroByte = serialInArray [6]; //Which values have a zero value
						// println (zeroByte & 2);

						xLSB = serialInArray [3]; //X value's least significant byte
						if ((zeroByte & 1) == 1)
							xLSB = 0;
						xMSB = serialInArray [2]; //X value's most significant byte     
						if ((zeroByte & 2) == 2)
							xMSB = 0;

						yLSB = serialInArray [5]; //Y value's least significant byte
						if ((zeroByte & 4) == 4)
							yLSB = 0;

						yMSB = serialInArray [4]; //Y value's most significant byte
						if ((zeroByte & 8) == 8)
							yMSB = 0;

						//print (serialInArray [0] + "\t Command:" + Command + "\tyMSB:" + yMSB + "\tyLSB: " + yLSB + "\ttxLSB: " + xMSB + "\txLSB: " + xLSB + "\tzeroByte " + zeroByte + "\tChecksum " + serialInArray [7]); 

						// >=====< combine bytes to form large integers >==================< //

						Command = serialInArray [1];
						xValue = xMSB << 8 | xLSB;                    // Get xValue from yMSB & yLSB  
						yValue = yMSB << 8 | yLSB;                    // Get yValue from xMSB & xLSB

						//                 How that works: if xMSB = 10001001   and xLSB = 0100 0011 
						//			       xMSB << 8 = 10001001 00000000    (shift xMSB left by 8 bits)                       
						//			       xLSB =          01000011    
						//			       xLSB | xMSB = 10001001 01000011    combine the 2 bytes using the logic or |
						//			       xValue = 10001001 01000011     now xValue is a 2 byte number 0 -> 65536  

//						print ("Got something: " + Command + "  " + xValue + "  " + yValue + " ");

						UpdateDictionaries ();
					}


				} //end buffer read

				//Thread.Sleep (1);
			}
		}  
	}

	void UpdateDictionaries ()
	{
		switch (Command) {
		//Recieve array1 and array2 from chip, update oscilloscope
		case 1: // Data is added to dynamic arrays
			incomingXValues.Add (xValue);
			incomingYValues.Add (yValue);
			m_lineGraph.UpdateYPositions (incomingYValues.ToArray ());
			break;
		case 2: // An array of unknown size is about to be recieved, empty storage arrays
			incomingXValues = new List<float> (0);
			incomingYValues = new List<float> (0);
			break;    
		case 3:  // Array has finished being recieved, update arrays being drawn 
			XPacket = incomingXValues;
			YPacket = incomingYValues;
//			print ("V3: " + Voltage3.Count + ", T3: " + Time3.Count + " @" + Time.time);
			m_lineGraph.UpdateYPositions (YPacket.ToArray ());
			DataRecieved = true;
			break;  

		//Recieve array2 and array3 from chip

//		case 4: // Data is added to dynamic arrays
//			DynamicArrayTime2.Add (xValue);
//			DynamicArray2.Add ((yValue - 16000.0f) / 32000.0f * 20.0f);
//			print ("V2: " + Voltage2.Count + ", Dyn T2" + Time2.Count + " @" + Time.time);
//			break;
//
//		case 5: // An array of unknown size is about to be recieved, empty storage arrays
//			DynamicArrayTime2.Clear ();
//			DynamicArray2.Clear ();
//			break;    
//
//		case 6:  // Array has finnished being recieved, update arrays being drawn 
//			Time2 = DynamicArrayTime2;
//			current = DynamicArray2;
//			print ("current: " + current.Count + ", T2" + Time2.Count + " @" + Time.time);
//			DataRecieved2 = true;
//			break;  
//		//         Recieve a value of calculated power consumption & add it to the 
//		//         PowerArray.
//		case 20:  
//			PowerArray.Add (yValue);
//			break; 
//		case 21:  
//			DynamicArrayTime.Add (xValue); 
//			DynamicArrayPower.Add (yValue);
//			break;
		default:
			break;
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

		while (m_thread.IsAlive) {
			//simply have main loop wait till thread ends
		}
	}
}