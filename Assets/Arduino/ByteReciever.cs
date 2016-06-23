//Bryan Leister - June 2016
//Reciever for the Arduino Touche by Mads Hobye: http://www.instructables.com/id/Touche-for-Arduino-Advanced-touch-sensing/
//
//Here is how the commands are structured coming from the Arduino, which in turn drives our Events
//
//void PlottArray(unsigned int Cmd,float Array1[],float Array2[]){
//
//	SendData(Cmd+1, 1,1);                               // Tell PC an array is about to be sent  (Clear the array) Command = 2;                   
//	delay(1);
//	for(int x=0;  x < sizeOfArray;  x++){               // Send the arrays 
//		SendData(Cmd, round(Array1[x]),round(Array2[x])); //Command 1 Adds to the array
//		delay(1);
//	}
//	SendData(Cmd+2, 1,1);                                // Confirm arrrays have been sent - Command 3
//}
//
//Command 0 is reserved for defining when a new packet is coming.


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ByteReciever : MonoBehaviour
{

	public event Action<float[]> OnByteReceived;

	public SerialReader m_serialReader;

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
	List<float> incomingXValues = new List<float> (0);
	List<float> XPacket = new List<float> (0);
	List<float> YPacket = new List<float> (0);
	List<float> incomingYValues = new List<float> (0);

	float[] yPositions;


	void Awake ()
	{
		if (m_serialReader == null) {
			Debug.LogError ("No Serial Reader!");
			Destroy (this);
		}

		if (m_serialReader.m_receiver == null)
			m_serialReader.m_receiver = this;
	}

	void Update ()
	{
		if (DataRecieved) {
			DataRecieved = false;
			if (OnByteReceived != null && yPositions != null)
				OnByteReceived (yPositions);
		}
	}

	public void HandleBytes (byte[] buffer)  //still in the read thread
	{
		int buffersize = buffer.Length;
		for (int i = 0; i < buffersize; i++) {

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

				//Command, xValue & yValue have now been recieved by the chip

				UpdateDictionaries ();
			}


		} //end buffer read

	}

	void UpdateDictionaries ()
	{
		switch (Command) {
		//Recieve array1 and array2 from chip, update oscilloscope
		case 1: // Data is added to dynamic arrays
			incomingXValues.Add (xValue);
			incomingYValues.Add (yValue);
//			yPositions = incomingYValues.ToArray ();   //This can make it feel more responsive by updated as data comes in...
//			DataRecieved = true;
			break;
		case 2: // An array of unknown size is about to be recieved, empty storage arrays
			incomingXValues = new List<float> (0);
			incomingYValues = new List<float> (0);
			break;    
		case 3:  // Array has finished being recieved, update arrays being drawn 
			XPacket = incomingXValues;
			YPacket = incomingYValues;
			//			print ("V3: " + Voltage3.Count + ", T3: " + Time3.Count + " @" + Time.time);
			yPositions = incomingYValues.ToArray ();   //Send data when Command says
			DataRecieved = true;
			incomingXValues.Clear ();
			incomingYValues.Clear ();
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

}
