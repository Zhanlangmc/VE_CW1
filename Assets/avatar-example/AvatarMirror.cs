using System;
using System.Collections.Generic;
using Ubiq.Avatars;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Rendering;

public class AvatarMirror : MonoBehaviour
{
    [Tooltip("The plane upon which to project reflected avatars. Note that " +
             "the rotation and scale properties of this object's transform " +
             "will be ignored, but the position will represent a point on " +
             "the plane.")]
    public Plane plane;

    public float mirrorScale = 1.0f; // ͳһ�������ű���

    public enum Plane
    {
        XY,
        YZ
    }
    
    private AvatarManager avatarManager;
    private List<Renderer> renderers = new();
    private Transform _transform;
    private Dictionary<Material,Material> materials = new();

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        avatarManager = NetworkScene.Find(this)
            .GetComponentInChildren<AvatarManager>();
    }

    private void OnDestroy()
    {
        foreach (var material in materials.Values)
        {
            Destroy(material);
        }
        materials = null;
    }

    Vector3 Divide(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    private void Update()
    {
        UpdatePlane(out var scaleMultiplier, out var eulerMultiplier);

        for (int ai = 0; ai < avatarManager.transform.childCount; ai++)
        {
            var avatar = avatarManager.transform.GetChild(ai);
            renderers.Clear();
            avatar.GetComponentsInChildren(includeInactive: false, renderers);

            // �ҳ��� Renderer��Slime ���壩
            var rootRenderer = avatar.GetComponentInChildren<SkinnedMeshRenderer>();

            foreach (var renderer in renderers)
            {
                Mesh mesh = null;

                if (renderer is SkinnedMeshRenderer skinned)
                {
                    mesh = new Mesh();
                    skinned.BakeMesh(mesh);
                }
                else if (renderer is MeshRenderer)
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter)
                    {
                        mesh = filter.sharedMesh;
                    }
                }

                if (mesh == null) continue;

                var t = renderer.transform;

                // Step 1: ���㾵��ƽ��
                Vector3 planeNormal;
                switch (plane)
                {
                    case Plane.XY:
                        planeNormal = _transform.forward; // Z����
                        break;
                    case Plane.YZ:
                        planeNormal = _transform.right;   // X����
                        break;
                    default:
                        planeNormal = _transform.forward;
                        break;
                }

                planeNormal.Normalize();
                var planePoint = _transform.position;

                // Step 2: ����λ�� = Դ�����ƽ��ĶԳƵ�
                var toPlane = t.position - planePoint;
                var distance = Vector3.Dot(toPlane, planeNormal);
                var mirroredPos = t.position - 2 * distance * planeNormal;

                // Step 3: ������ת = �������ת�ķ���
                var rot = t.rotation;
                var fwd = rot * Vector3.forward;
                var up = rot * Vector3.up;

                var mirroredFwd = Vector3.Reflect(fwd, planeNormal);
                var mirroredUp = Vector3.Reflect(up, planeNormal);

                var mirroredRot = Quaternion.LookRotation(mirroredFwd, mirroredUp);

                // ���Ŵ���
                var scale = t.lossyScale;
                if (renderer == rootRenderer)
                {
                    scale *= mirrorScale;
                }

                var matrix = Matrix4x4.TRS(mirroredPos, mirroredRot, scale);

                // Step 5: ���ʸ��Ʋ������޳�
                if (!materials.TryGetValue(renderer.sharedMaterial, out var material))
                {
                    material = new Material(renderer.sharedMaterial);
                    materials.Add(renderer.sharedMaterial, material);
                }

                material.CopyPropertiesFromMaterial(renderer.sharedMaterial);
                material.SetFloat("_Cull", (int)CullMode.Off);

                // Step 6: Draw
                Graphics.DrawMesh(mesh, matrix, material, gameObject.layer);
            }
        }
    }

    private void UpdatePlane( 
        out Vector3 scaleMultiplier, out Vector3 eulerMultiplier)
    {
        switch (plane)
        {
            case Plane.XY:
                scaleMultiplier = new Vector3(1,1,-1);
                eulerMultiplier = new Vector3(-1,-1,1);
                _transform.rotation = Quaternion.Euler(0,0,0);
                break;
            case Plane.YZ:
                scaleMultiplier = new Vector3(-1,1,1);
                eulerMultiplier = new Vector3(1,-1,1);
                _transform.rotation = Quaternion.Euler(0,90,0);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(plane), plane, null);
        }
    }
}
