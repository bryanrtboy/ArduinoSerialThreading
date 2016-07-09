//Bryan Leister June 2016
//
/// <summary>
/// /*Simple implementation of checking when gesture is being made based on MaxYValues.
/// TODO: Implement Dynamic Time Warping to check curve for more accurate results...
/// */
/// </summary>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ArduinoSerialReader
{
	public class UI : MonoBehaviour
	{
		
		public HorizontalLayoutGroup m_buttonHolder;
		public GameObject m_buttonPrefab;

		Dictionary<ToucheTouch.Type,Image> m_buttonImages;


		void Start ()
		{
			if (TouchDetector.instance == null) {
				Debug.LogError ("No Touch Detector in Scene!");
				Destroy (this);
				return;
			}

			if (m_buttonHolder == null) {
				Debug.LogError ("No Place to put the buttons! Please add a UI Panel with a Horizontal Layout Group to the scene");
				Destroy (this);
				return;
			}
	
			m_buttonImages = new Dictionary<ToucheTouch.Type,Image> ();
			foreach (ToucheTouch.Type value in System.Enum.GetValues(typeof(ToucheTouch.Type))) {
				GameObject g = Instantiate (m_buttonPrefab, m_buttonHolder.transform.position, Quaternion.identity) as GameObject;
				g.name = value.ToString ();
				g.transform.parent = m_buttonHolder.transform;
				g.transform.localScale = Vector3.one;
				Button b = g.GetComponent<Button> () as Button;

				Text label = g.GetComponentInChildren<Text> () as Text;
				label.text = g.name;
				Image img = g.GetComponent<Image> () as Image;
				m_buttonImages.Add (value, img);

				AddListener (b, (int)value);
				Debug.Log (value);
			}

		}

		void AddListener (Button b, int value)
		{
			b.onClick.AddListener (() => TouchDetector.instance.SetToucheCurveTo (value));
		}

		void OnEnable ()
		{
			if (TouchDetector.instance == null)
				Debug.LogError ("No Touch Detector in Scene! Use the Script Execution order to insure UI is loaded after TouchDetector.");

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
			m_buttonImages [type].color = Color.red;
		}

		void HandleTouchOff (ToucheTouch.Type type)
		{
			m_buttonImages [type].color = Color.grey;
		}
			
	}
}