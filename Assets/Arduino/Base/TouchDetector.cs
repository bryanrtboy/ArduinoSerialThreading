using UnityEngine;
using System;
using System.Collections;

namespace ArduinoSerialReader
{
	public class TouchDetector : MonoBehaviour
	{

		public event Action<ToucheTouchType> OnNone;
		public event Action<ToucheTouchType> OnTouch;
		public event Action<ToucheTouchType> OnGrab;
		public event Action<ToucheTouchType> OnInWater;

		public ByteReciever m_receiver;
		//These should be attached to Buttons Objects that are named - None, Touch, Grab, InWater. Buttons should send their position when pressed to SetButtonMaxPosition

		Vector2[] m_storedGesturePoints = new Vector2[4];
		float[] m_gestureDistances = new float [4];
		Vector2 m_lastMaxYposition = Vector2.zero;
		ToucheTouchType m_touchType;
		int m_touchTypeCount;

		void Awake ()
		{
			m_touchTypeCount = System.Enum.GetValues (typeof(ToucheTouchType.Kind)).Length;
			m_storedGesturePoints = new Vector2[m_touchTypeCount];
			m_gestureDistances = new float [m_touchTypeCount];
		}

		void OnEnable ()
		{
			GetMaxPositions ();
			m_receiver.OnByteReceived += GestureCompare;
		}

		void OnDisable ()
		{
			m_receiver.OnByteReceived -= GestureCompare;
		}

		public void GetMaxPositions ()
		{
			for (int i = 0; i < m_touchTypeCount; i++) {
				ToucheTouchType.Kind k = (ToucheTouchType.Kind)i;
				if (PlayerPrefs.HasKey (k.ToString () + "x") && PlayerPrefs.HasKey (k.ToString () + "y")) {
					float x = PlayerPrefs.GetFloat (k.ToString () + "x");	
					float y = PlayerPrefs.GetFloat (k.ToString () + "y");
					m_storedGesturePoints [i] = new Vector2 (x, y);
				}
			}
		}

		public void SetMaxPosition (int buttonIndex)
		{

			ToucheTouchType.Kind k = (ToucheTouchType.Kind)buttonIndex;
			m_storedGesturePoints [buttonIndex] = m_lastMaxYposition;
			PlayerPrefs.SetFloat (k.ToString () + "x", m_lastMaxYposition.x);
			PlayerPrefs.SetFloat (k.ToString () + "y", m_lastMaxYposition.y);
		}

		void GestureCompare (Vector2[] positions, int maxYposition)
		{
			m_lastMaxYposition = positions [maxYposition];

			float totalDist = 0;
			int currentMax = 0;
			float currentMaxValue = -1;

			for (int i = 0; i < m_touchTypeCount; i++) {
				//Store the maxY so that if a button is pressed, we will enter this value for that button.
				m_lastMaxYposition = positions [maxYposition];

				//Calculate the distance for this button, and put it in an array of distances for all of the buttons
				m_gestureDistances [i] = Vector2.Distance (m_lastMaxYposition, m_storedGesturePoints [i]);
				totalDist += m_gestureDistances [i];
				if (m_gestureDistances [i] < currentMaxValue || i == 0) {
					currentMax = i;
					currentMaxValue = m_gestureDistances [i];
				}
			}

			totalDist = totalDist / 3;


			for (int i = 0; i < m_touchTypeCount; i++) {
				float currentAmmount = 0;
				currentAmmount = 1 - m_gestureDistances [i] / totalDist; //How much of the button is filled (strength of signal)

				m_touchType = new ToucheTouchType ();

				if (currentMax == i) {
					m_touchType.isActive = true; // if it is the current one, it must be active
					m_touchType.amount = currentAmmount;
				} else {
					m_touchType.isActive = false; // if it is the current one, it must be active
					m_touchType.amount = 1; //If this is not the current matching touch, just fill the button entirely
				}

				switch (i) {
				case 0:
					if (m_touchType.isActive) {
						m_touchType.typeOfTouch = ToucheTouchType.Kind.Nada;
						if (OnNone != null)
							OnNone (m_touchType);
					}
					break;
				case 1:
					if (m_touchType.isActive) {
						m_touchType.typeOfTouch = ToucheTouchType.Kind.Touch;
						if (OnTouch != null)
							OnTouch (m_touchType);
					}
					break;
				case 2:
					if (m_touchType.isActive) {
						m_touchType.typeOfTouch = ToucheTouchType.Kind.Grab;
						if (OnGrab != null)
							OnGrab (m_touchType);
					}
					break;
				case 3:
					if (m_touchType.isActive) {
						m_touchType.typeOfTouch = ToucheTouchType.Kind.InWater;
						if (OnInWater != null)
							OnInWater (m_touchType);
					}
					break;
				default :
					if (m_touchType.isActive) {
						m_touchType.typeOfTouch = ToucheTouchType.Kind.Nada;
						if (OnNone != null)
							OnNone (m_touchType);
					}
					break;
				}
			}
		}

	}

	public class ToucheTouchType
	{
		public enum Kind
		{
			//Using 'None' as a name has issues...
			Nada = 0,
			Touch = 1,
			Grab = 2,
			InWater = 3
		}

		public Kind typeOfTouch = Kind.Nada;
		public float amount = 0;
		public bool isActive = false;
	}

}
