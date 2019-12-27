using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HumanoidFootIK : MonoBehaviour {

    [SerializeField]
	protected Transform rightFootBone;
	[SerializeField]
	protected Transform leftFootBone;

	[SerializeField]
	protected LayerMask ignoreOnRaycast;

	[SerializeField]
	protected float checkHeightOffset = 0.7f;

	[SerializeField]
	protected float footHeight = 0.1f;

	[SerializeField]
	protected bool useParamWeights = true;
	[SerializeField]
	protected string rightIKWeightParamName = "RightFootIKWeight";
	[SerializeField]
	protected string leftIKWeightParamName = "LeftFootIKWeight";

	[SerializeField]
	float leftFootWeight = 1;
	[SerializeField]
	float rightFootWeight = 1;

	[SerializeField]
	protected float checkDistance = 1f;

	[SerializeField]
	protected float resetSpeedOnFailedCheck = 1f;

	[SerializeField]
	protected bool adjustTransformToAngle = true;

	[SerializeField]
	protected float transformAdjustSpeed = 8;

  

	[SerializeField]
	protected bool debug = false;

	protected Animator animator;

	protected Stack<float> raycastPosYs = new Stack<float>();

	protected Vector3 lastIKPosRight, lastIKPosLeft;

	protected void Start () 
	{
		animator = GetComponent<Animator>();
		if(!animator) Debug.LogError("Animator is null. MechanimFootIK2D must have an animator component on gameobject.");

		lastIKPosLeft = leftFootBone.transform.position;
		lastIKPosRight = rightFootBone.transform.position;
	}

	protected void OnAnimatorIK() 
	{
		if(leftFootBone != null) CalculateFoot(leftFootBone, ref leftFootWeight, leftIKWeightParamName, AvatarIKGoal.LeftFoot, ref lastIKPosLeft);
		if(rightFootBone != null) CalculateFoot(rightFootBone, ref rightFootWeight, rightIKWeightParamName, AvatarIKGoal.RightFoot, ref lastIKPosRight);

		//adjust position so feet can touch when ground is sloped
		//needs raycasts from both CalculateFoot methods
		if(adjustTransformToAngle && raycastPosYs.Count > 0)
		{
			float pos;
			if(raycastPosYs.Count > 1) 
			{
				float pos1 = raycastPosYs.Pop(); 
				float pos2 = raycastPosYs.Pop(); 
				pos = (pos1 + pos2)/ 2f;
			}
			else // (raycastNormals.Count > 0) 
			{
				pos = raycastPosYs.Pop();
			}
			transform.position += new Vector3(0, (pos - transform.position.y) * Time.deltaTime * transformAdjustSpeed, 0);
		}
        else transform.position += new Vector3(0, (0 - transform.position.y) * Time.deltaTime * transformAdjustSpeed, 0);
    }

	protected void CalculateFoot(Transform foot, ref float footWeight, string weightParamName, AvatarIKGoal footGoal, ref Vector3 lastIKPos)
	{
		Vector3 checkPos = new Vector3(foot.position.x, transform.position.y + checkHeightOffset, foot.position.z);

        RaycastHit hitInfo;
		if (Physics.Raycast(checkPos, Vector2.down, out hitInfo, checkDistance, ~(ignoreOnRaycast)))
        {
			if(useParamWeights)
			{
				footWeight = animator.GetFloat(weightParamName);
			}

			if(adjustTransformToAngle)
			{
				raycastPosYs.Push(hitInfo.point.y);
			}

			animator.SetIKPositionWeight(footGoal, footWeight);

			Vector3 ikPos = hitInfo.point;
			animator.SetIKPosition(footGoal, new Vector3(ikPos.x, ikPos.y + footHeight, ikPos.z));
			lastIKPos = ikPos;

			// Calculate foot rotation based on raycast normal
			Vector3 toeRollLookAngle = Vector3.Cross(-foot.right, hitInfo.normal); 
			Quaternion footRotation = Quaternion.LookRotation(toeRollLookAngle, new Vector3(0,1,0));

			animator.SetIKRotationWeight(footGoal, footWeight);
			animator.SetIKRotation(footGoal, footRotation);
		}
		else
		{
			//move towards normal anim slowly
			animator.SetIKPositionWeight(footGoal, footWeight);

			animator.SetIKPosition(footGoal, new Vector3(lastIKPos.x, lastIKPos.y + footHeight, lastIKPos.z));

			//todo
			animator.SetIKRotationWeight(footGoal, 0);

			//move weight back to last pos
			footWeight -= Time.deltaTime * resetSpeedOnFailedCheck;
		}	
	
		//debug
		if(debug)
		{
			Debug.DrawRay(checkPos, Vector3.down * checkDistance, Color.grey);
		}
	}
		
}
