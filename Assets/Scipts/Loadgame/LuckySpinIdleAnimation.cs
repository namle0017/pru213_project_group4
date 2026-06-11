using UnityEngine;

public class LuckySpinIdleAnimation : MonoBehaviour
{
    [SerializeField] private float intervalSeconds = 3f;
    [SerializeField] private float spinDuration = 0.6f;
    [SerializeField] private float spinDegrees = 360f;

    private float nextSpinTime;
    private float spinStartTime;
    private Quaternion idleRotation;
    private Quaternion targetRotation;
    private bool isSpinning;

    private void OnEnable()
    {
        idleRotation = transform.localRotation;
        nextSpinTime = Time.unscaledTime + intervalSeconds;
        isSpinning = false;
    }

    private void Update()
    {
        float now = Time.unscaledTime;

        if (!isSpinning)
        {
            if (now < nextSpinTime)
            {
                return;
            }

            spinStartTime = now;
            idleRotation = transform.localRotation;
            targetRotation = idleRotation * Quaternion.Euler(0f, 0f, -spinDegrees);
            isSpinning = true;
            return;
        }

        float progress = Mathf.Clamp01((now - spinStartTime) / Mathf.Max(0.01f, spinDuration));
        float eased = 1f - Mathf.Pow(1f - progress, 3f);
        transform.localRotation = Quaternion.SlerpUnclamped(idleRotation, targetRotation, eased);

        if (progress >= 1f)
        {
            transform.localRotation = idleRotation;
            isSpinning = false;
            nextSpinTime = now + intervalSeconds;
        }
    }
}
