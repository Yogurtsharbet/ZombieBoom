using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCenter : MonoBehaviour {
    private Block block;

    private static int deadLineLayer = 30;
    private static int scoreZoneLayer = 31;

    private void Awake() {
        block = GetComponentInParent<Block>();
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == scoreZoneLayer)
            block.BlockToScore();

    }
    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.layer == deadLineLayer && block.killFlag
            && block != block.blockManager.NextBlock
            && block != block.blockManager.GrabbedBlock) {
            GameOver();
        }
    }

    private void GameOver() {
        FindAnyObjectByType<Manager_UI_Sub_ScoreAndGameover>().OnOpenGameover();
    }
}
