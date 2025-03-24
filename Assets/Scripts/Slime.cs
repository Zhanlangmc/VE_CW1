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
    public float inside_coeff = 0.0f;   // 嵌入量
    
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return; // 只响应被 dodgeball 击中（按需设置 Tag）

        // 获取 impact 点和方向（估算）
        Vector3 impactPoint = meshCollider.ClosestPoint(other.transform.position); // 最接近撞击点
        Vector3 direction = (impactPoint - other.transform.position).normalized; // 估算法线
        Vector3 correctedImpactPoint = impactPoint + direction * inside_coeff;

        // Debug.DrawRay(impactPoint, direction * 0.5f, Color.red, 1.5f);
        // Debug.Log("[TriggerHit] From: " + other.name + " at " + impactPoint);

        StartCoroutine(DeformMesh(correctedImpactPoint, direction));
    }

    IEnumerator DeformMesh(Vector3 impactPoint, Vector3 impactNormal)
    {
        Transform meshTransform = skinnedMeshRenderer.transform;
        isDeforming = true;
        float deformDuration = recoverySpeed;
        float recoverDuration = recoverySpeed;

        float t = 0f;

        // 阶段一：凹陷
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
                    Vector3 localImpactNormal = meshTransform.InverseTransformDirection(impactNormal);
                    Vector3 targetPosition = originalVertices[i] + localImpactNormal * deformAmount;
                    modifiedVertices[i] = Vector3.Lerp(originalVertices[i], targetPosition, progress);
                }
                else
                {
                    modifiedVertices[i] = originalVertices[i];
                }
            }

            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();

            t += Time.deltaTime;
            yield return null;
        }

        // 阶段二：恢复
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

        // 完全恢复
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
            skinnedMeshRenderer.sharedMesh = mesh;
        }

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }
}
