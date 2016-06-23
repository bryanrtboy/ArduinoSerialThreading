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

public class UI : MonoBehaviour
{

	public ByteReciever m_receiver;
	public Image[] m_buttonImages;
	//These should be attached to Buttons Objects that are named - None, Touch, Grab, InWater. Buttons should send their position when pressed to SetButtonMaxPosition

	Vector2[] gesturePoints = new Vector2[4];
	float[] gestureDist = new float [4];
	Vector2 m_lastMaxYposition = Vector2.zero;

	void Awake ()
	{
		for (int i = 0; i < m_buttonImages.Length; i++) {
			if (PlayerPrefs.HasKey (m_buttonImages [i].gameObject.name + "x") && PlayerPrefs.HasKey (m_buttonImages [i].gameObject.name + "y")) {
				float x = PlayerPrefs.GetFloat (m_buttonImages [i].gameObject.name + "x");	
				float y = PlayerPrefs.GetFloat (m_buttonImages [i].gameObject.name + "y");
				gesturePoints [i] = new Vector2 (x, y);
			}
		}

	}

	void OnEnable ()
	{
		m_receiver.OnByteReceived += GestureCompare;
	}

	void OnDisable ()
	{
		m_receiver.OnByteReceived -= GestureCompare;
	}

	public void SetButtonMaxPosition (int buttonIndex)
	{
		gesturePoints [buttonIndex] = m_lastMaxYposition;
		PlayerPrefs.SetFloat (m_buttonImages [buttonIndex].gameObject.name + "x", m_lastMaxYposition.x);
		PlayerPrefs.SetFloat (m_buttonImages [buttonIndex].gameObject.name + "y", m_lastMaxYposition.y);
	}

	void GestureCompare (Vector2[] positions, int maxYposition)
	{
		m_lastMaxYposition = positions [maxYposition];

		float totalDist = 0;
		int currentMax = 0;
		float currentMaxValue = -1;
		for (int i = 0; i < m_buttonImages.Length; i++) {
			//Store the maxY so that if a button is pressed, we will enter this value for that button.
			m_lastMaxYposition = positions [maxYposition];

			//Calculate the distance for this button, and put it in an array of distances for all of the buttons
			gestureDist [i] = Vector2.Distance (m_lastMaxYposition, gesturePoints [i]);
			totalDist += gestureDist [i];
			if (gestureDist [i] < currentMaxValue || i == 0) {
				currentMax = i;
				currentMaxValue = gestureDist [i];
			}
		}

		totalDist = totalDist / 3;

		for (int i = 0; i < m_buttonImages.Length; i++) {
			float currentAmmount = 0;
			currentAmmount = 1 - gestureDist [i] / totalDist;

			if (currentMax == i) {
				m_buttonImages [i].color = Color.red;
				m_buttonImages [i].fillAmount = currentAmmount;
			} else {
				m_buttonImages [i].color = Color.white;
				m_buttonImages [i].fillAmount = 1;
			}
		}
	}
}