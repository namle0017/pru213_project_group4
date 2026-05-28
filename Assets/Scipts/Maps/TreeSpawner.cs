using UnityEngine;
using UnityEngine.U2D;

public class TreeSpawner : MonoBehaviour
{
    public GameObject treePrefab;

    public SpriteShapeController terrain;

    public float spacing = 5f;

    public float yOffset = 1f;

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        int pointCount =
            terrain.spline.GetPointCount();

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 point =
                terrain.spline.GetPosition(i);

            // khoảng cách cây
            if (i % Mathf.RoundToInt(spacing) != 0)
                continue;

            Vector3 spawnPos =
                new Vector3(
                    point.x,
                    point.y + yOffset,
                    0
                );

            GameObject tree =
                Instantiate(
                    treePrefab,
                    spawnPos,
                    Quaternion.identity
                );

            tree.transform.SetParent(transform);

            float randomScale =
                Random.Range(0.8f, 1.3f);

            tree.transform.localScale =
                Vector3.one * randomScale;
        }
    }
}