using UnityEngine;
using System;
using System.Collections;

public class TouchDetector : MonoBehaviour
{

	public event Action<ToucheTouchType> OnTouchReceived;
	public event Action<ToucheTouchType> OnNewTouchReceived;

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
			currentAmmount = 1 - m_gestureDistances [i] / totalDist;
			ToucheTouchType.Kind k = (ToucheTouchType.Kind)i;
			if (currentMax == i) {
				//				m_buttonImages [i].color = Color.red;
				//				m_buttonImages [i].fillAmount = currentAmmount;

				m_touchType = new ToucheTouchType ();
				m_touchType.typeOfTouch = k;
				m_touchType.amount = currentAmmount;
				if (OnNewTouchReceived != null)
					OnNewTouchReceived (m_touchType);
	
			} else {
				//				m_buttonImages [i].color = Color.white;
				//				m_buttonImages [i].fillAmount = 1;
				//m_touchType = new ToucheTouchType (k, 1f);
				m_touchType = new ToucheTouchType ();
				m_touchType.typeOfTouch = k;
				m_touchType.amount = 1f;

				if (OnTouchReceived != null)
					OnTouchReceived (m_touchType);
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
		Water = 3
	}

	public Kind typeOfTouch = Kind.Nada;
	public float amount = 0;
}
