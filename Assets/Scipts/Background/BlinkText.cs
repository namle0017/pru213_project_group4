using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    private TextMeshProUGUI textUI;

    public float speed = 2f;

    void Start()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float alpha = Mathf.Abs(Mathf.Sin(Time.time * speed));

        Color color = textUI.color;
        color.a = alpha;

        textUI.color = color;
    }
}