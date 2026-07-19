using UnityEngine;

public class MotorDriverLegAnimator : MonoBehaviour
{
    [Header("Driver Parts")]
    [SerializeField] private Transform _driverLegBack;
    [SerializeField] private Transform _driverLegFront;
    [SerializeField] private Transform _driverShoe;

    [Header("Wheel References")]
    [SerializeField] private Rigidbody2D _frontWheelRb;
    [SerializeField] private Rigidbody2D _rearWheelRb;

    [Header("Animation Settings")]
    [SerializeField] private float _pedalSpeedMultiplier = 0.045f;
    [SerializeField] private float _maxPedalSpeed = 8f;
    [SerializeField] private float _movementThreshold = 8f;
    [SerializeField] private float _returnSpeed = 8f;
    [SerializeField] private float _backLegAmplitude = 18f;
    [SerializeField] private float _frontLegAmplitude = 24f;
    [SerializeField] private float _shoeAmplitude = 14f;
    [SerializeField] private float _frontLegPhaseOffset = Mathf.PI;
    [SerializeField] private float _shoePhaseOffset = 1.9f;

    private float _defaultBackLegZ;
    private float _defaultFrontLegZ;
    private float _defaultShoeZ;
    private float _phase;

    private void Awake()
    {
        AutoAssignIfNeeded();
        CacheDefaultAngles();
    }

    private void OnValidate()
    {
        AutoAssignIfNeeded();
        CacheDefaultAngles();
    }

    private void Update()
    {
        if (_driverLegBack == null || _driverLegFront == null || _driverShoe == null)
        {
            return;
        }

        float signedWheelSpeed = GetAverageWheelAngularVelocity();
        float absWheelSpeed = Mathf.Abs(signedWheelSpeed);

        if (absWheelSpeed > _movementThreshold)
        {
            float pedalSpeed = Mathf.Clamp(absWheelSpeed * _pedalSpeedMultiplier, 0.5f, _maxPedalSpeed);
            _phase += pedalSpeed * Time.deltaTime * Mathf.Sign(signedWheelSpeed);

            SetLocalZ(_driverLegBack, _defaultBackLegZ + Mathf.Sin(_phase) * _backLegAmplitude);
            SetLocalZ(_driverLegFront, _defaultFrontLegZ + Mathf.Sin(_phase + _frontLegPhaseOffset) * _frontLegAmplitude);
            SetLocalZ(_driverShoe, _defaultShoeZ + Mathf.Sin(_phase + _shoePhaseOffset) * _shoeAmplitude);
            return;
        }

        float lerpFactor = 1f - Mathf.Exp(-_returnSpeed * Time.deltaTime);
        SetLocalZ(_driverLegBack, Mathf.LerpAngle(GetLocalZ(_driverLegBack), _defaultBackLegZ, lerpFactor));
        SetLocalZ(_driverLegFront, Mathf.LerpAngle(GetLocalZ(_driverLegFront), _defaultFrontLegZ, lerpFactor));
        SetLocalZ(_driverShoe, Mathf.LerpAngle(GetLocalZ(_driverShoe), _defaultShoeZ, lerpFactor));
    }

    private void AutoAssignIfNeeded()
    {
        if (_driverLegBack == null)
        {
            Transform legBack = transform.Find("DriverLegBack");
            if (legBack != null)
            {
                _driverLegBack = legBack;
            }
        }

        if (_driverLegFront == null)
        {
            Transform legFront = transform.Find("DriverLegFront");
            if (legFront != null)
            {
                _driverLegFront = legFront;
            }
        }

        if (_driverShoe == null)
        {
            Transform shoe = transform.Find("DriverShoe");
            if (shoe != null)
            {
                _driverShoe = shoe;
            }
        }

        if (_frontWheelRb == null)
        {
            Transform frontWheel = transform.Find("Front_tire");
            if (frontWheel != null)
            {
                _frontWheelRb = frontWheel.GetComponent<Rigidbody2D>();
            }
        }

        if (_rearWheelRb == null)
        {
            Transform rearWheel = transform.Find("Back_tire_f1");
            if (rearWheel != null)
            {
                _rearWheelRb = rearWheel.GetComponent<Rigidbody2D>();
            }
        }
    }

    private void CacheDefaultAngles()
    {
        if (_driverLegBack != null)
        {
            _defaultBackLegZ = GetLocalZ(_driverLegBack);
        }

        if (_driverLegFront != null)
        {
            _defaultFrontLegZ = GetLocalZ(_driverLegFront);
        }

        if (_driverShoe != null)
        {
            _defaultShoeZ = GetLocalZ(_driverShoe);
        }
    }

    private float GetAverageWheelAngularVelocity()
    {
        float total = 0f;
        int count = 0;

        if (_frontWheelRb != null)
        {
            total += _frontWheelRb.angularVelocity;
            count++;
        }

        if (_rearWheelRb != null)
        {
            total += _rearWheelRb.angularVelocity;
            count++;
        }

        if (count == 0)
        {
            return 0f;
        }

        return total / count;
    }

    private static float GetLocalZ(Transform target)
    {
        return target.localEulerAngles.z;
    }

    private static void SetLocalZ(Transform target, float angleZ)
    {
        Vector3 currentEuler = target.localEulerAngles;
        currentEuler.z = angleZ;
        target.localEulerAngles = currentEuler;
    }
}
