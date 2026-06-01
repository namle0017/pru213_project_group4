using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    public Transform cameraTransform;

    private float spriteWidth;

    void Start()
    {
        spriteWidth =
            GetComponent<SpriteRenderer>()
            .bounds.size.x;
    }

    void Update()
    {
        if (cameraTransform.position.x >
            transform.position.x + spriteWidth)
        {
            transform.position +=
                Vector3.right * spriteWidth * 2f;
        }
    }
}