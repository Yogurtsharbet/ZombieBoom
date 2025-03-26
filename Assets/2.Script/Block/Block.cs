using UnityEngine;
using UnityEngine.Pool;
using BlockName = System.String;
using BlockMass = System.Single;
using BlockScore = System.UInt16;
using System.Collections;
using System.Collections.Concurrent;

public enum BlockType {
    Devil, Tuna, Police, Popcorn, Butter, Corn
}

public enum BlockSize {
    Small, Medium, Big, Huge
}

/// <summary>
/// <para>Block 최상위 Class</para>
/// <para>Block의 공통 속성을 정의합니다.</para>
/// <para>각 속성 Block들은 이 Block Class를 상속받습니다.</para>
/// <para> 공통 속성 : </para>
/// <para><see cref="Name"/> : 블록 이름</para>
/// <para><see cref="Type"/> : 블록 속성</para>
/// <para><see cref="Size"/> : 블록 크기</para>
/// <para><see cref="Mass"/> : 블록 무게</para>
/// <para><see cref="Score"/> : 블록 소멸 시 획득 점수</para>
/// <para><see cref="isActive"/> : True일 때만 속성 상호작용이 가능합니다.</para>
/// </summary>
public class Block : MonoBehaviour {
    // Block all Tag Arr
    protected static string[] blockTag = { "WaterBlock", "FireBlock", "ElectricBlock", "ExplosiveBlock", "WoodBlock", "NoneBlock" };

    // Block local variables
    protected BlockName _name;
    protected BlockType _type;
    protected BlockSize _size;
    protected BlockMass _mass;
    protected BlockScore _score;
    protected bool isActive;
    public Vector3 blockScale_OnGrab;
    public Vector3 blockScale_OnCreate;

    // Block Properties
    public BlockName Name => _name;
    public BlockType Type => _type;
    public BlockSize Size => _size;
    public BlockMass Mass => _mass;
    public BlockScore Score => _score;

    // Block Components
    protected Rigidbody2D blockRigid;
    protected Collider2D blockCollider;
    [SerializeField] private Collider2D[] compositedColliders = new Collider2D[2];
    public bool isColliderEnabled{
        get{
            if (blockCollider != null) return blockCollider.enabled;
            else return false;
        }
        set{
            if(blockCollider != null){
                if (blockCollider.GetType().Equals(typeof(CompositeCollider2D))) for(int i = 0; i < compositedColliders.Length; i++) compositedColliders[i].enabled = value;
                blockCollider.enabled = value;
            }
        }
    }
    protected SpriteRenderer spriteRenderer;

    protected IObjectPool<Block> blockPool;
    public BlockManager blockManager;

    protected int blockLayer = 6;
    private int platformLayer = 29;

    public bool isInteracting;
    public bool killFlag;

    public Transform Dispenser_Anchor_Left;
    public Transform Dispenser_Anchor_Right;
    public Transform Dispenser_Anchor_Origin;
    public Collider2D center;

    protected virtual void Awake() {
        blockRigid = GetComponent<Rigidbody2D>();
        blockCollider = GetComponent<Collider2D>();
        TryGetComponent(out spriteRenderer);
        blockManager = FindAnyObjectByType<BlockManager>();

        center = transform.GetChild(0).GetComponent<Collider2D>();
    }

    protected void OnEnable() {
        InitBlock();
    }
    protected void OnDisable(){
        OnBlockDisable?.Invoke();
    }
    public System.Action OnBlockDisable;

