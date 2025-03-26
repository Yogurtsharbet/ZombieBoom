using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class PlatformData : JsonData<BlockDataStruct> {
    [SerializeField] private float motorSpeed;
    [SerializeField] private float motorPower;
    [SerializeField] private float impulseTorque;

    [SerializeField] private float rotateTime;
    [SerializeField] private float pauseTime;
    [SerializeField] private float reCenteringTime;

    [SerializeField] private float totalPower;
    [SerializeField] private float amplicatePower;
    [SerializeField] private Ease easeMode;

    public float MotorSpeed {
        get => motorSpeed;
        set => motorSpeed = Mathf.Clamp(value, 100f, 9999f);
    }
    public float MaxTorque {
        get => motorPower;
        set => motorPower = Mathf.Clamp(value, 100f, 9999f);
    }
    public float ImpulseTorque {
        get => impulseTorque;
        set => impulseTorque = Mathf.Clamp(value, 100f, 9999f);
    }
    public float RotateTime {
        get => rotateTime;
        set => rotateTime = Mathf.Clamp(value, 0f, 10f);
    }
    public float PauseTime {
        get => pauseTime;
        set => pauseTime = Mathf.Clamp(value, 0f, 10f);
    }
    public float ReCenteringTime {
        get => reCenteringTime;
        set => reCenteringTime = Mathf.Clamp(value, 0f, 10f);
    }
    public float TotalPower {
        get => totalPower;
        set => totalPower = Mathf.Clamp(value, 0.1f, 10f);
    }
    public float AmplicatePower {
        get => amplicatePower;
        set => amplicatePower = Mathf.Clamp(value, 0.1f, 10f);
    }
    public Ease EaseMode {
        get => easeMode;
        set => easeMode = value;
    }
}
