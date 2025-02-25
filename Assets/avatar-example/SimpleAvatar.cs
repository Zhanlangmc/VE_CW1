using Ubiq;
using UnityEngine;

/// <summary>
/// This class connects the events provided by the HeadAndHandsAvatar to a set
/// of transforms. The HeadAndHandsAvatar handles all the syncing of avatar
/// poses and outputs events for both local and remote copies, so we only need
/// one version of this class.
/// </summary>
public class SimpleAvatar : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    
    private HeadAndHandsAvatar avatar;
    
    private void Start()
    {
        avatar = GetComponentInParent<HeadAndHandsAvatar>();

        avatar.OnHeadUpdate.AddListener(Avatar_OnHeadUpdate);
        avatar.OnLeftHandUpdate.AddListener(Avatar_OnLeftHandUpdate);
        avatar.OnRightHandUpdate.AddListener(Avatar_OnRightHandUpdate);
    }

    private void OnDestroy()
    {
        if (avatar)
        {
            avatar.OnHeadUpdate.RemoveListener(Avatar_OnHeadUpdate);
            avatar.OnLeftHandUpdate.RemoveListener(Avatar_OnLeftHandUpdate);
            avatar.OnRightHandUpdate.RemoveListener(Avatar_OnRightHandUpdate);
        }
    }
    
    private void Avatar_OnHeadUpdate(InputVar<Pose> pose)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        
        if (!pose.valid)
        {
            head.gameObject.SetActive(false);
            return;
        }
        
        head.SetPositionAndRotation(pose.value.position,pose.value.rotation);
    }

    private void Avatar_OnLeftHandUpdate(InputVar<Pose> pose)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        
        if (!pose.valid)
        {
            leftHand.gameObject.SetActive(false);
            return;
        }
        
        leftHand.SetPositionAndRotation(pose.value.position,pose.value.rotation);
    }
    
    private void Avatar_OnRightHandUpdate(InputVar<Pose> pose)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        
        if (!pose.valid)
        {
            rightHand.gameObject.SetActive(false);
            return;
        }
        
        rightHand.SetPositionAndRotation(pose.value.position,pose.value.rotation);
    }
}
