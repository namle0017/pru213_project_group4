using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    private TextMeshProUGUI textUI;

    public float blinkSpeed = 2f;
    public float floatSpeed = 2f;
    public float moveAmount = 5f;
    public float scaleAmount = 0.03f;

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
        // 🔥 BLINK MƯỢT (không tắt hẳn)
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(0.4f, 1f, t);

        Color c = textUI.color;
        c.a = alpha;
        textUI.color = c;

        // 🔥 FLOAT NHẸ
        float y = Mathf.Sin(Time.time * floatSpeed) * moveAmount;
        transform.localPosition = startPos + new Vector3(0, y, 0);

        // 🔥 SCALE RẤT NHẸ (không rung)
        float s = 1f + Mathf.Sin(Time.time * floatSpeed) * scaleAmount;
        transform.localScale = new Vector3(s, s, s);
    }
}