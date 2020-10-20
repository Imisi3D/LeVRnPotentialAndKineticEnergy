using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DestinationPoint
{
	// transform to reach
	public GameObject target;

	/* if set to true, then object's movement will stop when it reaches this object's transform and it will resume when movement component is reactivated.
	 * stopping movement is done using disabling component
	 */
	public bool shouldStop = false;

	// if true then parent transform rotation must interpolate object's rotation.
	public bool mustInheritRotation = false;

	// If true, the script won't move the character, use this if the character is using a root motion animation.
	public bool shouldUseRootMotion = false;

	// The animation state with which the moving character should approach this point.
	public VoiceImageCanvasSync.AnimationState animationState;




}
