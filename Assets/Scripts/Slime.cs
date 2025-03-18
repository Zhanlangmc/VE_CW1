using UnityEngine;
using System.Collections;

public class Slime : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
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
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found on " + gameObject.name);
            return;
        }

        // 复制 Mesh，确保可读写
        mesh = Instantiate(skinnedMeshRenderer.sharedMesh);
        skinnedMeshRenderer.sharedMesh = mesh;

        originalVertices = mesh.vertices;
        modifiedVertices = mesh.vertices;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name); // 确保检测到碰撞

        // 获取碰撞点
        ContactPoint contact = collision.contacts[0];
        Vector3 impactPoint = contact.point; // 世界坐标系的碰撞点
        Vector3 impactNormal = contact.normal; // 碰撞点的法线方向
        Debug.Log("Impact point: " + impactPoint + " | Normal: " + impactNormal);
        // 修正碰撞点，防止它太靠内
        Vector3 correctedImpactPoint = impactPoint + impactNormal * inside_coeff;

        StartCoroutine(DeformMesh(correctedImpactPoint, impactNormal));
    }

    IEnumerator DeformMesh(Vector3 impactPoint, Vector3 impactNormal)
    {
        isDeforming = true;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            for (int i = 0; i < modifiedVertices.Length; i++)
            {
                Vector3 worldVertexPos = transform.TransformPoint(originalVertices[i]);
                float distance = Vector3.Distance(worldVertexPos, impactPoint);

                if (distance < deformRadius)
                {
                    float deformAmount = deformStrength * Mathf.Exp(-distance / deformRadius);
                    Vector3 localImpactNormal = transform.InverseTransformDirection(impactNormal); // 变换到局部空间
                    modifiedVertices[i] = originalVertices[i] + localImpactNormal * deformAmount;
                }
            }

            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();

            elapsedTime += Time.deltaTime * recoverySpeed;
            yield return null;
        }

        // 逐渐恢复形状
        while (elapsedTime > 0)
        {
            for (int i = 0; i < modifiedVertices.Length; i++)
            {
                modifiedVertices[i] = Vector3.Lerp(modifiedVertices[i], originalVertices[i], Time.deltaTime * recoverySpeed);
            }

            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();

            elapsedTime -= Time.deltaTime;
            yield return null;
        }
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
    }
}
