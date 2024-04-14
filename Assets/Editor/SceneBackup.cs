public class SceneBackup : UnityEditor.AssetModificationProcessor
{
    static string[] OnWillSaveAssets(string[] paths)
    {
        bool manualSave = AutoSave.IsManualSave;
        if (manualSave)
        {
            AutoSave.Backup();
        }

        return paths;
    }
}