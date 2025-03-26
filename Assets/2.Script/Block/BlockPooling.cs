using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// <para>Block ObjectPooling을 위한 Class</para>
/// <para>UnityEngine의 ObjectPool을 사용해 구현합니다.</para>
/// <para><see cref="blockPrefabs"/> : BlockManager에서 초기화된 prefab List를 가져옵니다.</para>
/// <para><see cref="blockPool"/> : 각 blockPrefab에 대응하는 ObjectPool이 저장된 Dictionary.</para>
/// </summary>
public class BlockPooling : MonoBehaviour {
    private Block[] blockPrefabs;
    private Dictionary<Block, IObjectPool<Block>> blockPool;
    public Dictionary<Block, IObjectPool<Block>> BlockPool => blockPool;

    private List<Block> activeBlock;
    public List<Block> ActiveBlock => activeBlock;

    private void Awake() {
        InitBlockPool();
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : BlockPool을 초기화합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : blockPrefabs의 Length 만큼 Unity Pool을 생성
    ///                 생성된 Pool을 Dictionary에 등록
    /// </para>
    /// </summary>
    private void InitBlockPool() {
        activeBlock = new List<Block>();
        blockPrefabs = FindAnyObjectByType<BlockManager>().BlockPrefabs;
        blockPool = new Dictionary<Block, IObjectPool<Block>>();
        for (int i = 0; i < blockPrefabs.Length; i++) {
            Block eachBlock = blockPrefabs[i];
            blockPool.Add(eachBlock,
                new ObjectPool<Block>(
                    createFunc: () => CreateBlock(eachBlock),
                    actionOnGet: OnGet,
                    actionOnRelease: OnRelease,
                    actionOnDestroy: DestroyBlock,
                    maxSize: 20
                    )
                );
        }
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : Pool에 Block이 부족할 때 호출됩니다.
    /// </para>
    /// </summary>
    /// <param name="block">Instantiate 대상으로 지정할 blockPrefab</param>
    /// <returns>지정한 객체를 Instantitate한 <see cref="Block"/>을 반환</returns>
    private Block CreateBlock(Block block) { 
        foreach(var eachBlock in blockPrefabs) {
            if (eachBlock == block) {
                Block newBlock = Instantiate(block, this.transform);
                newBlock.SetBlockData(block);
                newBlock.InitPooling(blockPool[block]);
                return newBlock;
            }
        }
        throw new System.ArgumentException("[ERROR] : Specified block is not existed!");
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : Pool에서 Block을 꺼낼 때 호출됩니다.
    /// </para>
    /// </summary>
    /// <param name="block">Pool 에서 꺼낸 Block이 자동으로 전달됩니다.</param>
    private void OnGet(Block block) { 
        block.ReturnType();
        block.killFlag = false;
        block.isInteracting = false;
        block.transform.rotation = Quaternion.identity;

        block.gameObject.SetActive(true);
        block.isColliderEnabled = false;
        block.gameObject.layer = 0;
        block.center.enabled = false;
        activeBlock.Add(block);
        Debug.Log($"{block.name} get");
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : Block이 Pool로 돌아갈 때 호출됩니다.
    /// </para>
    /// </summary>
    /// <param name="block">Pool 에서 꺼낸 Block이 자동으로 전달됩니다.</param>
    private void OnRelease(Block block) {
        block.gameObject.SetActive(false);
        block.isInteracting = false;
        block.killFlag = false;
        activeBlock.Remove(block);
        Debug.Log($"{block.name} release");
    }

    private void DestroyBlock(Block block) {
        Destroy(block.gameObject);
    }

}
