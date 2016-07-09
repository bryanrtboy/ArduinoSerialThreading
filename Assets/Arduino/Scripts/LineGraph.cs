using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ArduinoSerialReader;

public class LineGraph : MonoBehaviour
{

	public Material m_lineMaterial;
	public Font	m_font;
	public float m_width = 8f;
	public float m_yMultiplier = 0.1f;
	public Color[] m_lineColors = new Color[]{ Color.cyan, Color.red, Color.yellow, Color.green };

	float xIncrement = 0;
	List<GameObject> m_curves;

	public void CreateFixedCurves (CurveData data)
	{
		if (m_curves != null) {
			foreach (GameObject g in m_curves)
				Destroy (g);
			m_curves.Clear ();
		} else {
			m_curves = new List<GameObject> ();
		}



		int count = 0;
		foreach (int key in data.toucheCurves.Keys) {

			Color c = m_lineColors [count];
			count++;
			if (count > m_lineColors.Length)
				count = 0;
			
			GameObject parent = new GameObject (data.toucheCurves [key].type.ToString ());
			m_curves.Add (parent);

			LineRenderer lrenderer = parent.AddComponent<LineRenderer> () as LineRenderer;
			if (m_lineMaterial) {
				lrenderer.material = m_lineMaterial;
				lrenderer.SetColors (c, c);
			}
			lrenderer.SetWidth (.05f, .05f);
			lrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			TextMesh txt = parent.AddComponent<TextMesh> () as TextMesh;
			txt.text = parent.name + "\no";
			if (m_font) {
				txt.font = m_font;
				MeshRenderer m = txt.gameObject.GetComponent<MeshRenderer> () as MeshRenderer;
				m.material = m_font.material;
			}
			txt.characterSize = .25f;
			txt.fontSize = 14;
			txt.color = c;
			txt.alignment = TextAlignment.Center;
			txt.anchor = TextAnchor.LowerCenter;

			if (data.toucheCurves [key].curve == null)
				return;

			int length = data.toucheCurves [key].curve.Length;
			xIncrement = m_width / length;
			Vector3[] linePositions = new Vector3[length];
			float maxY = 0;
			float xPos = 0;
			for (int i = 0; i < length; i++) {
				linePositions [i] = new Vector3 (transform.position.x + (i * xIncrement), transform.position.y + ((data.toucheCurves [key].curve [i].y) * m_yMultiplier), transform.position.z);
				if (linePositions [i].y > maxY) {
					maxY = linePositions [i].y;
					xPos = linePositions [i].x;
				}
			}
			lrenderer.SetVertexCount (linePositions.Length);
			lrenderer.SetPositions (linePositions);

//			if (count % 2 == 0)
//				maxY += .5f;
//			else
//				maxY += .25f;

			txt.transform.position = new Vector3 (xPos, maxY - .1f, txt.transform.position.z);
		}

	}
}
