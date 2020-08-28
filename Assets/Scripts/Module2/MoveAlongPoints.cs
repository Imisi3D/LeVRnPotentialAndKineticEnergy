using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveAlongPoints : CustomComponent
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

	// movement along these points is never stopped.
	public GameObject[] destinationPoints;

	// movement along these points can be stopped.
	public List<DestinationPoint> points;

	// current index of the object to reach. (index in the array of points)
	public int currentDestinationIndex = 0;

	// Used for controlling character animations
	public Animator characterAnimator;

	// The animation state to set when the character is moving.
	public VoiceImageCanvasSync.AnimationState movingState;

	// The animation state to set when the character is idle.
	public VoiceImageCanvasSync.AnimationState idleState;

	// movement speed m/s
	public float speed = 5;

	// rotation speed rad/s
	public float rotationSpeed = 30f;

	public AudioSource audioSource;
	public AudioClip sound;
	public AudioClip onHitSound;


	private string idleStateParameterName = "";
	private string movingStateParameterName = "";

	private void Start()
	{
		foreach (GameObject obj in destinationPoints)
		{
			points.Add(new DestinationPoint(obj));
		}
		switch (movingState)
		{
			case VoiceImageCanvasSync.AnimationState.Walk: { movingStateParameterName = "walk"; break; }
			case VoiceImageCanvasSync.AnimationState.Run: { movingStateParameterName = "run"; break; }
			case VoiceImageCanvasSync.AnimationState.PushWheelbarrow: { movingStateParameterName = "pushWheelbarrow"; break; }
		}
		switch (idleState)
		{
			case VoiceImageCanvasSync.AnimationState.Idle: { idleStateParameterName = "idle"; break; }
			case VoiceImageCanvasSync.AnimationState.IdleWheelBarrow: { idleStateParameterName = "idleWheelBarrow"; break; }
		}
	}

	void Update()
	{
		DestinationPoint point = points[currentDestinationIndex];
		GameObject targetObject = point.target;
		Vector3 relativePos = targetObject.transform.position - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation(relativePos, transform.up);
		float d = (transform.position - targetObject.transform.position).magnitude;

		if (d < 0.5f)
		{
			//transform.position += transform.forward * speed * Time.deltaTime / 20;
			if (point.mustInheritRotation)
				targetRotation = point.target.transform.rotation;
			print("close");
		}
		else
		{
			transform.position += transform.forward * speed * Time.deltaTime;
			characterAnimator.SetBool(idleStateParameterName, false);
			characterAnimator.SetBool(movingStateParameterName, true);

			print("far");
		}
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

		Vector3 distance = gameObject.transform.position - targetObject.transform.position;
		float dist = distance.magnitude;

		// point reached and either no need to inherit rotation or target's rotation is applied to parent.
		if (dist < 0.8 && (!point.mustInheritRotation || (point.mustInheritRotation && Quaternion.Angle(transform.rotation, point.target.transform.rotation) < 10.0f)))
		{
			if (currentDestinationIndex < points.Count - 1)
			{
				if (points[currentDestinationIndex++].shouldStop)
				{
					enabled = false;
					characterAnimator.SetBool(idleStateParameterName, true);
					characterAnimator.SetBool(movingStateParameterName, false);
				}
			}
			else
			{
				characterAnimator.SetBool(idleStateParameterName, true);
				characterAnimator.SetBool(movingStateParameterName, false);
				enabled = false;
			}
		}
	}

	private void OnDisable()
	{
		if (audioSource != null)
		{
			audioSource.Stop();
			if (onHitSound != null)
				audioSource.PlayOneShot(onHitSound);
		}

	}
	private void OnEnable()
	{
		if (audioSource != null && sound != null)
		{
			audioSource.clip = sound;
			audioSource.Play();
		}
	}
}
