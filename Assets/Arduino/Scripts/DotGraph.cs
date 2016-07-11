using UnityEngine;
using System.Collections;
using ArduinoSerialReader;


public class DotGraph : MonoBehaviour
{

	public ByteReciever m_receiver;
	public Transform m_node;
	public float m_width = 8f;
	public float m_yMultiplier = 0.1f;
	int m_nodeCount = 0;
	//Should match the Arduino Touche frequency count

	Transform[] m_nodes;
	float xIncrement = 0;
	bool nodesAreMade = false;

	void MakeNodes ()
	{
//		Debug.Log ("Making " + m_nodeCount + " dots.");
		m_nodes = new Transform[m_nodeCount];
		xIncrement = m_width / m_nodeCount;
		for (int i = 0; i < m_nodeCount; i++) {
			m_nodes [i] = Instantiate (m_node, transform.position, Quaternion.identity) as Transform;
			m_nodes [i].position = new Vector3 (transform.position.x + (i * xIncrement), transform.position.y, transform.position.z);
			m_nodes [i].name = "Node " + i.ToString ();
			m_nodes [i].parent = this.transform;
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

	public void UpdatePositions (Vector2[] positions)
	{
		if (!nodesAreMade) {
			m_nodeCount = TouchDetector.instance.m_steps;

			if (m_nodeCount != 0) {
				nodesAreMade = true;
				MakeNodes ();
			}
			return; //only continue if nodes are made
		}

		for (int i = 0; i < positions.Length; i++) {
			if (i >= m_nodeCount) {
				Debug.Log ("Stopping, not enough nodes for the positions...");
				return;
			}
			m_nodes [i].transform.position = new Vector3 (m_nodes [i].transform.position.x, transform.position.y + (positions [i].y * m_yMultiplier), transform.position.z);
		}
	}

}
