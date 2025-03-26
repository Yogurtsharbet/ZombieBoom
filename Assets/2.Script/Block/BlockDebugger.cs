using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDebugger : MonoBehaviour {
    private BlockManager blockManager;
    private PlayerBehavior playerBehavior;

    public bool 물블록;
    public bool 불블록;
    public bool 전기블록;
    public bool 나무_소형블록;
    public bool 나무_중형블록;
    public bool 나무_대형블록;
    public bool 가폭_소형블록;
    public bool 가폭_중형블록;
    public bool 가폭_대형블록;
    public bool 무속성_소형블록;
    public bool 무속성_중형블록;
    public bool 무속성_대형블록;
    public bool 무속성_초대형블록;

    private List<bool> boolList = new List<bool>();

    private void Awake() {
        blockManager = FindAnyObjectByType<BlockManager>();
        playerBehavior = FindAnyObjectByType<PlayerBehavior>();

        boolList.Add(물블록);
        boolList.Add(불블록);
        boolList.Add(전기블록);
        boolList.Add(나무_소형블록);
        boolList.Add(나무_중형블록);
        boolList.Add(나무_대형블록);
        boolList.Add(가폭_소형블록);
        boolList.Add(가폭_중형블록);
        boolList.Add(가폭_대형블록);
        boolList.Add(무속성_소형블록);
        boolList.Add(무속성_중형블록);
        boolList.Add(무속성_대형블록);
        boolList.Add(무속성_초대형블록);
    }

    private void Update() {
        if (물블록) {
            물블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Devil, BlockSize.Small));
        }
        if (불블록) {
            불블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Tuna, BlockSize.Small));
        }
        if (전기블록) {
            전기블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Police, BlockSize.Small));
        }
        if (나무_소형블록) {
            나무_소형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Butter, BlockSize.Small));
        }
        if (나무_중형블록) {
            나무_중형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Butter, BlockSize.Medium));
        }
        if (나무_대형블록) {
            나무_대형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Butter, BlockSize.Big));
        }
        if (가폭_소형블록) {
            가폭_소형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Popcorn, BlockSize.Small));
        }
        if (가폭_중형블록) {
            가폭_중형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Popcorn, BlockSize.Medium));
        }
        if (가폭_대형블록) {
            가폭_대형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Popcorn, BlockSize.Big));
        }
        if (무속성_소형블록) {
            무속성_소형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Corn, BlockSize.Small));
        }
        if (무속성_중형블록) {
            무속성_중형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Corn, BlockSize.Medium));
        }
        if (무속성_대형블록) {
            무속성_대형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Corn, BlockSize.Big));
        }
        if (무속성_초대형블록) {
            무속성_초대형블록 = false;
            playerBehavior.DebugBlock(blockManager.GetBlock(BlockType.Corn, BlockSize.Huge));
        }
    }
}