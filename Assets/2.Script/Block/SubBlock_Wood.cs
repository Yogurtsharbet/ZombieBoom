using System.Collections;
using UnityEngine;

public class SubBlock_Wood : Block {

    private BlockParticlePool ParticlePool;

    private float burnRadius = 0f;
    private float masterRadius = 0.7f;

    protected override void Awake() {
        base.Awake();
        ParticlePool = FindAnyObjectByType<BlockParticlePool>();
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 나무 블록과 충돌시 블록들과 상호작용 합니다.
    ///         - 불 블록과 충돌시 연소
    ///         - 물 블록과 충돌시 무속성으로 변경
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
                    burn(collision);
                }
                else if (collision.collider.CompareTag(blockTag[0])) {
                    //무속성 태그로 변경
                    RemoveType();
                    Particle particle = ParticlePool.GetParticle(ParticleType.WATER);
                    particle.transform.position = transform.position;
                }
            }
        }
    }

    public override void SetBlockData(Block block) {
        base.SetBlockData(block);
        burnRadius = blockManager.interactionDataStruct.Get(Type, Size)._radius * masterRadius;
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 나무 블록이 불 블록과 만났을 때 일어나는 반응을 호출합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : size별로 나누어 반응을 호출합니다.
    /// </para>
    /// </summary>
    private void burn(Collision2D collision) {
        switch (_size) {
            case BlockSize.Small:
                burnSmall(collision);
                break;
            case BlockSize.Medium:
                burnMedium();
                break;
            case BlockSize.Big:
                burnBig();
                break;
        }
    }
    /// <summary>
    /// 소형 블록 - 해당 블록만 연소 합니다.
    /// </summary>
    private void burnSmall(Collision2D collision) {
        Particle burnParticle = ParticlePool.GetParticle(ParticleType.BURN);
        burnParticle.transform.position = collision.gameObject.transform.position;
        collision.gameObject.GetComponent<Block>().RemoveBlock();
        burnParticle = ParticlePool.GetParticle(ParticleType.BURN);
        burnParticle.transform.position = gameObject.transform.position;
        RemoveBlock();
    }

    /// <summary>
    /// 중형 블록 - 해당 블록의 일정 범위의 무속성 초대형 블록 이외의 블록을 연소시킵니다.
    /// </summary>
    private void burnMedium() {
        Particle burnParticle;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, burnRadius, 1 << 6);
        foreach (Collider2D collider in colliders) {
            Block tempBlock = collider.GetComponent<Block>();
            if (!(tempBlock.Type == BlockType.Corn && tempBlock.Size == BlockSize.Huge)) {
                burnParticle = ParticlePool.GetParticle(ParticleType.BURN);
                burnParticle.transform.position = collider.transform.position;
                burnParticle.GetComponent<Bone>().is_zombie = (tempBlock.Type == BlockType.Corn);
                Debug.Log(tempBlock.Type == BlockType.Corn);
                tempBlock.BlockToScore(true);
            }
        }
        burnParticle = ParticlePool.GetParticle(ParticleType.BURN);
        burnParticle.transform.position = gameObject.transform.position;
        RemoveBlock();
    }

    /// <summary>
    /// 대형 블록 - 해당 블록의 일정 범위의 무속성 초대형 블록 이외의 블록을 연소시킵니다.
    /// </summary>
    private void burnBig() {
        Particle burnParticle;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, burnRadius, 1 << 6);
        foreach (Collider2D collider in colliders) {
            Block tempBlock = collider.GetComponent<Block>();
            if (!(tempBlock.Type == BlockType.Corn && tempBlock.Size == BlockSize.Huge)) {
                burnParticle = ParticlePool.GetParticle(ParticleType.BURN);
                burnParticle.transform.position = collider.transform.position;
                burnParticle.GetComponent<Bone>().is_zombie = (tempBlock.Type == BlockType.Corn);
                Debug.Log(tempBlock.Type == BlockType.Corn);
                tempBlock.BlockToScore(true);
            }
        }
        burnParticle = ParticlePool.GetParticle(ParticleType.BURN);
        burnParticle.transform.position = gameObject.transform.position;
        RemoveBlock();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, burnRadius);
    }
}
