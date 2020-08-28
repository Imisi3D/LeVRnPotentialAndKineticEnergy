using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectAlongPoints : CustomComponent
{
	[System.Serializable]
	public class DestinationPoint
	{
		// holder of transform to reach
		public GameObject target;
		/* if set to true, then object's movement will stop when it reaches this object's transform and it will resume when movement component is reactivated.
		 * stopping movement is done using disabling component
		 */
		public bool shouldStop = false;

		// if true then parent transform rotation must interpolate object's rotation.
		public bool mustInheritRotation = false;

		public DestinationPoint(GameObject target, bool mustInheritRotation = false, bool shouldStop = false)
		{
			this.target = target;
			this.shouldStop = shouldStop;
			this.mustInheritRotation = mustInheritRotation;
		}
	}

	public Transform object2Move;

	public Animator characterAnimator;
	public VoiceImageCanvasSync.AnimationState movingState;
	public VoiceImageCanvasSync.AnimationState idleState;

	// movement along these points can be stopped.
	public List<DestinationPoint> points;

	// current index of the object to reach. (index in the array of points)
	public int currentDestinationIndex = 0;

	// movement speed m/s
	public float speed = 5;

	// rotation speed rad/s
	public float rotationSpeed = 30f;

	public float rotationThreshhold = 10f;

	private string idleStateParameterName = "";
	private string movingStateParameterName = "";

	private void Start()
	{
		switch (idleState)
		{
			case VoiceImageCanvasSync.AnimationState.Idle: { idleStateParameterName = "idle"; break; }
			case VoiceImageCanvasSync.AnimationState.IdleWheelBarrow: { idleStateParameterName = "idleWheelbarrow"; break; }
		}
		switch (movingState)
		{
			case VoiceImageCanvasSync.AnimationState.Walk: { movingStateParameterName = "walk"; break; }
			case VoiceImageCanvasSync.AnimationState.PushWheelbarrow: { movingStateParameterName = "pushWheelbarrow"; break; }
			case VoiceImageCanvasSync.AnimationState.Run: { movingStateParameterName = "run"; break; }
		}
	}

	void Update()
	{
		DestinationPoint point = points[currentDestinationIndex];
		GameObject targetObject = point.target;
		Vector3 relativePos = targetObject.transform.position - object2Move.position;
		Quaternion targetRotation = Quaternion.LookRotation(relativePos, object2Move.up);
		float d = (object2Move.position - targetObject.transform.position).magnitude;

		if (characterAnimator != null)
		{
			characterAnimator.SetBool(idleStateParameterName, false);
			characterAnimator.SetBool(movingStateParameterName, true);
		}
		if (d < 0.5f)
		{
			if (point.mustInheritRotation)
				targetRotation = point.target.transform.rotation;
			print("close");
		}
		else
		{
			object2Move.position += object2Move.forward * speed * Time.deltaTime;
			print("far");
		}
		object2Move.rotation = Quaternion.Lerp(object2Move.rotation, targetRotation, rotationSpeed * Time.deltaTime);

		Vector3 distance = object2Move.position - targetObject.transform.position;
		float dist = distance.magnitude;

		// point reached and either no need to inherit rotation or target's rotation is applied to parent.
		if (dist < 0.8 && (!point.mustInheritRotation || (point.mustInheritRotation && Quaternion.Angle(object2Move.rotation, point.target.transform.rotation) < rotationThreshhold)))
		{
			if (currentDestinationIndex < points.Count - 1)
			{
				if (points[currentDestinationIndex++].shouldStop)
				{
					if (characterAnimator != null)
					{
						characterAnimator.SetBool(idleStateParameterName, true);
						characterAnimator.SetBool(movingStateParameterName, false);
					}
					enabled = false;
				}
			}
			else
			{
				if (characterAnimator != null)
				{
					characterAnimator.SetBool(idleStateParameterName, true);
					characterAnimator.SetBool(movingStateParameterName, false);
				}
				enabled = false;
			}
		}
	}
}
