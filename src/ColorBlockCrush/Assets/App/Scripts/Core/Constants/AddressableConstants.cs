namespace App.Scripts.Core.Constants
{
    public static class AddressableConstants
    {
        public const string UI_ASSETS_CONFIG = "Assets/App/Addressables/StaticData/Ui/UIAssetsConfig.asset";
        public static string LevelAddress(int levelId) => $"level-{levelId}";
    }
}
