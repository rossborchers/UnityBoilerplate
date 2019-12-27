using UnityEngine;
using System.Collections;

public class Follow2D : MonoBehaviour {

	[SerializeField]
	protected Transform target;

	[SerializeField]
	protected Vector3 offset = new Vector3(0, 0, -10);

	[SerializeField]
	protected Vector3 lookDirection = new Vector3(0, 0 , 1);

	[SerializeField]
	protected float speed;

    [SerializeField]
    protected bool anticipate = true;

    [SerializeField]
    protected float maxAnticipateOffset = 5f;

    [SerializeField]
    protected Vector2 anticipateDirectionWeight = Vector2.one;

    [SerializeField]
	protected bool prewarm = true;

    protected Vector3 lastTargetPos;
    protected Vector3 runningOffset = Vector3.zero;

    // Use this for initialization
    protected void Start () 
	{
		if(target == null)
		{
			Debug.LogWarning("Follow2D has no target.");
			return;
		}
		if (prewarm) transform.position = target.position + offset;

		transform.forward = lookDirection;
	}
	
	// Update is called once per frame
	protected void Update ()
	{
        if (target == null) return;

        Vector3 targetPos = target.position;

        if (anticipate)
        {
            //caclulate offset 
            Vector3 frameDifference = (targetPos - lastTargetPos);
            frameDifference = new Vector3(frameDifference.x * anticipateDirectionWeight.x, frameDifference.y * anticipateDirectionWeight.y, 0);

            runningOffset += frameDifference;
            if (runningOffset.magnitude > maxAnticipateOffset)
            {
                Vector3 norm = frameDifference.normalized;
                if(norm.magnitude > 0) runningOffset = norm * maxAnticipateOffset;

            }
            lastTargetPos = target.position;

            targetPos = target.position + runningOffset;
        }

        transform.Translate((targetPos + offset - transform.position) * Time.deltaTime * speed);

        transform.forward = lookDirection;
    }


}
