//Bryan Leister June 2016
//
/// <summary>
/// /*Simple implementation of checking when gesture is being made based on MaxYValues.
/// TODO: Implement Dynamic Time Warping to check curve for more accurate results...
/// */
/// </summary>
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ArduinoSerialReader
{
	public class UI : MonoBehaviour
	{

		public Image[] m_buttonImages = new Image[4];
		//With 4 types of touches, we need to have 4 images and they should be sorted in order that matches the ToucheTouchType

		void Awake ()
		{
			if (TouchDetector.instance == null) {
				Debug.LogError ("No Touch Detector in Scene!");
				Destroy (this);
				return;
			}
		}

		void OnEnable ()
		{
			TouchDetector.instance.TouchOn += HandleNewTouchDetected;
			TouchDetector.instance.TouchOff += HandleTouchOff;
		}

		void OnDisable ()
		{
			TouchDetector.instance.TouchOn -= HandleNewTouchDetected;
			TouchDetector.instance.TouchOff -= HandleTouchOff;
		}

		void HandleNewTouchDetected (ToucheTouch.Type type)
		{
			m_buttonImages [(int)type].color = Color.red;
		}

		void HandleTouchOff (ToucheTouch.Type type)
		{
			m_buttonImages [(int)type].color = Color.grey;
		}
			
	}
}