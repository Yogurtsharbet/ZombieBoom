using BlockName = System.String;
using BlockMass = System.Single;
using BlockScore = System.UInt16;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class InteractionDataStruct {
    public InteractionData[] Data;

    public InteractionData Get(BlockType type, BlockSize size) {
        foreach (InteractionData each in Data)
            if (each._type == type &&
                each._size == size) 
                return each;
        return null;
    }
}

[System.Serializable]
public class InteractionData {
    public BlockType _type;
    public BlockSize _size;
    public float _radius;
    public float _power;
}

[System.Serializable]
public class BlockDataStruct {
    public BlockData[] blocks;
}

[System.Serializable]
public class BlockData {
    public BlockName _name;
    public BlockType _type;
    public BlockSize _size;
    public BlockMass _mass;
    public BlockScore _score;

    public BlockData() { }

    public BlockData(Block block) {
        _name = block.Name;
        _type = block.Type;
        _size = block.Size;
        _mass = block.Mass;
        _score = block.Score;
    }
}

[System.Serializable]
public class BlockModuleDataStruct {
    public List<BlockModuleData> blockModules;
    public int moduleCount { get; private set; }
    public void SetModuleCount(int count) { moduleCount = count; }

    public static BlockModuleDataStruct Load() {
        TextAsset dataFile = Resources.Load<TextAsset>("BlockModuleData");
        if (dataFile == null) {
            Application.Quit();
            throw new FileNotFoundException("[ERROR] : Fatal. BlockModuleData file is not exist!");
        }
        return JsonUtility.FromJson<BlockModuleDataStruct>(dataFile.text);
    }

    //public static void Save() {
    //    // For Debugging
    //    BlockModuleDataStruct blockModuleData = new BlockModuleDataStruct();
    //    blockModuleData.blockModules = new List<BlockModuleData>();
    //    blockModuleData.blockModules.Add(new BlockModuleData());
    //    blockModuleData.blockModules.Add(new BlockModuleData());
    //    blockModuleData.blockModules[0].blockModule.Add(new BlockModule("불 블록", 2));
    //    blockModuleData.blockModules[0].blockModule.Add(new BlockModule("무속성 소형블록", 5));
    //    blockModuleData.blockModules[1].blockModule.Add(new BlockModule("물 블록", 4));
    //    blockModuleData.blockModules[1].blockModule.Add(new BlockModule("전기 블록", 4));

    //    string json = JsonUtility.ToJson(blockModuleData, true);
    //    string path = Path.Combine(Application.dataPath, "Resources/BlockModuleData.json");
    //    try {
    //        File.WriteAllText(path, json);
    //    }
    //    catch (IOException e) {
    //        throw new FileNotFoundException("[ERROR] : Save is not completed.", e);
    //    }
    //}

    //public void Shuffle() {
    //    for (int i = blockModules.Count - 1; i > 0; i--) {
    //        int k = Random.Range(0, i + 1);
    //        (blockModules[i], blockModules[k]) = (blockModules[k], blockModules[i]);
    //    }
    //}
}

[System.Serializable]
public class BlockModuleData {
    public List<BlockModule> blocks = new List<BlockModule>();
}

[System.Serializable]
public class BlockModule {
    public BlockType blockType;
    public BlockSize blockSize;
    public int blockChance;

    public BlockModule() { }

    public BlockModule(BlockType blockType, BlockSize blockSize, int blockChance) {
        this.blockType = blockType;
        this.blockSize = blockSize;
        this.blockChance = blockChance;
    }
}
