using System.Collections.Generic;
using App.Scripts.Core.Enums;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace App.Scripts.Gameplay.EditorTools
{
    public static class GameplayUiWiring
    {
        private const string UI_GROUP_NAME = "UI";
        private const string CONFIG_PATH = "Assets/App/Addressables/StaticData/Ui/UIAssetsConfig.asset";
        private const string COMPLETE_UXML = "Assets/App/Addressables/UI/UXML/Popups/LevelCompletePopup.uxml";
        private const string FAILED_UXML = "Assets/App/Addressables/UI/UXML/Popups/LevelFailedPopup.uxml";
        private const string GAME_SCREEN_UXML = "Assets/App/Addressables/UI/UXML/Screens/GameScreen.uxml";

        [MenuItem("Tools/Color Block Crush/Wire Gameplay UI")]
        public static void Wire()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError("[CBC] Addressable settings not found");
                return;
            }

            string completeGuid = MarkAddressable(settings, COMPLETE_UXML);
            string failedGuid = MarkAddressable(settings, FAILED_UXML);
            string gameScreenGuid = MarkAddressable(settings, GAME_SCREEN_UXML);

            if (completeGuid == null || failedGuid == null || gameScreenGuid == null)
                return;

            ScriptableObject config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(CONFIG_PATH);

            if (config == null)
            {
                Debug.LogError("[CBC] UIAssetsConfig not found");
                return;
            }

            SerializedObject serialized = new SerializedObject(config);
            SerializedProperty popups = serialized.FindProperty("_popups");

            if (popups == null)
            {
                Debug.LogError("[CBC] _popups property not found");
                return;
            }

            Dictionary<int, string> required = new Dictionary<int, string>
            {
                { (int)PopupId.LevelComplete, completeGuid },
                { (int)PopupId.LevelFailed, failedGuid }
            };

            foreach (KeyValuePair<int, string> pair in required)
                UpsertEntry(popups, pair.Key, pair.Value);

            SerializedProperty screens = serialized.FindProperty("_screens");

            if (screens != null)
                UpsertEntry(screens, (int)ScreenId.Game, gameScreenGuid);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CBC] Gameplay UI wired. Popup entries: {popups.arraySize}");
        }

        private static void UpsertEntry(SerializedProperty list, int id, string guid)
        {
            if (TryFindEntry(list, id, out SerializedProperty existing))
            {
                existing.FindPropertyRelative("Asset").FindPropertyRelative("m_AssetGUID").stringValue = guid;
                return;
            }

            list.InsertArrayElementAtIndex(list.arraySize);

            SerializedProperty element = list.GetArrayElementAtIndex(list.arraySize - 1);
            element.FindPropertyRelative("Id").enumValueIndex = id;
            element.FindPropertyRelative("Asset").FindPropertyRelative("m_AssetGUID").stringValue = guid;
        }

        private static bool TryFindEntry(SerializedProperty popups, int id, out SerializedProperty entry)
        {
            for (int i = 0; i < popups.arraySize; i++)
            {
                SerializedProperty element = popups.GetArrayElementAtIndex(i);

                if (element.FindPropertyRelative("Id").enumValueIndex != id)
                    continue;

                entry = element;
                return true;
            }

            entry = null;
            return false;
        }

        private static string MarkAddressable(AddressableAssetSettings settings, string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path);

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"[CBC] asset not found: {path}");
                return null;
            }

            AddressableAssetGroup group = settings.FindGroup(UI_GROUP_NAME);

            if (group == null)
                group = settings.DefaultGroup;

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = System.IO.Path.GetFileNameWithoutExtension(path);

            return guid;
        }
    }
}
