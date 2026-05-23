using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    private TextMeshProUGUI textUI;

    public float blinkSpeed = 2f;
    public float moveSpeed = 2f;
    public float moveAmount = 10f;
    public float scaleAmount = 0.05f;

    private Vector3 startPos;
    private Vector3 startScale;

    void Start()
    {
        textUI = GetComponent<TextMeshProUGUI>();

        startPos = transform.localPosition;
        startScale = transform.localScale;
    }

    void Update()
    {
        // NHẤP NHÁY
        float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));

        Color color = textUI.color;
        color.a = alpha;
        textUI.color = color;

        // FLOAT LÊN XUỐNG
        float y = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
        transform.localPosition = startPos + new Vector3(0, y, 0);

        // SCALE NHẸ
        float scale = 1 + Mathf.Sin(Time.time * moveSpeed) * scaleAmount;
        transform.localScale = startScale * scale;
    }
}
