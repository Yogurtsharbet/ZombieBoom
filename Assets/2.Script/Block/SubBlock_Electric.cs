using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBlock_Electric : Block {

    private BlockParticlePool ParticlePool;
    
    private PlatformControl platformController;
    private List<Block> activeBlock;
    
    protected override void Awake() {
        base.Awake();
        platformController = FindAnyObjectByType<PlatformControl>();
        activeBlock = FindAnyObjectByType<BlockPooling>().ActiveBlock;
        ParticlePool = FindAnyObjectByType<BlockParticlePool>();
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 블록과 충돌시 상호작용 합니다.
    ///         - 물 블록과 충돌시 플랫폼을 움직입니다.
    /// </para>
    /// <para>
    ///     작동 방식 : 해당하는 Layer위에서 collider 충돌한 물체의 tag를 조사하여
    ///         해당 tag가 상호작용에 해당하는 tag일 때, 각 상호작용을 실행합니다.
    /// </para>
    /// </summary>
    /// <param name="collision">충돌 collision</param>
    protected override void OnCollisionEnter2D(Collision2D collision) {
        base.OnCollisionEnter2D(collision);
        if (!gameObject.CompareTag(blockTag[5])) {
            if (collision.gameObject.layer == 6) {
                if (collision.collider.CompareTag(blockTag[0])) {
                    float collisionX = collision.GetContact(0).point.x;
                    platformFlip(collisionX);
                    RemoveBlock();
                    collision.gameObject.GetComponent<Block>().RemoveBlock();

                    Particle particle = ParticlePool.GetParticle(ParticleType.ELECTRIC);
                    particle.transform.position = transform.position;
                }
            }
        }
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 물 블록과 충돌시 상호작용으로 플랫폼을 회전합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : 현재 충돌 x position과 platform의 x position를 체크하여
    ///         충돌 x > platform x : 오른쪽에서 충돌하여 오른쪽으로 회전
    ///         충돌 x < platform x : 왼쪽에서 충돌하여 왼쪽으로 회전
    /// </para>
    /// </summary>
    /// <param name="collX">충돌 위치 position</param>
    private void platformFlip(float collX) {
        foreach(Block block in activeBlock) {
            if (block == blockManager.GrabbedBlock || block == blockManager.NextBlock) continue;
            block.InteractionInvincible(InteractionType.Electric);
        }

        float platformX = platformController.gameObject.transform.position.x;
        platformController.InteractPlatform(collX < platformX);

        blockManager.OnInteraction();
    }
}
