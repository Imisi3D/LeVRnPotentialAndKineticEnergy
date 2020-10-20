using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CharacterMovement : CustomComponent
{
    // Character object, separated for flexibility.
    public GameObject character;

    // Character's animator.
    public Animator characterAnimator;

    // List of locations to reach while moving.
    public List<DestinationPoint> destinationPoints;

    // Character's walking speed.
    public float walkingSpeed = 1.41f;

    // Character's rotation rate.
    public float turningSpeed = 1.57f;

    // Turning animation, to be used when character is turning.
    public VoiceImageCanvasSync.AnimationState rotationAnimation = VoiceImageCanvasSync.AnimationState.TurnRight180;

    // Animation state to be set when final destination is reached.
    public VoiceImageCanvasSync.AnimationState destinationReachedAnimation = VoiceImageCanvasSync.AnimationState.Idle;

    // Threshold at which the destination is considered as reached.
    public float distanceThreshhold = 0.1f;

    // Threshold at which the character should move up or down.
    public float verticalMovementThreshold = 0.3f;

    // Threshold at which character should start turning if he must inherit rotation.
    public float rotationThreshold = 0.3f;

    public int currentDestinationIndex = 0;
    private DestinationPoint currentDestinationPoint;
    private VoiceImageCanvasSync.AnimationState currentAnimationState;
    private string currentAnimationStateParamName;
    private float rootMotionAnimationEndTime = -1f;

    void Start()
    {
        currentDestinationPoint = destinationPoints[currentDestinationIndex];
        updateAnimationStateParamName(currentDestinationPoint.animationState);
    }

    void Update()
    {
        if (rootMotionAnimationEndTime > 0)
        {
            if (Time.time < rootMotionAnimationEndTime)
                return;
            else
                toNextDestination();
        }
        float distance = (currentDestinationPoint.target.transform.position - character.transform.position).magnitude;
        Vector3 characterLocation = character.transform.position;
        characterLocation.y = 0f;
        Vector3 destination = currentDestinationPoint.target.transform.position;
        destination.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(destination - characterLocation);

        if (currentDestinationPoint.mustInheritRotation && distance < rotationThreshold)
        {
            updateAnimationStateParamName(rotationAnimation);
            targetRotation = currentDestinationPoint.target.transform.rotation;
        }
        else if (!currentDestinationPoint.shouldUseRootMotion)
        {
            character.transform.position += character.transform.forward * walkingSpeed * Time.deltaTime;
        }


        //        float yawRotationOffset = targetRotation.eulerAngles.y * turningSpeed * Time.deltaTime;
        //        float difference = (targetRotation * Quaternion.Inverse(character.transform.rotation)).eulerAngles.y;
        //        if (difference < yawRotationOffset)
        //            yawRotationOffset = difference;
        //        print("rotation offset: YAW: " + yawRotationOffset);
        Quaternion rot = Quaternion.Lerp(character.transform.rotation, targetRotation, Time.deltaTime * turningSpeed);
        character.transform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, 0);
        //character.transform.rotation *= Quaternion.Euler(0, yawRotationOffset, 0);

        characterLocation = character.transform.position;
        characterLocation.y = 0f;
        destination = currentDestinationPoint.target.transform.position;
        destination.y = 0f;
        distance = (destination - characterLocation).magnitude;
        print("distance to target is " + distance);
        float rotDifference = character.transform.rotation.eulerAngles.y - currentDestinationPoint.target.transform.rotation.eulerAngles.y;
        print(rotDifference);
        if ((distance < distanceThreshhold && !currentDestinationPoint.mustInheritRotation) || (currentDestinationPoint.mustInheritRotation && Mathf.Abs(rotDifference) < 5f))
        {
            toNextDestination();
        }

    }

    // Sets the next point as the destination, if no destination is left, deactivate component.
    private void toNextDestination()
    {
        if (currentDestinationIndex < destinationPoints.Count - 1)
        {
            currentDestinationIndex++;
            currentDestinationPoint = destinationPoints[currentDestinationIndex];

            updateAnimationStateParamName(currentDestinationPoint.animationState);
            if (rootMotionAnimationEndTime == -1 && currentDestinationPoint.shouldUseRootMotion)
            {
                if (currentDestinationPoint.animationState == VoiceImageCanvasSync.AnimationState.MoveUp)
                    rootMotionAnimationEndTime = Time.time + 1.1f;
                else
                    rootMotionAnimationEndTime = Time.time + 0.75f;
            }
            else
            {
                rootMotionAnimationEndTime = -1;
            }
        }
        else
        {
            enabled = false;
        }

    }

    private void OnDisable()
    {
        rootMotionAnimationEndTime = -1;
        updateAnimationStateParamName(destinationReachedAnimation);
    }

    // Updates character animation state.
    void updateAnimationStateParamName(VoiceImageCanvasSync.AnimationState animationState)
    {
        currentAnimationState = animationState;
        switch (currentAnimationState)
        {
            case VoiceImageCanvasSync.AnimationState.Idle: { currentAnimationStateParamName = "idle"; break; }
            case VoiceImageCanvasSync.AnimationState.Walk: { currentAnimationStateParamName = "walk"; break; }
            case VoiceImageCanvasSync.AnimationState.TurnRight90: { currentAnimationStateParamName = "turn_right_90"; break; }
            case VoiceImageCanvasSync.AnimationState.TurnRight180: { currentAnimationStateParamName = "turn_right_180"; break; }
            case VoiceImageCanvasSync.AnimationState.TurnLeft90: { currentAnimationStateParamName = "turn_left_90"; break; }
            case VoiceImageCanvasSync.AnimationState.TurnLeft180: { currentAnimationStateParamName = "turn_left_180"; break; }
            case VoiceImageCanvasSync.AnimationState.MoveUp: { currentAnimationStateParamName = "move_up"; break; }
            case VoiceImageCanvasSync.AnimationState.MoveDown: { currentAnimationStateParamName = "move_down"; break; }
        }
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("idle", false);
            characterAnimator.SetBool("walk", false);
            characterAnimator.SetBool("turn_right_90", false);
            characterAnimator.SetBool("turn_right_180", false);
            characterAnimator.SetBool("turn_left_90", false);
            characterAnimator.SetBool("turn_left_180", false);
            characterAnimator.SetBool("move_up", false);
            characterAnimator.SetBool("move_down", false);
            characterAnimator.SetBool("run", false);

            characterAnimator.SetBool(currentAnimationStateParamName, true);
        }
    }
}
