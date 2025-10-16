using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "save.json");
    }

    public static void SaveData(string json)
    {
        File.WriteAllText(GetPath(), json);
    }

    public static string LoadData()
    {
        string path = GetPath();
        if(File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return null;
    }

    public static void DeleteData()
    {
        string path = GetPath();
        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
