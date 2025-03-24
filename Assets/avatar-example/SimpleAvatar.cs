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
        if (!isActiveAndEnabled || head == null)
        {
            return;
        }

        if (!pose.valid)
        {
            head.gameObject.SetActive(false);
            return;
        }

        head.gameObject.SetActive(true);
        head.SetPositionAndRotation(pose.value.position, pose.value.rotation);
    }

    private void Avatar_OnLeftHandUpdate(InputVar<Pose> pose)
    {
        if (!isActiveAndEnabled || leftHand == null)
        {
            return;
        }

        //if (!pose.valid)
        //{
        //    leftHand.gameObject.SetActive(false);
        //    return;
        //}

        leftHand.gameObject.SetActive(true);
        leftHand.SetPositionAndRotation(pose.value.position, pose.value.rotation);
    }

    private void Avatar_OnRightHandUpdate(InputVar<Pose> pose)
    {
        if (!isActiveAndEnabled || rightHand == null)
        {
            return;
        }

        //if (!pose.valid)
        //{
        //    rightHand.gameObject.SetActive(false);
        //    return;
        //}

        rightHand.gameObject.SetActive(true);
        rightHand.SetPositionAndRotation(pose.value.position, pose.value.rotation);
    }

    private void Update()
    {
        if (!Application.isEditor || !Application.isPlaying)
            return;

        // 模拟手部跟随头部（左侧一点）
        if (head != null && leftHand != null)
        {
            var offset = head.right * -0.4f + Vector3.up * 0.2f + head.forward * 0.2f;
            leftHand.position = head.position + offset;
            leftHand.rotation = head.rotation;
        }

        // 同理，右手
        if (head != null && rightHand != null)
        {
            var offset = head.right * 0.4f + Vector3.up * 0.2f + head.forward * 0.2f;
            rightHand.position = head.position + offset;
            rightHand.rotation = head.rotation;
        }

    }
}
