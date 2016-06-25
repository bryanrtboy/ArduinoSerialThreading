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
		public TouchDetector m_touchReceiver;
		public Image[] m_buttonImages = new Image[4];
		//With 4 types of touches, we need to have 4 images and they should be sorted in order that matches the ToucheTouchType

		void OnEnable ()
		{
			m_touchReceiver.OnNone += HandleNone; 
			m_touchReceiver.OnTouch += HandleTouch;
			m_touchReceiver.OnGrab += HandleGrab;
			m_touchReceiver.OnInWater += HandleInWater;

		}

		void OnDisable ()
		{
			m_touchReceiver.OnNone -= HandleNone; 
			m_touchReceiver.OnTouch -= HandleTouch;
			m_touchReceiver.OnGrab -= HandleGrab;
			m_touchReceiver.OnInWater -= HandleInWater;

		}

		void HandleNone (ToucheTouchType touch)
		{
			if (touch.isActive)
				m_buttonImages [0].color = Color.red;
			else
				m_buttonImages [0].color = Color.white;
		
			m_buttonImages [0].fillAmount = touch.amount;
		}

		void HandleTouch (ToucheTouchType touch)
		{

			if (touch.isActive)
				m_buttonImages [1].color = Color.red;
			else
				m_buttonImages [1].color = Color.white;

			m_buttonImages [1].fillAmount = touch.amount;
		}

		void HandleGrab (ToucheTouchType touch)
		{
			if (touch.isActive)
				m_buttonImages [2].color = Color.red;
			else
				m_buttonImages [2].color = Color.white;

			m_buttonImages [2].fillAmount = touch.amount;
		}

		void HandleInWater (ToucheTouchType touch)
		{
			if (touch.isActive)
				m_buttonImages [3].color = Color.red;
			else
				m_buttonImages [3].color = Color.white;

			m_buttonImages [3].fillAmount = touch.amount;
		}
	}
}