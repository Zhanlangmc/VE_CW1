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

    private void Update()
    {
        UpdatePlane(out var scaleMultiplier, out var eulerMultiplier);
        
        // Get avatars
        for (int ai = 0; ai < avatarManager.transform.childCount; ai++)
        {
            var avatar = avatarManager.transform.GetChild(ai);
            
            // Get every renderer on each avatar and queue it up to be rendered
            // again in the mirror. 
            // 
            // Re-use a list for the renderers. New lists need to be cleaned up
            // by the C# runtime, which is performance intensive and can cause
            // gameplay hiccups, so we re-use where we can. 
            renderers.Clear();
            avatar.GetComponentsInChildren(includeInactive:false,renderers);
            
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                var filter = renderer.GetComponent<MeshFilter>();
                var rendererTransform = renderer.GetComponent<Transform>();
                if (!filter)
                {
                    // Not all renderers will have meshes - just skip those
                    // without.
                    continue;
                }
                
                // Manipulate the renderer's transformation matrix to make it 
                // appear reflected.
                var mat = renderer.localToWorldMatrix;

                // Translate renderer to origin
                mat = Matrix4x4.Translate(-rendererTransform.position) * mat;
                
                // Align with the plane 
                mat = Matrix4x4.Rotate(Quaternion.Inverse(rendererTransform.rotation)) * mat;
                
                // 'Reflect' in the plane
                mat = Matrix4x4.Scale(scaleMultiplier) * mat;
                
                // Return renderer to position, inverting required angles
                var eul = rendererTransform.rotation.eulerAngles;
                eul.Scale(eulerMultiplier);
                mat = Matrix4x4.Rotate(Quaternion.Euler(eul)) * mat;
                mat = Matrix4x4.Translate(rendererTransform.position) * mat;
                
                // Project renderer onto mirror
                var mirrorToRenderer = rendererTransform.position - _transform.position;
                var closestPointOnMirror = _transform.position + _transform.right * 
                    Vector3.Dot(mirrorToRenderer,_transform.right);
                
                var toMirror = closestPointOnMirror - rendererTransform.position;
                toMirror.y = 0;
                mat = Matrix4x4.Translate(toMirror*2) * mat;
                
                // Account for inverted matrix causing inverted winding order by
                // rendering back and front faces
                if (!materials.TryGetValue(renderer.sharedMaterial,out var material))
                {
                    material = new Material(renderer.sharedMaterial);
                    materials.Add(renderer.sharedMaterial,material);
                }
                
                material.CopyPropertiesFromMaterial(renderer.sharedMaterial);
                material.SetFloat("_Cull",(int)CullMode.Off);
                
                Graphics.DrawMesh(
                    mesh: filter.mesh,
                    matrix: mat,
                    material: material,
                    layer: gameObject.layer);
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
