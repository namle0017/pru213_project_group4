using UnityEngine;
using TMPro;

public class GameTitleEffect : MonoBehaviour
{
    private TextMeshProUGUI textUI;

    public float floatSpeed = 1.5f;
    public float floatAmount = 8f;

    public float scaleSpeed = 1f;
    public float scaleAmount = 0.03f;

    public float glowSpeed = 1f;

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
        // 🌊 FLOAT NHẸ (KIỂU LOGO GAME)
        float y = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = startPos + new Vector3(0, y, 0);

        // 🔥 SCALE NHẸ (HƠI “THỞ”)
        float s = 1f + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = new Vector3(s, s, s);

        // ✨ SÁNG NHẸ (KHÔNG NHẤP NHÁY MẠNH)
        float glow = Mathf.Lerp(0.85f, 1f,
            (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f);

        Color c = textUI.color;
        c.a = glow;
        textUI.color = c;
    }
}