using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitVelocity2D : MonoBehaviour 
{

	[SerializeField]
	protected float limit = Mathf.Infinity;

	[SerializeField]
	protected Vector2 weight = Vector2.one;

	protected Rigidbody2D rigidbody;

	protected void Start()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		if(rigidbody == null)
		{
			Debug.Log("Rigidbody2D is null in LimitVelocity2D, make sure a limited gameobject has a Rigidbody2D component.");
		}
	}

	protected void FixedUpdate () 
	{
		if(rigidbody == null) return;

		float velocityMag = new Vector2(rigidbody.velocity.x * weight.x, rigidbody.velocity.y * weight.y).magnitude;
		Vector2 veloictyNorm = rigidbody.velocity.normalized;
		if(velocityMag > limit)
		{
			rigidbody.velocity = veloictyNorm * limit;
		}
	}
}
