using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBlock_Water : Block {

    private BlockParticlePool ParticlePool;

    protected override void Awake(){
        base.Awake();
        ParticlePool = FindAnyObjectByType<BlockParticlePool>();
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 블록과 충돌시 상호작용 합니다.
    ///         - 불 블록과 충돌시 물과 해당 블록의 속성을 삭제합니다.
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
                if (collision.collider.CompareTag(blockTag[1])) {
                    RemoveType();
                    collision.gameObject.GetComponent<Block>().RemoveType();

                    Particle particle = ParticlePool.GetParticle(ParticleType.WATER);
                    particle.transform.position = transform.position;

                    particle = ParticlePool.GetParticle(ParticleType.WATER);
                    particle.transform.position = collision.transform.position;
                }
            }
        }
    }
}
