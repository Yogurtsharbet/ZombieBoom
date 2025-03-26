using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PlatformControl : MonoBehaviour {
    private static readonly string PlatformDataFileName = "PlatformData";

    private Rigidbody2D platformRigidbody;
    private HingeJoint2D platformHinge;
    
    private Coroutine interaction;
    private PlatformData data;

    private BlockManager blockManager;
    private PlayerBehavior playerBehavior;

    private void Awake() {
        playerBehavior = FindAnyObjectByType<PlayerBehavior>();
        blockManager = FindAnyObjectByType<BlockManager>();

        platformRigidbody = GetComponent<Rigidbody2D>();
        platformHinge = GetComponent<HingeJoint2D>();
        
        data = new PlatformData();
        data.Load(PlatformDataFileName);
    }

    private void Start() {
        platformRigidbody.isKinematic = true;
        platformRigidbody.angularDrag = 2f;
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 물 블록과 충돌시 상호작용 코루틴을 실행합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : coroutine에 왼쪽 여부를 매개변수로 전달하여 회전 coroutine을 실행합니다.
    /// </para>
    /// </summary>
    /// <param name="isLeft">왼쪽에서 충돌 되었으면 true, 아니면 false</param>
    public void InteractPlatform(bool isLeft) {
        playerBehavior.isReadyToGrab = false;

        if (interaction != null) {
            StopCoroutine(interaction);
            rotationDotween(0.1f);
        }
        interaction = StartCoroutine(AddTorqueCo(isLeft));
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : platform을 회전합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : rotation만 하면 물체들이 물리적용이 되지 않아 플랫폼에 addTorque로 힘을 줍니다.
    ///         회전 일정시간 후 회전을 멈춥니다.
    /// </para>
    /// </summary>
    /// <param name="isLeft">왼쪽 방향 여부</param>
    /// <returns>platform 이동 후 돌아옴</returns>
    private IEnumerator AddTorqueCo(bool isLeft) {

        platformRigidbody.isKinematic = false;
        platformHinge.useMotor = true;

        JointMotor2D motor = platformHinge.motor;
        motor.maxMotorTorque = data.MaxTorque;
        motor.motorSpeed = (isLeft ? 1 : -1) * data.MotorSpeed;
        platformHinge.motor = motor;

        Manager_MonoSingleton_Audio.instance.SFX_Clip_IndexToPlay = 8;

        yield return new WaitForSeconds(data.RotateTime);

        //회전 멈춤
        motor.motorSpeed = 0f;
        platformHinge.motor = motor;
        platformRigidbody.angularVelocity = 0f;
        platformRigidbody.isKinematic = true;

        //원래 방향으로 돌립니다.
        yield return new WaitForSeconds(data.PauseTime);

        rotationDotween();
    }


    /// <summary>
    /// <para>
    ///     작동 목적 : platform을 원래 위치로 돌려놓습니다.
    /// </para>
    /// <para>
    ///     작동 방식 : dotween으로 부드럽게 platform이 원래 위치로 돌아오도록 합니다.
    /// </para>
    /// </summary>
    private void rotationDotween(float reCentering = -1f) {
        if (reCentering == -1f) reCentering = data.ReCenteringTime;
        this.transform.DOLocalRotate(new Vector3(0, 0, 0f), reCentering, RotateMode.Fast)
            .SetEase(data.EaseMode)
            .OnComplete(() => {
                platformRigidbody.isKinematic = true;
                playerBehavior.isReadyToGrab = true;
                interaction = null;
            });
    }
}