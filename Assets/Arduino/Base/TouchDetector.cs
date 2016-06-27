using UnityEngine;
using System;
using System.Collections;

namespace ArduinoSerialReader
{
	public class TouchDetector : MonoBehaviour
	{
		public static TouchDetector instance;

		public event Action<ToucheTouch.Type> TouchOn;
		public event Action<ToucheTouch.Type> TouchOff;
		public event Action<ToucheTouch[]> OnTouchAllTouches;

		public ByteReciever m_receiver;
		//These should be attached to Buttons Objects that are named - None, Touch, Grab, InWater. Buttons should send their position when pressed to SetButtonMaxPosition

		Vector2[] m_storedGesturePoints = new Vector2[4];
		float[] m_gestureDistances = new float [4];
		Vector2 m_lastMaxYposition = Vector2.zero;
		ToucheTouch m_touche;
		int m_toucheCount;
		[HideInInspector]
		public ToucheTouch.Type m_currentTouchType = ToucheTouch.Type.Nada;
		//This is set to a different type to force an evaluation to fire an initial TouchOn event
		ToucheTouch.Type m_lastTouchType = ToucheTouch.Type.Touch;


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

			m_toucheCount = System.Enum.GetValues (typeof(ToucheTouch.Type)).Length;
			m_storedGesturePoints = new Vector2[m_toucheCount];
			m_gestureDistances = new float [m_toucheCount];
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
			for (int i = 0; i < m_toucheCount; i++) {
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

			for (int i = 0; i < m_toucheCount; i++) {
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

			ToucheTouch[] touches = new ToucheTouch[m_toucheCount];
			for (int i = 0; i < m_toucheCount; i++) {
				currentAmmount = 0;
				currentAmmount = 1 - m_gestureDistances [i] / totalDist; //How much of the button is filled (strength of signal)

				m_touche = new ToucheTouch ();
				m_touche.type = (ToucheTouch.Type)i;
				m_touche.amount = currentAmmount;

				touches [i] = m_touche;

				if (currentMax == i)
					m_currentTouchType = m_touche.type;

				if (m_lastTouchType != m_currentTouchType) { //Don't send events repeatedly if the touch type has not changed
					if (m_touche.type != m_currentTouchType) { //Send Off to all that are not the current touchtype
						if (TouchOff != null)
							TouchOff (m_touche.type);
					} else {
					
						if (TouchOn != null)
							TouchOn (m_currentTouchType);
					} 
				}
			}

			if (OnTouchAllTouches != null)
				OnTouchAllTouches (touches);  //Send a batch of touches so that we can visualize how strong the matching touch is compared to neighbors
		
			m_lastTouchType = m_currentTouchType; //All touches are analyzed, set last touch to this touch
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
