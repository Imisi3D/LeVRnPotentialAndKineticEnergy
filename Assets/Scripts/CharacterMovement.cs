using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


/**
 * Character movement component, to be used for character movement in scene, handles movements and animations.
 *  - Character's movement and animations are specified using DestinationPoint objects.
 */
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

    // Index of current target point.
    public int currentDestinationIndex = 0;

    // Reference to current target point.
    private DestinationPoint currentDestinationPoint;

    // Used to keep track of current animation state of the character.
    private VoiceImageCanvasSync.AnimationState currentAnimationState;

    // Used to know which parameter to modify in the animation controller.
    private string currentAnimationStateParamName;

    // Used to tell the script when to ignore moving the object.
    private float rootMotionAnimationEndTime = -1f;

    // Used to ensure, once the character starts inheriting target's rotation, ignoring rotation threshold.
    private bool isRotating = false;

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
        Vector3 offset = currentDestinationPoint.target.transform.position - character.transform.position;
        float distance = offset.magnitude;
        Vector3 characterLocation = character.transform.position;
        characterLocation.y = 0f;
        Vector3 destination = currentDestinationPoint.target.transform.position;
        destination.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(destination - characterLocation);

        if ((currentDestinationPoint.mustInheritRotation && distance < rotationThreshold) || isRotating)
        {
            updateAnimationStateParamName(rotationAnimation);
            targetRotation = currentDestinationPoint.target.transform.rotation;
            isRotating = true;
        }
        else if (!currentDestinationPoint.shouldUseRootMotion && !isRotating)
        {
            character.transform.position += character.transform.forward * walkingSpeed * Time.deltaTime;
            if (currentDestinationPoint.considerYAxis)
                character.transform.position += new Vector3(0, 1, 0) * walkingSpeed * Time.deltaTime * Math.Sign(Vector3.Dot(offset, new Vector3(0, 1, 0)));;
        }
        Quaternion rot = Quaternion.Lerp(character.transform.rotation, targetRotation, Time.deltaTime * turningSpeed);
        character.transform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, 0);

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
            isRotating = false;
            currentDestinationIndex++;
            currentDestinationPoint = destinationPoints[currentDestinationIndex];

            updateAnimationStateParamName(currentDestinationPoint.animationState);
            if (rootMotionAnimationEndTime == -1 && currentDestinationPoint.shouldUseRootMotion)
            {
                rootMotionAnimationEndTime = Time.time + 1.1f;
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
        if (animationState == VoiceImageCanvasSync.AnimationState.NoUpdate) return;
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
            case VoiceImageCanvasSync.AnimationState.Slide: { currentAnimationStateParamName = "slide"; break; }
        }
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("idle", false);
            characterAnimator.SetBool("walk", false);
            characterAnimator.SetBool("talk", false);
            characterAnimator.SetBool("turn_right_90", false);
            characterAnimator.SetBool("turn_right_180", false);
            characterAnimator.SetBool("turn_left_90", false);
            characterAnimator.SetBool("turn_left_180", false);
            characterAnimator.SetBool("move_up", false);
            characterAnimator.SetBool("move_down", false);
            characterAnimator.SetBool("run", false);
            characterAnimator.SetBool("slide", false);

            characterAnimator.SetBool(currentAnimationStateParamName, true);
        }
    }
}
