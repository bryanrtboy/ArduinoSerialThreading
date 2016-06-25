using UnityEngine;
using System;
using System.Collections;

namespace ArduinoSerialReader
{
	public class TouchDetector : MonoBehaviour
	{
		public static TouchDetector instance;

		public event Action<ToucheTouch.Type> OnNewTouchDetected;
		public event Action<ToucheTouch[]> OnTouch;

		public ByteReciever m_receiver;
		//These should be attached to Buttons Objects that are named - None, Touch, Grab, InWater. Buttons should send their position when pressed to SetButtonMaxPosition

		Vector2[] m_storedGesturePoints = new Vector2[4];
		float[] m_gestureDistances = new float [4];
		Vector2 m_lastMaxYposition = Vector2.zero;
		ToucheTouch m_touchType;
		int m_touchTypeCount;
		[HideInInspector]
		public ToucheTouch.Type m_currentTouchType = ToucheTouch.Type.Nada;

		void Awake ()
		{
			//Check if instance already exists
			if (instance == null)
				//if not, set instance to this
				instance = this;
			//If instance already exists and it's not this:
			else if (instance != this)
				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a TouchDetector.
				Destroy (gameObject); 

			m_touchTypeCount = System.Enum.GetValues (typeof(ToucheTouch.Type)).Length;
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
				ToucheTouch.Type k = (ToucheTouch.Type)i;
				if (PlayerPrefs.HasKey (k.ToString () + "x") && PlayerPrefs.HasKey (k.ToString () + "y")) {
					float x = PlayerPrefs.GetFloat (k.ToString () + "x");	
					float y = PlayerPrefs.GetFloat (k.ToString () + "y");
					m_storedGesturePoints [i] = new Vector2 (x, y);
				}
			}
		}

		public void SetMaxPosition (int buttonIndex)
		{

			ToucheTouch.Type k = (ToucheTouch.Type)buttonIndex;
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
			float currentAmmount = 0;

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

			ToucheTouch[] touches = new ToucheTouch[m_touchTypeCount];
			for (int i = 0; i < m_touchTypeCount; i++) {
				currentAmmount = 0;
				currentAmmount = 1 - m_gestureDistances [i] / totalDist; //How much of the button is filled (strength of signal)

				m_touchType = new ToucheTouch ();
				m_touchType.type = (ToucheTouch.Type)i;
				m_touchType.amount = currentAmmount;

				touches [i] = m_touchType;
				if (currentMax == i) {
					ChangeTouchType (m_touchType);
				}
			}

			if (OnTouch != null)
				OnTouch (touches);  //Send a batch of touches so that we can visualize how strong the matching touch is compared to neighbors
		}

		public void  ChangeTouchType (ToucheTouch touch)
		{
			m_currentTouchType = touch.type;
			if (OnNewTouchDetected != null)
				OnNewTouchDetected (m_currentTouchType);
		}

	}

	public class ToucheTouch
	{
		public enum Type
		{
			//Using 'None' as a name has issues...
			Nada = 0,
			Touch = 1,
			Grab = 2,
			InWater = 3
		}

		public Type type = Type.Nada;
		public float amount = 0;
	}

}
