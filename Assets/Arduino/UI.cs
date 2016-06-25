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

	public TouchDetector m_touchReceiver;
	public Image[] m_buttonImages;
	//Needs to match number of TouchType.Kind = 4 and match sort order

	void OnEnable ()
	{
		m_touchReceiver.OnNewTouchReceived += HandleNewTouch;
		m_touchReceiver.OnTouchReceived += HandleTouch;
	}

	void OnDisable ()
	{
		m_touchReceiver.OnNewTouchReceived -= HandleNewTouch;
		m_touchReceiver.OnTouchReceived -= HandleTouch;
	}

	void HandleNewTouch (ToucheTouchType touch)
	{
		int i = (int)touch.typeOfTouch;
		m_buttonImages [i].color = Color.red;
		m_buttonImages [i].fillAmount = touch.amount;
	}

	void HandleTouch (ToucheTouchType touch)
	{
		int i = (int)touch.typeOfTouch;
		m_buttonImages [i].color = Color.white;
		m_buttonImages [i].fillAmount = 1;
	}

	//	void GestureCompare (Vector2[] positions, int maxYposition)
	//	{
	////		m_lastMaxYposition = positions [maxYposition];
	////
	////		float totalDist = 0;
	////		int currentMax = 0;
	////		float currentMaxValue = -1;
	////		for (int i = 0; i < m_buttonImages.Length; i++) {
	////			//Store the maxY so that if a button is pressed, we will enter this value for that button.
	////			m_lastMaxYposition = positions [maxYposition];
	////
	////			//Calculate the distance for this button, and put it in an array of distances for all of the buttons
	////			gestureDist [i] = Vector2.Distance (m_lastMaxYposition, gesturePoints [i]);
	////			totalDist += gestureDist [i];
	////			if (gestureDist [i] < currentMaxValue || i == 0) {
	////				currentMax = i;
	////				currentMaxValue = gestureDist [i];
	////			}
	////		}
	////
	////		totalDist = totalDist / 3;
	////
	////		for (int i = 0; i < m_buttonImages.Length; i++) {
	////			float currentAmmount = 0;
	////			currentAmmount = 1 - gestureDist [i] / totalDist;
	////
	////			if (currentMax == i) {
	////				m_buttonImages [i].color = Color.red;
	////				m_buttonImages [i].fillAmount = currentAmmount;
	////			} else {
	////				m_buttonImages [i].color = Color.white;
	////				m_buttonImages [i].fillAmount = 1;
	////			}
	////		}
	//	}
}