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
			m_touchReceiver.OnNone += HandleTouches; 
			m_touchReceiver.OnTouch += HandleTouches;
			m_touchReceiver.OnGrab += HandleTouches;
			m_touchReceiver.OnInWater += HandleTouches;

		}

		void OnDisable ()
		{
			m_touchReceiver.OnNone -= HandleTouches; 
			m_touchReceiver.OnTouch -= HandleTouches;
			m_touchReceiver.OnGrab -= HandleTouches;
			m_touchReceiver.OnInWater -= HandleTouches;

		}

		void HandleTouches (ToucheTouchType touch)
		{
			for (int i = 0; i < m_buttonImages.Length; i++) {
				if (i == (int)touch.typeOfTouch) {
					m_buttonImages [i].color = Color.red;
					m_buttonImages [i].fillAmount = touch.amount;
				} else {
					m_buttonImages [i].color = Color.white;
					m_buttonImages [i].fillAmount = 1f;
				}
			}
		}
	}
}