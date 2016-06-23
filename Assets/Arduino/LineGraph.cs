using UnityEngine;
using System.Collections;


public class LineGraph : MonoBehaviour
{

	public SerialReader m_serialReader;
	public Transform m_node;
	public float m_width = 100f;
	public float yIncrement = 0.1f;
	public int m_nodeCount = 16;

	[HideInInspector]
	public float[] ThreadSafeYValues;

	Transform[] m_nodes;
	bool isUpdating = false;
	int yMinimum = 10000;
	int yMaximum = 0;
	int mean = 0;
	float xIncrement = 0;


	void Awake ()
	{

		float updateSpeed = m_serialReader.m_updateRate;
		m_nodes = new Transform[m_nodeCount];
		xIncrement = m_width / m_nodeCount;
		for (int i = 0; i < m_nodeCount; i++) {
			m_nodes [i] = Instantiate (m_node, transform.position, Quaternion.identity) as Transform;
			m_nodes [i].position = new Vector3 (transform.position.x + (i * xIncrement), transform.position.y, transform.position.z);
		}

		//InvokeRepeating ("UpdatePositions", 1, m_serialReader.m_updateRate);
	}

	void Update ()
	{
		//UpdatePositions ();
		if (isUpdating)
			UpdatePositions ();
	}

	public void UpdateYPositions (float[] positions)
	{
		ThreadSafeYValues = positions;
		isUpdating = true;
	}


	public void UpdatePositions ()
	{
		isUpdating = false;

		if (ThreadSafeYValues == null)
			return;
		
		float[] tempArray = ThreadSafeYValues;

//		if (tempArray.Length > m_nodeCount)
//			Debug.Log ("Incoming value length is " + tempArray.Length);

		for (int i = 0; i < tempArray.Length; i++) {

			if (i >= m_nodeCount) {
				//Debug.Log ("Stopping...");
				return;
			}

			int temp = (int)tempArray [i];

//			if (temp <= 0)
//				continue;
			
			if (temp < yMinimum && i != 0)  //offset by this amount
				yMinimum = temp;
			if (temp > yMaximum)
				yMaximum = temp;
		
			mean = yMaximum - yMinimum;

			//Debug.Log ("mean: " + mean + " yMin: " + yMinimum + " yMax: " + yMaximum + " value: " + temp + " yIncrement: " + yIncrement);
			float f = (temp - yMinimum) * yIncrement;
			if (f <= 10000)
				m_nodes [i].transform.position = new Vector3 (m_nodes [i].transform.position.x, f, m_nodes [i].transform.position.z);

		}
	}

}
