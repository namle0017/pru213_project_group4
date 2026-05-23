using UnityEngine;

public class BGMove : MonoBehaviour
{
    public float speed = 0.5f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}