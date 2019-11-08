using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FollowAnimatedRig : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        follow();
    }

    public SkinnedMeshRenderer sourceAnimatedRig;
    private SkinnedMeshRenderer targetAnimatedRig;
    private void follow()
    {
        targetAnimatedRig = GetComponent<SkinnedMeshRenderer>();
        if (sourceAnimatedRig == null || targetAnimatedRig == null) return;

        targetAnimatedRig.bones =
            sourceAnimatedRig.bones.Where(b => targetAnimatedRig.bones.Any(
                t =>
                {
                    return t.name.Equals(b.name);
                }
                )).ToArray();
        print("count: " + targetAnimatedRig.bones.Length);
        foreach(Transform bone in targetAnimatedRig.bones)
        {
            print(bone.ToString());
        }
        print("------------------------------------------");
        print("count: " + sourceAnimatedRig.bones.Length);
        foreach(Transform bone in sourceAnimatedRig.bones)
        {
            print(bone.ToString());
        }
    }
}
