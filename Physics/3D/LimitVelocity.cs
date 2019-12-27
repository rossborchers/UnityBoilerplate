using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitVelocity : MonoBehaviour 
{

	[SerializeField]
	protected float limit = Mathf.Infinity;

	[SerializeField]
	protected Vector3 weight = Vector3.one;

	protected Rigidbody rigidbody;

	protected void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
		if(rigidbody == null)
		{
			Debug.Log("Rigidbody is null in LimitVelocity, make sure a limited gameobject has a Rigidbody component.");
		}
	}

	protected void FixedUpdate () 
	{
		if(rigidbody == null) return;

		float velocityMag = new Vector3(rigidbody.velocity.x * weight.x, rigidbody.velocity.y * weight.y, rigidbody.velocity.z * weight.z).magnitude;
		Vector3 veloictyNorm = rigidbody.velocity.normalized;
		if(velocityMag > limit)
		{
			rigidbody.velocity = veloictyNorm * limit ;
		}
	}
}