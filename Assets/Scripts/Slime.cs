using UnityEngine;
using System.Collections;

public class Slime : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshCollider meshCollider;
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    private bool isDeforming = false;

    public float deformStrength = 1.0f;  // 形变幅度
    public float deformRadius = 1.0f;    // 形变影响半径
    public float recoverySpeed = 0.5f;   // 形变恢复速度
    public float inside_coeff = 0.2f;   // 嵌入量
    
    void Start()
    {
        // 获取 Skinned Mesh Renderer
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        meshCollider = GetComponentInChildren<MeshCollider>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found on " + gameObject.name);
            return;
        }
        // 1. 复制 mesh
        mesh = Instantiate(skinnedMeshRenderer.sharedMesh);
        skinnedMeshRenderer.sharedMesh = mesh;

        // 2. 获取顶点
        originalVertices = mesh.vertices;
        modifiedVertices = mesh.vertices;

        // 3. 最后再绑定 mesh 到 collider
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name); // 确保检测到碰撞

        // 获取碰撞点
        ContactPoint contact = collision.contacts[0];
        Vector3 impactPoint = contact.point; // 世界坐标系的碰撞点
        Vector3 impactNormal = contact.normal; // 碰撞点的法线方向
        Debug.DrawRay(contact.point, contact.normal * 0.5f, Color.red, 2.0f);
        
        // 修正碰撞点，防止它太靠内
        Vector3 correctedImpactPoint = impactPoint + impactNormal * inside_coeff;

        StartCoroutine(DeformMesh(correctedImpactPoint, impactNormal));
    }

    IEnumerator DeformMesh(Vector3 impactPoint, Vector3 impactNormal)
    {
        Transform meshTransform = skinnedMeshRenderer.transform;
        isDeforming = true;
        float deformDuration = recoverySpeed;  // 凹陷持续时间（越大越慢）
        float recoverDuration = recoverySpeed;   // 恢复时间

        float t = 0f;

        // 阶段一：逐步凹陷
        while (t < deformDuration)
        {
            float progress = Mathf.Clamp01(t / deformDuration);

            for (int i = 0; i < modifiedVertices.Length; i++)
            {   
                Vector3 worldVertexPos = meshTransform.TransformPoint(originalVertices[i]);
                float distance = Vector3.Distance(worldVertexPos, impactPoint);

                if (distance < deformRadius)
                {
                    float deformAmount = deformStrength * Mathf.Exp(-distance / deformRadius);
                    Vector3 localImpactNormal = transform.InverseTransformDirection(impactNormal);

                    Vector3 targetPosition = originalVertices[i] + localImpactNormal * deformAmount;
                    modifiedVertices[i] = Vector3.Lerp(originalVertices[i], targetPosition, progress);
                }
                else
                {
                    modifiedVertices[i] = originalVertices[i]; // 未受影响的顶点保持不变
                }
            }

            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();

            t += Time.deltaTime;
            yield return null;
        }

        // 阶段二：逐步恢复
        t = 0f;
        while (t < recoverDuration)
        {
            float progress = t / recoverDuration;

            for (int i = 0; i < modifiedVertices.Length; i++)
            {
                modifiedVertices[i] = Vector3.Lerp(modifiedVertices[i], originalVertices[i], progress);
            }

            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();

            t += Time.deltaTime;
            yield return null;
        }

        // 最终归位
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i];
        }
        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();

        isDeforming = false;
    }


    void LateUpdate()
    {
        if (isDeforming)
        {
            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();
            skinnedMeshRenderer.sharedMesh = mesh;  // 强制更新 Skinned Mesh Renderer
        }
        
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }
}
