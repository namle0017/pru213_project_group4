using UnityEngine;

public class BGAnimation : MonoBehaviour
{
    public float moveSpeed = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * moveSpeed) * 0.5f;

        transform.position = startPos + new Vector3(x, 0, 0);
    }
}