using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.MyAssets.EditorTools
{
    /// <summary>
    /// 스크립트 파일을 지웠을 때 오브젝트에 남는 Missing Script 컴포넌트를 걷어낸다.
    ///
    /// 왜 필요한가: 스크립트를 지우면 그것이 붙어 있던 오브젝트에는 "깨진 컴포넌트"가 남는다.
    /// 이 상태에서는 <b>프리팹으로 만들거나 저장하는 것 자체가 거부된다</b>
    /// ("You are trying to save a Prefab with a missing script").
    /// 계층에서 하나씩 찾아 지울 수도 있지만, 씬과 프리팹에 흩어져 있으면 빠뜨리기 쉽다.
    ///
    /// UI 구조 개편(Phase 6)에서 스크립트 9개를 지우면서 필요해졌다.
    /// </summary>
    public static class MissingScriptCleaner
    {
        private const string SEARCH_FOLDER = "Assets/MyAssets";
        private const string MENU_SELECTION = "Tools/Magnetic Chess/Remove Missing Scripts (계층에서 선택한 오브젝트)";
        private const string MENU_ALL = "Tools/Magnetic Chess/Remove Missing Scripts (MyAssets 전체)";

        /// <summary>
        /// 계층에서 고른 오브젝트와 그 자식들만 훑는다.
        /// 프리팹으로 만들기 직전에 그 오브젝트만 훑고 싶을 때 쓴다.
        /// </summary>
        [MenuItem(MENU_SELECTION)]
        private static void RemoveFromSelection()
        {
            GameObject[] selection = Selection.gameObjects;

            if (selection.Length == 0)
            {
                Debug.LogWarning("[MissingScriptCleaner] 계층에서 오브젝트를 먼저 고른다.");
                return;
            }

            int removed = 0;
            StringBuilder report = new StringBuilder();

            for (int i = 0; i < selection.Length; i++)
            {
                removed += RemoveInHierarchy(selection[i], selection[i].name, report);
            }

            if (removed == 0)
            {
                Debug.Log("[MissingScriptCleaner] 고른 오브젝트에 Missing Script 없음.");
                return;
            }

            // 씬 오브젝트를 고쳤으면 씬이 더러워졌다고 알려야 저장 대상이 된다.
            EditorSceneManager.MarkSceneDirty(selection[0].scene);

            Debug.Log("[MissingScriptCleaner] " + removed + "개 제거\n\n" + report);
        }

        /// <summary>
        /// MyAssets 아래의 모든 프리팹과 씬을 훑는다. 씬을 열었다 닫으므로 시간이 걸린다.
        /// </summary>
        [MenuItem(MENU_ALL)]
        private static void RemoveFromAll()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
            {
                Debug.LogWarning("[MissingScriptCleaner] 씬 저장이 취소되어 중단했다.");
                return;
            }

            int removed = 0;
            StringBuilder report = new StringBuilder();

            try
            {
                removed += CleanPrefabs(report);
                removed += CleanScenes(report);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (removed == 0)
            {
                Debug.Log("[MissingScriptCleaner] MyAssets에 Missing Script 없음.");
                return;
            }

            AssetDatabase.SaveAssets();

            Debug.Log("[MissingScriptCleaner] 모두 " + removed + "개 제거\n\n" + report);
        }

        private static int CleanPrefabs(StringBuilder report)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SEARCH_FOLDER });
            int removed = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                EditorUtility.DisplayProgressBar("Missing Script 제거", path, (float)i / guids.Length);

                // 프리팹 자산은 내용을 바꾸려면 임시로 열어야 한다. 열면 반드시 언로드한다.
                GameObject root = PrefabUtility.LoadPrefabContents(path);

                try
                {
                    int count = RemoveInHierarchy(root, root.name, report, path);

                    if (count > 0)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        removed += count;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            return removed;
        }

        private static int CleanScenes(StringBuilder report)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { SEARCH_FOLDER });
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            int removed = 0;

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    EditorUtility.DisplayProgressBar("Missing Script 제거", path, (float)i / guids.Length);

                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    int count = 0;
                    GameObject[] roots = scene.GetRootGameObjects();
                    for (int r = 0; r < roots.Length; r++)
                    {
                        count += RemoveInHierarchy(roots[r], roots[r].name, report, path);
                    }

                    if (count > 0)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        removed += count;
                    }
                }
            }
            finally
            {
                // 검사 때문에 사용자가 열어 두었던 씬 구성이 바뀌면 안 된다.
                if (previousSetup != null && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }

            return removed;
        }

        private static int RemoveInHierarchy(GameObject go, string objectPath, StringBuilder report, string assetPath = null)
        {
            int removed = 0;

            // 프리팹 인스턴스의 컴포넌트는 인스턴스에서 지울 수 없다. 원본 프리팹을 고쳐야 한다.
            // 조용히 넘어가면 "지웠는데 그대로"가 되므로 어디를 고쳐야 하는지 알린다.
            if (PrefabUtility.IsPartOfPrefabInstance(go)
                && GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0)
            {
                string source = PrefabUtility.GetCorrespondingObjectFromSource(go) != null
                    ? AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go))
                    : "(원본을 찾지 못함)";

                report.AppendLine("[건너뜀] " + Prefix(assetPath) + objectPath
                    + "\n    프리팹 인스턴스다. 원본을 고쳐야 한다 -> " + source);
            }
            else
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

                if (count > 0)
                {
                    removed += count;
                    report.AppendLine(Prefix(assetPath) + objectPath + " -> " + count + "개 제거");
                }
            }

            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                removed += RemoveInHierarchy(child.gameObject, objectPath + "/" + child.name, report, assetPath);
            }

            return removed;
        }

        private static string Prefix(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) ? "" : assetPath + " : ";
        }
    }
}
