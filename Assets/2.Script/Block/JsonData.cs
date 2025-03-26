using System.IO;
using UnityEngine;

[System.Serializable]
public class JsonData<T> where T : class {
    public void Load(string fileName) {
        TextAsset dataFile = Resources.Load<TextAsset>(fileName);

        if (dataFile != null) {
            JsonUtility.FromJsonOverwrite(dataFile.text, this);
        }
        else {
            throw new FileNotFoundException("File is not found");
        }
    }

    public void Save(string path) {
        string jsonData = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, jsonData);
    }
}
