using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBlock_Explosive : Block {

    private BlockParticlePool ParticlePool;

    private float explosionForce;
    private float explosionRadius;

    private float masterRadius = 0.7f;
    private float masterPower = 5f;

    protected override void Awake() {
        base.Awake();
        ParticlePool = FindAnyObjectByType<BlockParticlePool>();
    }

    public override void SetBlockData(Block block) {
        base.SetBlockData(block);
        explosionRadius = blockManager.interactionDataStruct.Get(Type, Size)._radius * masterRadius;
        explosionForce = blockManager.interactionDataStruct.Get(Type, Size)._power * masterPower;
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 블록과 충돌시 상호작용 합니다.
    ///         - 불 블록과 충돌시 폭발
    ///         - 물 블록과 충돌시 속성 삭제
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
                    //불 블록
                    explosion();
                    RemoveBlock();
                    collision.gameObject.GetComponent<Block>().RemoveBlock();
                }
                else if (collision.collider.CompareTag(blockTag[0])) {
                    //물 블록 - 무속성 태그로 변경
                    RemoveType();
                    Particle particle = ParticlePool.GetParticle(ParticleType.WATER);
                    particle.transform.position = transform.position;
                }
            }
        }
    }

    /// <summary>
    ///  <para>
    ///     작동 목적 : 불 블록과 충돌시 폭발을 일으킵니다.
    /// </para>
    /// <para>
    ///     작동 방식 : 주변 오브젝트에 폭발 효과를 줍니다.
    ///         주변 특정 범위 내의 오브젝트에 반동을 줍니다.
    /// </para>
    /// </summary>
    private void explosion() {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, 1 << 6);

        foreach (Collider2D collider in colliders) {
            Block tempBlock = collider.GetComponent<Block>();
            tempBlock.InteractionInvincible(Block.InteractionType.Explosive);
            if (!(tempBlock.Size == BlockSize.Huge)) {
                Rigidbody2D rigidbody = collider.GetComponent<Rigidbody2D>();
                if (rigidbody != null) {
                    Vector2 direction = (rigidbody.transform.position - transform.position).normalized;
                    rigidbody.AddForce(direction * explosionForce, ForceMode2D.Impulse);
                    // ForceMode2D.Impulse - 한순간에 큰 힘을 즉각적으로 가함
                }
            }
        }

        Particle particle = ParticlePool.GetParticle(ParticleType.BOOM);
        particle.transform.position = transform.position;
        switch (GetComponent<Block>().Size) {
            case BlockSize.Small: particle.transform.localScale = new Vector3(0.5f, 0.5f, 1f); break;
            case BlockSize.Medium: particle.transform.localScale = new Vector3(0.8f, 0.8f, 1f); break;
            case BlockSize.Big: particle.transform.localScale = new Vector3(1.2f, 1.2f, 1f); break;
        }
        blockManager.OnInteraction();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
