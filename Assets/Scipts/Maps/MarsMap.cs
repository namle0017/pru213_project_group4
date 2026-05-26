using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class MarsMap : MonoBehaviour
{
    [Header("Sprite Shape")]
    public SpriteShapeController spriteShape;

    [Header("Terrain Settings")]

    [Range(50, 300)]
    public int levelLength = 120;

    [Range(1f, 5f)]
    public float xMultiplier = 2f;

    [Range(5f, 30f)]
    public float yMultiplier = 15f;

    [Range(0.01f, 1f)]
    public float noiseStep = 0.08f;

    [Range(0f, 1f)]
    public float smoothness = 0.2f;

    [Header("Bottom Fill")]
    public float bottom = 30f;

    private Vector3 lastPos;

    void OnValidate()
    {
        if (spriteShape == null)
            return;

        // Reset spline
        spriteShape.spline.Clear();

        // Terrain kín
        spriteShape.spline.isOpenEnded = false;

        // Generate terrain
        for (int i = 0; i < levelLength; i++)
        {
            // Perlin Noise
            float noise = Mathf.PerlinNoise(i * noiseStep, 0);

            // Terrain nhọn hơn
            noise = Mathf.Pow(noise, 1.5f);

            // Random cliff
            float extraHeight = Random.Range(-1f, 2f);

            // Height cuối
            float height = (noise * yMultiplier) + extraHeight;

            // Position point
            lastPos = transform.position + new Vector3(
                i * xMultiplier,
                height,
                0
            );

            // Add point
            spriteShape.spline.InsertPointAt(i, lastPos);

            // Terrain gãy khúc
            if (i != 0 && i != levelLength - 1)
            {
                spriteShape.spline.SetTangentMode(
                    i,
                    ShapeTangentMode.Broken
                );
            }

            // Tangent trái
            spriteShape.spline.SetLeftTangent(
                i,
                Vector3.left * smoothness
            );

            // Tangent phải
            spriteShape.spline.SetRightTangent(
                i,
                Vector3.right * smoothness
            );
        }

        // ===== FILL ĐẤT PHÍA DƯỚI =====

        float bottomY = transform.position.y - bottom;

        // Góc phải dưới
        spriteShape.spline.InsertPointAt(
            levelLength,
            new Vector3(lastPos.x, bottomY, 0)
        );

        // Góc trái dưới
        spriteShape.spline.InsertPointAt(
            levelLength + 1,
            new Vector3(transform.position.x, bottomY, 0)
        );
    }
}