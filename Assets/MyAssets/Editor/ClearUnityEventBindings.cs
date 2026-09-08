using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Assets.MyAssets.EditorTools
{
    /// <summary>
    /// 선택한 프리팹·씬에서 인스펙터에 저장된 UnityEvent 호출(OnClick 목록 등)을 비운다.
    ///
    /// 왜: 인스펙터 바인딩은 호출할 메소드 이름을 문자열로 저장해서, 이름이 바뀌거나
    /// 사라져도 컴파일러가 알려주지 않고 실행 중 버튼만 조용히 먹통이 된다.
    /// 배선을 코드로 옮긴 뒤 이 도구로 인스펙터 쪽을 비우면 그 실패 방식이 사라진다.
    ///
    /// 순서가 중요하다:
    ///   1. 코드에 AddListener 배선을 넣는다
    ///   2. 인스펙터에서 버튼 참조를 연결한다
    ///   3. <b>이 도구로 바인딩을 비운다</b>
    ///   4. 플레이 테스트
    /// 2와 3 사이에는 코드와 인스펙터 양쪽이 걸려 있어 클릭 한 번에 두 번 실행된다.
    /// 그 구간에서는 테스트하지 않는다.
    /// </summary>
    public static class ClearUnityEventBindings
    {
        private const string MENU_PATH = "Tools/Magnetic Chess/Clear UnityEvent Bindings (선택한 에셋)";

        [MenuItem(MENU_PATH)]
        private static void ClearSelected()
        {
            List<string> prefabPaths = new List<string>();
            List<string> scenePaths = new List<string>();

            foreach (UnityEngine.Object selected in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selected);

                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    prefabPaths.Add(path);
                }
                else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    scenePaths.Add(path);
                }
            }

            if (prefabPaths.Count == 0 && scenePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Clear UnityEvent Bindings",
                    "Project 창에서 프리팹이나 씬을 선택한 뒤 실행해야 한다.", "확인");
                return;
            }

            string summary = "프리팹 " + prefabPaths.Count + "개, 씬 " + scenePaths.Count + "개의\n"
                             + "인스펙터 UnityEvent 호출을 모두 비운다.\n\n"
                             + "코드 배선(AddListener)이 이미 들어가 있는지 확인했는가?";

            if (EditorUtility.DisplayDialog("Clear UnityEvent Bindings", summary, "비운다", "취소") == false)
            {
                return;
            }

            int cleared = 0;

            foreach (string path in prefabPaths)
            {
                cleared += ClearPrefab(path);
            }

            if (scenePaths.Count > 0)
            {
                cleared += ClearScenes(scenePaths);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ClearBindings] 호출 " + cleared + "개를 비웠다.");
        }

        private static int ClearPrefab(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            if (root == null)
            {
                Debug.LogWarning("[ClearBindings] 불러오지 못했다: " + path);
                return 0;
            }

            int cleared;

            try
            {
                cleared = ClearHierarchy(root, path);

                if (cleared > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);

                    if (success == false)
                    {
                        Debug.LogWarning("[ClearBindings] 저장 실패: " + path);
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return cleared;
        }

        private static int ClearScenes(List<string> paths)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
            {
                return 0;
            }

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            int cleared = 0;

            try
            {
                foreach (string path in paths)
                {
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    int inScene = 0;
                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        inScene += ClearHierarchy(root, path);
                    }

                    if (inScene > 0)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }

                    cleared += inScene;
                }
            }
            finally
            {
                if (previousSetup != null && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }

            return cleared;
        }

        private static int ClearHierarchy(GameObject go, string assetPath)
        {
            int cleared = 0;

            foreach (Component component in go.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                cleared += ClearComponent(component, assetPath);
            }

            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                cleared += ClearHierarchy(transform.GetChild(i).gameObject, assetPath);
            }

            return cleared;
        }

        private static int ClearComponent(Component component, string assetPath)
        {
            int cleared = 0;
            SerializedObject serialized = null;

            // Button.m_OnClick처럼 이벤트 필드가 부모 타입에 private으로 선언된 경우가 많아
            // 상속 계층을 거슬러 올라가며 찾는다(GetFields는 부모의 private을 돌려주지 않는다).
            for (Type type = component.GetType(); type != null && type != typeof(object); type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType) == false)
                    {
                        continue;
                    }

                    if (field.GetValue(component) is UnityEventBase unityEvent == false)
                    {
                        continue;
                    }

                    int count = unityEvent.GetPersistentEventCount();
                    if (count == 0)
                    {
                        continue;
                    }

                    serialized ??= new SerializedObject(component);

                    SerializedProperty calls = serialized.FindProperty(field.Name + ".m_PersistentCalls.m_Calls");
                    if (calls == null)
                    {
                        Debug.LogWarning("[ClearBindings] 호출 목록을 찾지 못했다: "
                                         + component.GetType().Name + "." + field.Name + " (" + assetPath + ")");
                        continue;
                    }

                    calls.ClearArray();
                    cleared += count;
                }
            }

            if (serialized != null)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return cleared;
        }
    }
}
