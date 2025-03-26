using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using BlockName = System.String;
using BlockMass = System.Single;
using BlockScore = System.UInt16;
using System.Collections;

/// <summary>
/// 
/// <para><see cref="blockPrefabs"/> : 모든 사용할 blockPrefab이 Inspector에서 지정됩니다.</para>
/// </summary>
[RequireComponent(typeof(BlockPooling))]
public class BlockManager : MonoBehaviour {
    [SerializeField] private Block[] blockPrefabs;
    [HideInInspector] public InteractionDataStruct interactionDataStruct;
    private BlockDataStruct blockDataStruct;

    private BlockModuleDataStruct blockModuleData;
    private Stack<Block> currentBlockModule;

    public Block[] BlockPrefabs => blockPrefabs;
    public BlockData[] BlockData => blockDataStruct.blocks;

    private PlayerBehavior playerBehavior;
    public Block GrabbedBlock { get { return playerBehavior.grabbedBlock; } }
    public Block NextBlock { get { return playerBehavior.nextBlock; } }
    public bool isInteractingElectric { get { return !playerBehavior.isReadyToGrab; } }

    private Dictionary<Block, IObjectPool<Block>> blockPool;
    public List<Block> activeBlock { get; private set; }

    private string jsonFile_Block = "BlockData";
    private string jsonFile_Interaction = "InteractionData";

    private float totalBlockMass = 0f;
    public float TotalBlockMass => totalBlockMass;

    private CurveLine deadLine;

    public enum Boolean {
        [InspectorName("젖음 블록도 투매치 가능")] True,
        [InspectorName("젖음 블록은 투매치 불가")] False
    };
    [Header("젖음 블록도 합성될 수 있는지 여부")]
    public Boolean isWetAbleMerge = Boolean.False;
    public bool IsWetAbleMerge { get { return isWetAbleMerge == Boolean.True; } }

    [Header("상호작용 후 무적 시간")]
    public float KillPending = 4f;
    public WaitForSeconds KillPendingTime;

    [Header("블록 낙하 직후 무적 시간")]
    public float DropPending = 2f;
    public WaitForSeconds DropPendingTime;

    private void Awake() {
        playerBehavior = FindAnyObjectByType<PlayerBehavior>();
        deadLine = FindAnyObjectByType<CurveLine>();
    }

    private void Start() {
        BlockPooling pooling = GetComponent<BlockPooling>();
        blockPool = pooling.BlockPool;
        activeBlock = pooling.ActiveBlock;

        KillPendingTime = new WaitForSeconds(KillPending);
        DropPendingTime = new WaitForSeconds(DropPending);

        InitBlocks();
        UpdateBlockModule();
    }

