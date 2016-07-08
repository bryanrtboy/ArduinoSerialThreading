using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ArduinoSerialReader
{
	public class TouchDetector : MonoBehaviour
	{
		public static TouchDetector instance;

		public event Action<ToucheTouch.Type> TouchOn;
		public event Action<ToucheTouch.Type> TouchOff;

		public ByteReciever m_receiver;
		//These should be attached to Buttons Objects that are named - None, Touch, Grab, InWater. Buttons should send their position when pressed to SetButtonMaxPosition

		[HideInInspector]
		public ToucheTouch.Type m_currentTouchType = ToucheTouch.Type.Nada;
		//This is set to a different type to force an evaluation to fire an initial TouchOn/off events
		ToucheTouch.Type m_lastTouchType = ToucheTouch.Type.Touch;
		Dictionary<int,ToucheTouch> m_toucheCurves;
		Vector2[] m_currentCurve;


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
		}

		void OnEnable ()
		{
			LoadData ();
			m_receiver.OnByteReceived += SetCurrentPositions;

		}

		void OnDisable ()
		{
			SaveData ();
			m_receiver.OnByteReceived -= SetCurrentPositions;
		}

		public void SetToucheCurveTo (int type)
		{
			if (m_toucheCurves == null)
				m_toucheCurves = new Dictionary<int, ToucheTouch> ();
			
			ToucheTouch touche = new ToucheTouch ((ToucheTouch.Type)type, 0, Vector2ToCurvePositions (m_currentCurve), 0);

			if (m_toucheCurves.ContainsKey (type)) {
				m_toucheCurves [type] = touche;
			} else {
				m_toucheCurves.Add (type, touche);
			}
		}

		void SetCurrentPositions (Vector2[] positions)
		{
			m_currentCurve = positions;
			CompareCurrentCurveToSavedCurves ();
		}

		void CompareCurrentCurveToSavedCurves ()
		{

			if (m_toucheCurves == null || m_currentCurve == null)
				return;

			int storedToucheCurveCount = m_toucheCurves.Count;
		
			List<ToucheTouch> toucheList = new List<ToucheTouch> ();

			foreach (int key in m_toucheCurves.Keys) {
				int distance = 0;
				Vector2[] storedVector2s = CurvePositionsToVector2 (m_toucheCurves [key].curve);
				int steps = m_toucheCurves [key].curve.Length - 1; //We set this here in case we are missing a packet or two
				if (steps > m_currentCurve.Length - 1)
					steps = m_currentCurve.Length - 1;
				
				for (int j = 0; j < steps; j++) {
					float temp = Vector2.Distance (storedVector2s [j], m_currentCurve [j]);
					distance += (int)(temp * temp);//square to exxagerate differences
				}

				m_toucheCurves [key].distance = distance;
				toucheList.Add (m_toucheCurves [key]);
			}
				
			toucheList.Sort (); //sorted by distance

			m_currentTouchType = toucheList [0].type; //lowest number wins...

			if (m_lastTouchType != m_currentTouchType) {
				
				for (int i = 0; i < toucheList.Count; i++) {
					if (i == 0) {
						if (TouchOn != null)
							TouchOn (m_currentTouchType);
					} else {
						if (TouchOff != null)
							TouchOff (toucheList [i].type);
					}
				}
			}
				
			m_lastTouchType = m_currentTouchType;
		}

		#region Saving Persistent Data

		public void SaveData ()
		{
			if (m_toucheCurves == null)
				return;
			
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (Application.persistentDataPath + "/savedCurves.dat");

			CurveData data = new CurveData ();
			data.toucheCurves = m_toucheCurves;

			bf.Serialize (file, data);
			file.Close ();
		}

		public void LoadData ()
		{
			if (File.Exists (Application.persistentDataPath + "/savedCurves.dat")) {
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Open (Application.persistentDataPath + "/savedCurves.dat", FileMode.Open);

				CurveData data = (CurveData)bf.Deserialize (file);
				file.Close ();

				m_toucheCurves = data.toucheCurves;
			}
		}

		#endregion

		#region Utilities

		Vector2[] CurvePositionsToVector2 (CurvePositions[] curvePositions)
		{
			int length = curvePositions.Length;
			Vector2[] pos = new Vector2[length];
			for (int i = 0; i < length; i++) {
				pos [i] = new Vector2 (curvePositions [i].x, curvePositions [i].y);
			}
			return pos;
		}

		CurvePositions[] Vector2ToCurvePositions (Vector2[] positions)
		{
			int length = positions.Length;
			CurvePositions[] pos = new CurvePositions[length];
			for (int i = 0; i < length; i++) {
				pos [i] = new CurvePositions (positions [i].x, positions [i].y);
			}
			return pos;
		}

		#endregion
	}

	#region Custom Classes
	[Serializable]
	public class CurveData
	{
		public Dictionary<int,ToucheTouch> toucheCurves;
	}

	[Serializable]
	public class CurvePositions
	{
		public float x;
		public float y;

		public CurvePositions (float _x, float _y)
		{
			x = _x;
			y = _y;
		}
	}

	[Serializable]
	public class ToucheTouch : IComparable<ToucheTouch>
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
		public CurvePositions[] curve;
		//Would prefer to use Vector2's, but they are not serializable...
		public int distance = 0;

		public ToucheTouch (Type _type, float _amount, CurvePositions[] _curve, int _distance)
		{
			type = _type;
			amount = _amount;
			curve = _curve;
			distance = _distance;
		}

		public int CompareTo (ToucheTouch other) //Sort based on distance
		{
			if (this.distance < other.distance)
				return -1;
			else if (this.distance > other.distance)
				return 1;
			else
				return 0;
		}

		#endregion

	}
}
