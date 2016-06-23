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

	Thread thread;
	bool runThread = true;
	bool updateThread;
		
	SerialPort sp;


	int xValue, yValue, Command;
	bool Error = true;
	int ErrorCounter = 0;
	int TotalRecieved = 0;
	[HideInInspector]
	public bool DataRecieved1 = false;
	[HideInInspector]
	public bool DataRecieved2 = false;
	[HideInInspector]
	public bool DataRecieved3 = false;
	[HideInInspector]
	public bool DataRecieved = false;
	[HideInInspector]
	public bool Data1Recieved = false;
	[HideInInspector]
	public bool Data2Recieved = false;

	int NumOfSerialBytes = 8;
	int[] serialInArray = new int[8];
	// Buffer array
	int serialCount = 0;
	// A count of how many bytes received
	int xMSB, xLSB, yMSB, yLSB;
	// Bytes of data

	//Dynamic arrays
	List<float> DynamicArrayTime1 = new List<float> (0);
	public List<float> DynamicArrayTime2 = new List<float> (0);
	public List<float> DynamicArrayTime3 = new List<float> (0);
	List<float> Time1 = new List<float> (0);
	public List<float> Time2 = new List<float> (0);
	List<float> Time3 = new List<float> (0);
	List<float> Voltage1 = new List<float> (0);
	List<float> Voltage2 = new List<float> (0);
	public List<float> Voltage3 = new List<float> (0);
	List<float> current = new List<float> (0);
	List<float> DynamicArray1 = new List<float> (0);
	List<float> DynamicArray2 = new List<float> (0);
	public List<float> DynamicArray3 = new List<float> (0);

	List<float> PowerArray = new List<float> (0);
	// Dynamic arrays that will use the append()
	List<float> DynamicArrayPower = new List<float> (0);
	// function to add values
	List<float> DynamicArrayTime = new List<float> (0);

	void Start ()
	{
		foreach (string s in SerialPort.GetPortNames())
			Debug.Log (s);
		
		sp = new SerialPort (m_name, m_baudRate);
		sp.ReadBufferSize = 20;
		sp.Open ();


		thread = new Thread (ThreadUpdate); 
		thread.Start (sp);

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
		UpdateGraph ();
	}

	void ThreadUpdate (object context)
	{

		SerialPort serialPort = context as SerialPort;

		while (runThread && serialPort.IsOpen) {
			if (updateThread) {
				updateThread = false;

//				//some crazy ass function that takes forever to do here
//				string inData = serialPort.ReadLine ();
//				print (inData);

				int inByte = sp.ReadByte ();
				if (inByte == 0)
					serialCount = 0;
				
				if (inByte > 255) {
					//print (" inByte = " + inByte);
				}

				// Add the latest byte from the serial port to array:
				serialInArray [serialCount] = inByte;
				serialCount++;

				Error = true;
				if (serialCount >= NumOfSerialBytes) {
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


					//println( "0\tCommand\tyMSB\tyLSB\txMSB\txLSB\tzeroByte\tsChecksum"); 
					//print (serialInArray [0] + "\t Command:" + Command + "\tyMSB:" + yMSB + "\tyLSB: " + yLSB + "\ttxLSB: " + xMSB + "\txLSB: " + xLSB + "\tzeroByte " + zeroByte + "\tserial in Array" + serialInArray [7]); 

					// >=====< combine bytes to form large integers >==================< //

					Command = serialInArray [1];

					xValue = xMSB << 8 | xLSB;                    // Get xValue from yMSB & yLSB  
					yValue = yMSB << 8 | yLSB;                    // Get yValue from xMSB & xLSB

					//print (Command + "  " + xValue + "  " + yValue + " ");

				}

//                 How that works: if xMSB = 10001001   and xLSB = 0100 0011 
//			       xMSB << 8 = 10001001 00000000    (shift xMSB left by 8 bits)                       
//			       xLSB =          01000011    
//			       xLSB | xMSB = 10001001 01000011    combine the 2 bytes using the logic or |
//			       xValue = 10001001 01000011     now xValue is a 2 byte number 0 -> 65536  

				Thread.Sleep (1);
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

	void UpdateGraph ()
	{
		switch (Command) {
		//         Recieve array1 and array2 from chip, update oscilloscope
		case 1: // Data is added to dynamic arrays
			DynamicArrayTime3.Add (xValue);
			DynamicArray3.Add (yValue);
			break;
		case 2: // An array of unknown size is about to be recieved, empty storage arrays
			DynamicArrayTime3.Clear ();
			DynamicArray3.Clear ();
			break;    
		case 3:  // Array has finnished being recieved, update arrays being drawn 
			Time3 = DynamicArrayTime3;
			Voltage3 = DynamicArray3;
							//   println(Voltage3.length);
			DataRecieved3 = true;
			break;  
		
		//                Recieve array2 and array3 from chip
		
		case 4: // Data is added to dynamic arrays
			DynamicArrayTime2.Add (xValue);
			DynamicArray2.Add ((yValue - 16000.0f) / 32000.0f * 20.0f);
			break;
		
		case 5: // An array of unknown size is about to be recieved, empty storage arrays
			DynamicArrayTime2.Clear ();
			DynamicArray2.Clear ();
			break;    
		
		case 6:  // Array has finnished being recieved, update arrays being drawn 
			Time2 = DynamicArrayTime2;
			current = DynamicArray2;
			DataRecieved2 = true;
			break;  
		//         Recieve a value of calculated power consumption & add it to the 
		//         PowerArray.
		case 20:  
			PowerArray.Add (yValue);
			break; 
		case 21:  
			DynamicArrayTime.Add (xValue); 
			DynamicArrayPower.Add (yValue);
			break;
		default:
			break;
		}
		


	}
}