using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.MyAssets.EditorTools
{
    /// <summary>
    /// 프리팹 인스턴스의 오버라이드가 실제로 존재하는 필드를 가리키는지 검사한다.
    ///
    /// 왜 필요한가: 씬은 프리팹 인스턴스에서 바꾼 값을 m_Modifications에
    /// <b>필드 이름 문자열</b>(propertyPath)로 저장한다. 필드 이름을 바꾸면
    /// [FormerlySerializedAs]가 <b>직접 저장된 필드는 옮겨주지만 이 오버라이드는 옮겨주지 않는다.</b>
    /// 그러면 옛 이름 항목은 아무도 읽지 않는 고아가 되고, 인스펙터에서 연결해 두었던
    /// 참조가 조용히 비어버린다.
    ///
    /// 실제로 필드 규칙을 통일하면서 _fadeWindow · _animatorCamera 등 4개가 이렇게 끊어졌고,
    /// "필드명: 값" 형태만 대조한 검증은 이것을 잡지 못했다. 그래서 따로 본다.
    /// </summary>
    public static class PrefabOverrideValidator
    {
        private const string SEARCH_FOLDER = "Assets/MyAssets";
        private const string MENU_PATH = "Tools/Magnetic Chess/Validate Prefab Overrides";

        [MenuItem(MENU_PATH)]
        private static void ValidateFromMenu()
        {
            List<string> orphans = Validate();

            if (orphans.Count == 0)
            {
                Debug.Log("[OverrideValidator] 고아 오버라이드 없음.");
                return;
            }

            StringBuilder report = new StringBuilder();
            report.AppendLine("[OverrideValidator] 존재하지 않는 필드를 가리키는 오버라이드 " + orphans.Count + "건");
            report.AppendLine("필드 이름이 바뀌었는데 오버라이드가 따라가지 못한 경우다.");
            report.AppendLine("인스펙터에서 해당 값을 다시 연결한 뒤 씬을 저장하면 사라진다.");
            report.AppendLine();

            for (int i = 0; i < orphans.Count; i++)
            {
                report.AppendLine("  " + orphans[i]);
            }

            Debug.LogWarning(report.ToString());
        }

        public static List<string> Validate()
        {
            List<string> orphans = new List<string>();

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
            {
                Debug.LogWarning("[OverrideValidator] 씬 저장이 취소되어 검사를 중단했다.");
                return orphans;
            }

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { SEARCH_FOLDER });
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    EditorUtility.DisplayProgressBar("프리팹 오버라이드 검사", path, (float)i / guids.Length);

                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        ScanGameObject(root, path, orphans);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (previousSetup != null && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }

            return orphans;
        }

        private static void ScanGameObject(GameObject go, string assetPath, List<string> orphans)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
            {
                ScanInstance(go, assetPath, orphans);
            }

            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                ScanGameObject(transform.GetChild(i).gameObject, assetPath, orphans);
            }
        }

        private static void ScanInstance(GameObject instanceRoot, string assetPath, List<string> orphans)
        {
            PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(instanceRoot);

            if (modifications == null)
            {
                return;
            }

            Dictionary<Object, SerializedObject> cache = new Dictionary<Object, SerializedObject>();

            foreach (PropertyModification modification in modifications)
            {
                if (modification == null || modification.target == null
                    || string.IsNullOrEmpty(modification.propertyPath))
                {
                    continue;
                }

                if (cache.TryGetValue(modification.target, out SerializedObject serialized) == false)
                {
                    serialized = new SerializedObject(modification.target);
                    cache[modification.target] = serialized;
                }

                // 해당 경로가 대상 타입에 존재하지 않으면 아무도 읽지 않는 항목이다.
                if (serialized.FindProperty(modification.propertyPath) != null)
                {
                    continue;
                }

                orphans.Add(assetPath + "\n      " + GetPath(instanceRoot)
                            + "\n      -> " + modification.target.GetType().Name
                            + "." + modification.propertyPath + " 라는 필드가 없다");
            }
        }

        private static string GetPath(GameObject go)
        {
            string path = go.name;

            for (Transform parent = go.transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return path;
        }
    }
}
