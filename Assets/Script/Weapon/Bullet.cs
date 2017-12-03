﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour 
{
	private static Collider[] nonAlloc = new Collider[10];

	public float speed = 100.0f;
	public float damage = 10.0f;
	public float lifeDuration = 4.0f;
	public float damageRadius = 0.0f;

	[System.NonSerialized]
	public Ammunition ammunition;

	private Rigidbody body;
	private Transform trans;



	public void SetData( Vector3 position, Vector3 direction )
	{
		if( body == null ) 
			this.RecoveryCache ();
		
		trans.position = position;
		trans.rotation = Quaternion.LookRotation (direction, Vector3.up);
	}

	private void Awake()
	{
		this.RecoveryCache ();
	}

	private void RecoveryCache()
	{
		body = GetComponent<Rigidbody> ();
		trans = GetComponent<Transform> ();
	}

	private void OnEnable()
	{
		StartCoroutine ("WaitingLifeTime");
	}

	private void FixedUpdate()
	{
		body.velocity =  trans.forward * speed * Time.deltaTime;
	}

	private void OnCollisionEnter( Collision other )
	{
		VitalityComponent vitality = other.gameObject.GetComponent<VitalityComponent> ();
		if (vitality != null) 
			vitality.TakeDamage (damage, other.contacts[0].point);

		if (damageRadius > 0.001f) 
		{
			int amount = Physics.OverlapSphereNonAlloc (this.trans.position, damageRadius, nonAlloc);
			for (int i = 0; i < amount; i++) 
			{
				VitalityComponent vit = nonAlloc [i].gameObject.GetComponent<VitalityComponent> ();
				if (vit) 
					vit.TakeDamage (
						damage * Vector3.Distance (this.trans.position, vit.transform.position) / damageRadius,
						this.trans.position
					);
			}
		}

		if (ammunition != null) ammunition.Recycle (this);
	}

	private IEnumerator WaitingLifeTime()
	{
		yield return new WaitForSeconds (lifeDuration);

		if (ammunition != null) ammunition.Recycle (this);
	}

}