using UnityEngine;
using System.Collections;
using ArduinoSerialReader;


public class DotGraph : MonoBehaviour
{

	public ByteReciever m_receiver;
	public Transform m_node;
	public float m_width = 8f;
	public float m_yMultiplier = 0.1f;
	public int m_nodeCount = 160;
	//Should match the Arduino Touche frequency count

	Transform[] m_nodes;
	float xIncrement = 0;

	void Awake ()
	{
		m_nodes = new Transform[m_nodeCount];
		xIncrement = m_width / m_nodeCount;
		for (int i = 0; i < m_nodeCount; i++) {
			m_nodes [i] = Instantiate (m_node, transform.position, Quaternion.identity) as Transform;
			m_nodes [i].position = new Vector3 (transform.position.x + (i * xIncrement), transform.position.y, transform.position.z);
			m_nodes [i].name = "Node " + i.ToString ();
		}
	}

	void OnEnable ()
	{
		m_receiver.OnByteReceived += UpdatePositions;
	}

	void OnDisable ()
	{
		m_receiver.OnByteReceived -= UpdatePositions;
	}

	public void UpdatePositions (Vector2[] positions, int maxYposition)
	{
		int count = positions.Length;

		for (int i = 0; i < count; i++) {
			if (i > m_nodeCount) {
				Debug.Log ("Stopping, not enough nodes for the positions...");
				return;
			}
			m_nodes [i].transform.position = new Vector3 (m_nodes [i].transform.position.x, transform.position.y + (positions [i].y * m_yMultiplier), transform.position.z);
			if (i == maxYposition)
				m_nodes [i].localScale = new Vector3 (2, 2, 2);
			else
				m_nodes [i].localScale = new Vector3 (1, 1, 1);
		}
	}

}
