using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.MyAssets.EditorTools
{
    /// <summary>
    /// MyAssets의 씬·프리팹을 현재 직렬화 형식으로 다시 써낸다.
    ///
    /// 왜 필요한가: [FormerlySerializedAs]는 에셋을 <b>불러올 때</b> 옛 필드명을 새 필드명으로 옮겨
    /// 주지만 그 결과는 메모리에만 있다. 파일에 새 이름으로 남으려면 다시 저장되어야 한다.
    /// 그런데 프리팹은 편집 모드에 들어갔다 나오는 것만으로는 변경됨(dirty) 표시가 되지 않아
    /// Ctrl+S가 아무것도 쓰지 않고, Reimport All은 Library 아티팩트만 다시 만들 뿐이다.
    ///
    /// 씬과 프리팹은 방법이 다르다:
    ///  · 씬   — ForceReserializeAssets가 실제로 다시 써준다.
    ///  · 프리팹 — ForceReserializeAssets는 "직렬화 버전이 그대로면" 건너뛴다. 필드명만 바뀐
    ///            경우가 여기 해당해서 파일이 갱신되지 않는다. 그래서 내용을 불러와
    ///            (이때 FormerlySerializedAs가 적용된다) 그대로 다시 저장하는 왕복을 쓴다.
    /// </summary>
    public static class ForceReserializeMyAssets
    {
        private const string SEARCH_FOLDER = "Assets/MyAssets";
        private const string MENU_PATH = "Tools/Magnetic Chess/Reserialize MyAssets (필드명 이관 확정)";

        [MenuItem(MENU_PATH)]
        private static void Reserialize()
        {
            int sceneCount = ReserializeScenes();
            int prefabCount = ReserializePrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Reserialize] 씬 " + sceneCount + "개, 프리팹 " + prefabCount + "개를 다시 써냈다.");
        }

        private static int ReserializeScenes()
        {
            List<string> paths = CollectPaths("t:Scene");

            if (paths.Count > 0)
            {
                // .meta는 건드리지 않는다. GUID가 바뀌면 씬 참조가 전부 끊어진다.
                AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeAssets);
            }

            return paths.Count;
        }

        private static int ReserializePrefabs()
        {
            List<string> paths = CollectPaths("t:Prefab");
            int saved = 0;

            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];

                EditorUtility.DisplayProgressBar("프리팹 재직렬화", path, (float)i / paths.Count);

                // LoadPrefabContents는 격리된 임시 씬에 프리팹을 펼친다.
                // 이 시점에 FormerlySerializedAs가 적용되어 옛 필드값이 새 필드로 옮겨진다.
                GameObject root = PrefabUtility.LoadPrefabContents(path);

                if (root == null)
                {
                    Debug.LogWarning("[Reserialize] 불러오지 못했다: " + path);
                    continue;
                }

                try
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);

                    if (success)
                    {
                        saved++;
                    }
                    else
                    {
                        Debug.LogWarning("[Reserialize] 저장 실패: " + path);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            EditorUtility.ClearProgressBar();

            return saved;
        }

        private static List<string> CollectPaths(string filter)
        {
            List<string> paths = new List<string>();
            string[] guids = AssetDatabase.FindAssets(filter, new[] { SEARCH_FOLDER });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (string.IsNullOrEmpty(path) == false && paths.Contains(path) == false)
                {
                    paths.Add(path);
                }
            }

            return paths;
        }
    }
}