    /// <summary>
    /// <para>
    ///     작동 목적 : BlockPooling에서 Instantiate 한 뒤 호출합니다.
    ///                 생성된 Block이 Release할 때 Pool을 참조할 수 있도록 Pool을 설정해줍니다.
    /// </para>
    /// </summary>
    /// <param name="blockPool">각 Block에 맞는 BlockPool</param>
    public void InitPooling(IObjectPool<Block> blockPool) {
        this.blockPool = blockPool;
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : Block이 생성될 때 호출합니다.
    ///                 생성 이후 Drop되기 전까지 다른 Block과 상호작용하지 못하도록 설정합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : Block의 RigidBody를 Kinematic으로 설정
    ///                 isActive를 False로 설정
    ///     </para>
    /// </summary>
    public void InitBlock() {
        blockRigid.bodyType = RigidbodyType2D.Kinematic;
        isActive = false;
        killFlag = false;
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : Block을 떨어트릴 때 호출합니다.
    ///                 상호작용이 가능한 상태로 변경하고 물리를 적용합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : Block의 RigidBody를 Dynamic으로 설정합니다.
    ///                 isActive를 True로 설정
    /// </para>
    /// </summary>
    public void DropBlock() {
        blockRigid.bodyType = RigidbodyType2D.Dynamic;
        isColliderEnabled = true;
        gameObject.layer = blockLayer;
        center.enabled = true;
        isActive = true;
        InteractionInvincible(InteractionType.Dropping);
        blockManager.AddTotalMass(_mass);
        transform.localScale = blockScale_OnGrab;
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : Block이 소멸 조건에 있을 때 호출합니다.
    ///                 점수 획득 처리를 하고 Block을 Pool로 돌려보냅니다.
    /// </para>
    /// <para>
    ///     작동 방식 : blockPool에서 Release 하면 OnRealse 를 통해서 active False.
    /// </para>
    /// </summary>
    /// <param name="isBurn">연소 처리 시 True. Default = False</param>
    public void BlockToScore(bool isBurn = false) {
        if (isBurn) {
            Manager_Score.Instance.AddBlockScore(110);
        }
        else {
            Manager_Score.Instance.AddBlockScore(Score);
        }
        blockManager.RemoveTotalMass(_mass);
        blockPool.Release(this);
    }

    public void RemoveBlock() {
        blockManager.RemoveTotalMass(_mass);
        if (gameObject.activeSelf)
            blockPool.Release(this);
    }

    public void RemoveType() {
        gameObject.tag = blockTag[5];
        if(spriteRenderer != null) spriteRenderer.color = Color.gray;
        OnRemoveType?.Invoke();
    }
    public System.Action OnRemoveType; 

    public void ReturnType() {
        gameObject.tag = blockTag[(int)_type];
        if(spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    public void SetBlockData(BlockData blockData) {
        if (blockManager == null) Awake();

        _name = blockData._name;
        _type = blockData._type;
        _size = blockData._size;
        _mass = blockData._mass;
        _score = blockData._score;
    }

    public virtual void SetBlockData(Block block) {
        _name = block._name;
        _type = block._type;
        _size = block._size;
        _mass = block._mass;
        _score = block._score;

        block.GetComponent<Rigidbody2D>().mass = _mass;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if ((collision.gameObject.layer == blockLayer || collision.gameObject.layer == platformLayer)
            && this.gameObject.layer == blockLayer
            && !killFlag && !isInteracting) {
            killFlag = true;
        }
        MergeBlock(collision);
    }

    private void MergeBlock(Collision2D collision) {
        if (collision.collider.CompareTag(this.tag)
            && ((!this.CompareTag(blockTag[5]) || blockManager.IsWetAbleMerge) || this.Type == BlockType.Corn)
            && (this.Type != BlockType.Tuna && this.Type != BlockType.Devil && this.Type != BlockType.Police) ) {

            Block targetBlock = collision.collider.GetComponent<Block>();
            if (targetBlock.Type == this.Type && targetBlock.Size == this.Size && this.Size != BlockSize.Huge
                && this.gameObject.activeSelf && targetBlock.gameObject.activeSelf) {
        
                if (GetInstanceID() < targetBlock.GetInstanceID()) {
                    
                    RemoveBlock();
                    targetBlock.RemoveBlock();
                    Block newBlock;

                    //WARNING :: BigCat Block의 Index를 BlockPrefabs의 맨 마지막으로 설정.
                    //WARNING :: 만약 Block이 추가된다면 index 수정 필요
                    if ((this.Type != BlockType.Corn) && (this.Size == BlockSize.Big)) newBlock = blockManager.GetBlock(blockManager.BlockPrefabs[blockManager.BlockPrefabs.Length - 1]);
                    else newBlock = blockManager.GetBlock(this.Type, this.Size + 1);
        
                    newBlock.transform.position = (transform.position + targetBlock.transform.position) * 0.5f;
                    newBlock.DropBlock();
        
                    Manager_MonoSingleton_Audio.instance.SFX_Clip_IndexToPlay = 2;
                }
            }
        }

        // LEGACY CODE : Lv3 Block changes to Lv4 None Type Block
        //*****************************************************************************************************************//
        //if (collision.collider.CompareTag(this.tag)) {
        //    Block targetBlock = collision.collider.GetComponent<Block>();
        //    if ((this.Type != BlockType.Fire && this.Type != BlockType.Water && this.Type != BlockType.Electric)
        //        && targetBlock.Type == this.Type
        //        && targetBlock.Size == this.Size) {
        //        if ((Size == BlockSize.Small || Size == BlockSize.Medium) ||
        //            (Size == BlockSize.Big && Type == BlockType.None)) {
        //            RemoveBlock();

        //            if (GetInstanceID() < targetBlock.GetInstanceID()) {
        //                Block newBlock = blockManager.GetBlock(this.Type, this.Size + 1);
        //                newBlock.transform.position =
        //                    (transform.position + targetBlock.transform.position) * 0.5f;
        //                newBlock.DropBlock();
        //            }
        //        }
        //    }
        //}
        //*****************************************************************************************************************//
    }
    

    public enum InteractionType { Explosive, Electric, Dropping }
    private Coroutine invincibleCoroutine;
    public void InteractionInvincible(InteractionType type) {
        if (invincibleCoroutine != null)
            StopCoroutine(invincibleCoroutine);
        
        switch (type) {
            case InteractionType.Explosive:
                invincibleCoroutine = StartCoroutine(GetInvincible(blockManager.KillPendingTime));
                break;
            case InteractionType.Electric:
                invincibleCoroutine = StartCoroutine(GetInvincible(blockManager.KillPendingTime));
                break;
            case InteractionType.Dropping:
                invincibleCoroutine = StartCoroutine(GetInvincible(blockManager.DropPendingTime));
                break;
        }
    }

    public void ActiveKillFlag() {
        isInteracting = false;
        killFlag = true;
    }

    public void InactiveKillFlag() {
        isInteracting = true;
        killFlag = false;
    }

    protected IEnumerator GetInvincible(WaitForSeconds time) {
        InactiveKillFlag();
        yield return time;
        //ActiveKillFlag();
    }
}
