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
		public LineGraph m_lineGraph;
		public int m_steps = 160;

		[HideInInspector]
		public ToucheTouch.Type m_currentTouchType = ToucheTouch.Type.None;
		//This is set to a different type to force an evaluation to fire an initial TouchOn/off events
		ToucheTouch.Type m_lastTouchType = ToucheTouch.Type.Pinch;
		Dictionary<int,ToucheTouch> m_toucheCurves;
		Vector2[] m_currentCurve;
		CurveData data;

		//Number of points in our curve on the X axis


		void Awake ()
		{
			//Check if instance already exists
			if (instance == null) {
				//if not, set instance to this
//				Debug.Log ("Instance is null at " + Time.time);
				instance = this;
				//If instance already exists and it's not this:
			} else if (instance != this) {
				Debug.Log ("Instance does not equal this at " + Time.time);
				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a TouchDetector.
				Destroy (gameObject); 
			}
		}

		void OnEnable ()
		{
			
			m_receiver.OnByteReceived += SetCurrentPositions;
			LoadData ();

		}

		void OnDisable ()
		{
			
			m_receiver.OnByteReceived -= SetCurrentPositions;
			SaveData ();
		}

		public void SetToucheCurveTo (int type)
		{
			if (m_toucheCurves == null)
				m_toucheCurves = new Dictionary<int, ToucheTouch> ();
			
			ToucheTouch touche = new ToucheTouch ((ToucheTouch.Type)type, Vector2ToCurvePositions (m_currentCurve), 0);

			if (m_toucheCurves.ContainsKey (type)) {
				m_toucheCurves [type] = touche;
			} else {
				m_toucheCurves.Add (type, touche);
			}

			SaveData ();
			if (m_lineGraph)
				m_lineGraph.CreateFixedCurves (data);
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

				int vCount = storedVector2s.Length;
				int currCount = m_currentCurve.Length;
				if (currCount < vCount)
					vCount = currCount;

				if (vCount == 0)
					return; //Stop if we have no vectors...
				
				for (int j = 0; j < m_steps; j++) {
					if (j < vCount) {
						float temp = Vector2.Distance (storedVector2s [j], m_currentCurve [j]);
						distance += (int)(temp * temp);//square to exxagerate differences
					}
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
			data = new CurveData (m_toucheCurves);
			bf.Serialize (file, data);
			file.Close ();
		}

		public void LoadData ()
		{
			if (File.Exists (Application.persistentDataPath + "/savedCurves.dat")) {
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Open (Application.persistentDataPath + "/savedCurves.dat", FileMode.Open);

				data = (CurveData)bf.Deserialize (file);
				file.Close ();

				m_toucheCurves = data.toucheCurves;

				if (m_lineGraph)
					m_lineGraph.CreateFixedCurves (data);
			}
		}

		#endregion

		#region Utilities

		Vector2[] CurvePositionsToVector2 (CurvePositions[] curvePositions)
		{
			if (curvePositions == null)
				return null;

			int length = curvePositions.Length;
			Vector2[] pos = new Vector2[length];
			for (int i = 0; i < length; i++) {
				pos [i] = new Vector2 (curvePositions [i].x, curvePositions [i].y);
			}
			return pos;
		}

		CurvePositions[] Vector2ToCurvePositions (Vector2[] positions)
		{
			if (positions == null)
				return null;

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

		public CurveData (Dictionary<int,ToucheTouch> _toucheCurves)
		{
			toucheCurves = _toucheCurves;
		}
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
			None = 0,
			Pinch = 1,
			Grab = 2,
			In_Water = 3
		}

		public Type type;
		public CurvePositions[] curve;
		//Would prefer to use Vector2's, but they are not serializable...
		public int distance;

		public ToucheTouch (Type _type, CurvePositions[] _curve, int _distance)
		{
			type = _type;
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