    /// <summary>
    /// <para>작동 목적 : 불러온 Block Data로 blockPrefabs의 Data를 초기 설정합니다.</para>
    /// </summary>
    /// <exception cref="OverflowException"></exception>
    public void InitBlocks() {
        LoadJsonData();

        // JSON에 저장된 Block의 개수와 Inspector에 할당된 Prefabs의 개수가 다르면 Exception
        if (blockPrefabs.Length != BlockData.Length) {
            Application.Quit();
            throw new OverflowException("[ERROR] : Block Prefabs and JSON Block Data have different length!");
        }

        for (int i = 0; i < blockPrefabs.Length; i++) {
            blockPrefabs[i].SetBlockData(BlockData[i]);
        }
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : JSON 포맷으로 저장된 Block Data를 불러옵니다.
    ///     작동 방식 : jsonFile string을 참조하여 Resource 폴더 내 JSON 파일을 Load.
    /// </para>
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    public void LoadJsonData() {
        // Resource 폴더에 저장된 JSON Block Data를 Load.
        TextAsset dataFile = Resources.Load<TextAsset>(jsonFile_Block);
        if (dataFile == null) {
            Application.Quit();
            throw new FileNotFoundException("[ERROR] : Fatal. Block Data file is not exist!");
        }
        blockDataStruct = JsonUtility.FromJson<BlockDataStruct>(dataFile.text);

        dataFile = Resources.Load<TextAsset>(jsonFile_Interaction);
        if (dataFile == null) {
            Application.Quit();
            throw new FileNotFoundException("[ERROR] : Fatal. Interaction Data file is not exist!");
        }
        interactionDataStruct = JsonUtility.FromJson<InteractionDataStruct>(dataFile.text);

        blockModuleData = BlockModuleDataStruct.Load();
        Shuffle(blockModuleData.blockModules);
        currentBlockModule = new Stack<Block>();
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 지정한 type, size의 블록 하나를 Pool에서 꺼내 반환합니다.
    /// </para>
    /// <para>
    ///     작동 방식 : blockPrefabs에서 type, size가 일치하는 Block을 골라 Pool에서 해당 객체를 Get
    /// </para>
    /// </summary>
    /// <param name="type">가져올 Block의 type</param>
    /// <param name="size">가져올 Block의 size</param>
    /// <returns>Pool에서 꺼낸 <see cref="Block"/>을 반환</returns>
    public Block GetBlock(BlockType type, BlockSize size) {
        foreach (Block eachBlock in blockPrefabs) {
            if (eachBlock.Type == type && eachBlock.Size == size)
                return blockPool[eachBlock].Get();
        }
        throw new ArgumentException($"[ERROR] : Specified type or size is not existed! [{type}, {size}]");
    }
    public Block GetBlock(Block block) {
        return blockPool[block].Get();

    }


    public void SetBlockPosition(BlockType type, BlockSize size, Vector3 position, bool addChecker) {
        Block blockToSet = GetBlock(type, size);
        blockToSet.transform.position = position;
        blockToSet.DropBlock();
        if (addChecker) blockToSet.gameObject.AddComponent<TutorialBlock>();
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 현재 모듈에서 블록 하나를 꺼내 해당 블록을 풀에서 생성합니다.
    /// </para>
    /// </summary>
    /// <returns>모듈에서 지정한 <see cref="Block"/>을 Pooling해서 반환</returns>

    public Block GetRandomModuleBlock() {
        Block block = currentBlockModule.Pop();
        UpdateBlockModule();
        return blockPool[block].Get();
    }

    /// <summary>
    /// <para>
    ///     작동 목적 : 현재 모듈이 비었으면 새로운 모듈을 가져옵니다.
    ///                 새로운 모듈에 존재하는 블록들을 랜덤한 순서로 현재 모듈에 추가합니다.
    ///                 모듈을 모두 가져왔으면, 모듈을 새로 섞은 뒤 다시 처음부터 가져옵니다.
    /// </para>
    /// </summary>
    private void UpdateBlockModule() {
        if (currentBlockModule.Count == 0) {
            List<Block> newModule = new List<Block>();

            var currentModule = blockModuleData.blockModules[blockModuleData.moduleCount].blocks;
            int totalCount = 0;
            foreach (var eachModule in currentModule)
                totalCount += eachModule.blockChance;
            if (totalCount == 0) {
                blockModuleData.SetModuleCount(blockModuleData.moduleCount + 1);
                if (blockModuleData.moduleCount == blockModuleData.blockModules.Count) {
                    blockModuleData.SetModuleCount(0);
                    Shuffle(blockModuleData.blockModules);
                }
                UpdateBlockModule();
                return;
            }

            foreach (BlockModule blocks in blockModuleData.blockModules[blockModuleData.moduleCount].blocks) {
                foreach (Block eachBlock in blockPrefabs) {
                    if (eachBlock.Type == blocks.blockType &&
                        eachBlock.Size == blocks.blockSize) {
                        for (int i = 0; i < blocks.blockChance; i++)
                            newModule.Add(eachBlock);
                        break;
                    }
                }
            }

            Shuffle(newModule);
            foreach (Block block in newModule)
                currentBlockModule.Push(block);

            blockModuleData.SetModuleCount(blockModuleData.moduleCount + 1);

            if (blockModuleData.moduleCount == blockModuleData.blockModules.Count) {
                blockModuleData.SetModuleCount(0);
                Shuffle(blockModuleData.blockModules);
            }
        }
    }

    /// <summary>
    ///     Drop시 Block Mass 추가
    /// </summary>
    /// <param name="mass">추가할 block mass</param>
    public void AddTotalMass(float mass) {
        totalBlockMass += mass;
    }

    /// <summary>
    ///     Block Active False시 Block Mass 삭제
    /// </summary>
    /// <param name="mass"></param>
    public void RemoveTotalMass(float mass) {
        totalBlockMass -= mass;
        if (totalBlockMass < 1) totalBlockMass = 1;
    }

    public void Shuffle<T>(IList<T> target) {
        for (int i = target.Count - 1; i > 0; i--) {
            int k = UnityEngine.Random.Range(0, i + 1);
            (target[i], target[k]) = (target[k], target[i]);
        }
    }

    private Coroutine resetInvincible;
    public void OnInteraction() {
        if(resetInvincible != null) 
            StopCoroutine(resetInvincible);
        resetInvincible = StartCoroutine(ResetInvincible());
    }

    private IEnumerator ResetInvincible() {
        deadLine.StopCheckDead();
        yield return KillPendingTime;
        deadLine.StartCheckDead();
    }
}
