﻿using UnityEngine;
using System.Collections;
using ArduinoSerialReader;

[RequireComponent (typeof(ParticleSystem))]
public class ParticleGraph : MonoBehaviour
{
	public ByteReciever m_receiver;
	public float m_width = 15f;
	public float m_yMultiplier = .01f;


	ParticleSystem m_particleSystem;
	ParticleSystem.Particle[] m_particles;

	float x_increment = 1;

	void Awake ()
	{
		m_particleSystem = GetComponent<ParticleSystem> ();
	}

	void OnEnable ()
	{
		m_receiver.OnByteReceived += UpdateParticles;
	}

	void OnDisable ()
	{
		m_receiver.OnByteReceived -= UpdateParticles;
	}

	void UpdateParticles (Vector2[] positions)
	{
		int totalPositions = positions.Length;

		if (m_width > 0)
			x_increment = m_width / totalPositions;

		for (int i = 0; i < totalPositions; i++) {
			ParticleSystem.EmitParams emitOveride = new ParticleSystem.EmitParams ();
			emitOveride.position = new Vector3 (transform.position.x + (i * x_increment), transform.position.y + (positions [i].y * m_yMultiplier), transform.position.z);
			m_particleSystem.Emit (emitOveride, 1); //Emit one particle at the position
		}
	}
}
