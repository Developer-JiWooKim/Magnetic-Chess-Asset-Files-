using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Assets.MyAssets.EditorTools
{
    /// <summary>
    /// 씬·프리팹에 저장된 UnityEvent 바인딩(버튼 OnClick 등)이 실제 코드의 메소드를 가리키는지 검사한다.
    ///
    /// 왜 필요한가: UnityEvent는 호출할 메소드를 문자열로 저장한다. 메소드 이름을 바꾸거나 지우면
    /// 컴파일러는 아무것도 알려주지 않고, 실행 중에 버튼만 조용히 먹통이 된다.
    /// 실제로 Phase 0에서 TitleScene의 메뉴 버튼이 바로 이 이유로 동작하지 않았다.
    /// 이름 규칙 통일처럼 rename이 대량으로 일어나는 작업 전후에 반드시 돌린다.
    ///
    /// 검사 항목
    ///  1. 대상 오브젝트가 비어 있는 바인딩
    ///  2. 대상 타입에 존재하지 않는 메소드를 가리키는 바인딩
    ///  3. 스크립트가 사라진 컴포넌트(Missing Script) — 클래스 파일을 옮기며 .meta를 놓치면 생긴다
    /// </summary>
    public static class UnityEventBindingValidator
    {
        private const string SEARCH_FOLDER = "Assets/MyAssets";
        private const string MENU_PATH = "Tools/Magnetic Chess/Validate UnityEvent Bindings";
        private const string STRICT_MENU_PATH = "Tools/Magnetic Chess/Validate No Inspector Bindings";

        /// <summary>
        /// 켜면 "끊어진 바인딩"이 아니라 "남아 있는 바인딩"을 전부 보고한다.
        /// UI 배선을 코드로 옮기는 작업이 끝났는지 확인하는 용도다.
        /// 목표는 0건이고, 0건이 되면 인스펙터에 조용히 죽는 문자열이 하나도 남지 않는다.
        /// </summary>
        private static bool _strict;

        public enum Severity
        {
            Warning,
            Error,
        }

        public readonly struct BindingIssue
        {
            public readonly Severity Severity;
            public readonly string AssetPath;
            public readonly string ObjectPath;
            public readonly string Detail;

            public BindingIssue(Severity severity, string assetPath, string objectPath, string detail)
            {
                Severity = severity;
                AssetPath = assetPath;
                ObjectPath = objectPath;
                Detail = detail;
            }

            public override string ToString()
            {
                return AssetPath + "\n    " + ObjectPath + "\n    -> " + Detail;
            }
        }

        [MenuItem(STRICT_MENU_PATH)]
        private static void ValidateStrictFromMenu()
        {
            _strict = true;
            try
            {
                Report(Validate(), "인스펙터에 남은 UnityEvent 바인딩 없음.");
            }
            finally
            {
                _strict = false;
            }
        }

        [MenuItem(MENU_PATH)]
        private static void ValidateFromMenu()
        {
            Report(Validate(), "끊어진 UnityEvent 바인딩 없음.");
        }

        private static void Report(List<BindingIssue> issues, string emptyMessage)
        {
            if (issues.Count == 0)
            {
                Debug.Log("[BindingValidator] " + emptyMessage);
                return;
            }

            int errorCount = 0;
            int warningCount = 0;

            StringBuilder report = new StringBuilder();

            for (int i = 0; i < issues.Count; i++)
            {
                BindingIssue issue = issues[i];

                if (issue.Severity == Severity.Error)
                {
                    errorCount++;
                    report.Append("[오류] ");
                }
                else
                {
                    warningCount++;
                    report.Append("[경고] ");
                }

                report.AppendLine(issue.ToString());
                report.AppendLine();
            }

            string summary = "[BindingValidator] 오류 " + errorCount + "건, 경고 " + warningCount + "건\n\n" + report;

            if (errorCount > 0)
            {
                Debug.LogError(summary);
            }
            else
            {
                Debug.LogWarning(summary);
            }
        }

        /// <summary>
        /// 프로젝트 전체를 검사하고 발견한 문제를 돌려준다. 메뉴와 (나중에 추가할) 테스트가 함께 쓴다.
        /// </summary>
        public static List<BindingIssue> Validate()
        {
            List<BindingIssue> issues = new List<BindingIssue>();

            // 씬을 열어야 하므로, 저장하지 않은 변경이 있으면 먼저 사용자에게 묻는다.
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
            {
                Debug.LogWarning("[BindingValidator] 씬 저장이 취소되어 검사를 중단했다.");
                return issues;
            }

            try
            {
                ValidatePrefabs(issues);
                ValidateScenes(issues);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return issues;
        }

        private static void ValidatePrefabs(List<BindingIssue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SEARCH_FOLDER });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                EditorUtility.DisplayProgressBar("UnityEvent 바인딩 검사", path, (float)i / guids.Length);

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                // 프리팹 자산 안의 바인딩은 대상이 프리팹 바깥(씬의 매니저 등)을 가리킬 때
                // 자산 파일에는 비어 있는 상태로 저장되고, 씬 인스턴스가 오버라이드로 채운다.
                // 그래서 프리팹 쪽의 "대상 없음"은 오류가 아니라 경고로만 남긴다.
                ScanGameObject(prefab, path, prefab.name, true, issues);
            }
        }

        private static void ValidateScenes(List<BindingIssue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { SEARCH_FOLDER });

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    EditorUtility.DisplayProgressBar("UnityEvent 바인딩 검사", path, (float)i / guids.Length);

                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    GameObject[] roots = scene.GetRootGameObjects();
                    for (int r = 0; r < roots.Length; r++)
                    {
                        ScanGameObject(roots[r], path, roots[r].name, false, issues);
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
        }

        /// <summary>
        /// 엔진이나 다른 패키지가 만든 내부 오브젝트인가. 이런 오브젝트는 우리가 고칠 수 없고
        /// 빌드에도 들어가지 않으므로 검사에서 뺀다.
        /// (예: HDRP가 남긴 SceneIDMap. 이 프로젝트는 URP라 그 스크립트가 해석되지 않는다.)
        /// </summary>
        private static bool IsEngineInternal(GameObject go)
        {
            const HideFlags internalFlags =
                HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            return (go.hideFlags & internalFlags) != 0;
        }

        private static void ScanGameObject(GameObject go, string assetPath, string objectPath, bool isPrefabAsset, List<BindingIssue> issues)
        {
            if (IsEngineInternal(go))
            {
                return;
            }

            Component[] components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    // 스크립트 파일을 옮기면서 .meta를 함께 옮기지 않으면 여기에 걸린다.
                    issues.Add(new BindingIssue(Severity.Error, assetPath, objectPath,
                        "Missing Script (컴포넌트 " + i + "번) - 스크립트 .meta의 GUID가 끊어졌다"));
                    continue;
                }

                ScanComponent(components[i], assetPath, objectPath, isPrefabAsset, issues);
            }

            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                ScanGameObject(child.gameObject, assetPath, objectPath + "/" + child.name, isPrefabAsset, issues);
            }
        }

        private static void ScanComponent(Component component, string assetPath, string objectPath, bool isPrefabAsset, List<BindingIssue> issues)
        {
            // Button.m_OnClick처럼 이벤트 필드가 부모 타입에 private으로 선언된 경우가 많다.
            // GetFields는 부모의 private 필드를 돌려주지 않으므로 상속 계층을 직접 거슬러 올라간다.
            for (Type type = component.GetType(); type != null && type != typeof(object); type = type.BaseType)
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                for (int i = 0; i < fields.Length; i++)
                {
                    if (typeof(UnityEventBase).IsAssignableFrom(fields[i].FieldType) == false)
                    {
                        continue;
                    }

                    UnityEventBase unityEvent = fields[i].GetValue(component) as UnityEventBase;
                    if (unityEvent == null)
                    {
                        continue;
                    }

                    ScanEvent(unityEvent, component, fields[i].Name, assetPath, objectPath, isPrefabAsset, issues);
                }
            }
        }

        private static void ScanEvent(UnityEventBase unityEvent, Component owner, string fieldName,
            string assetPath, string objectPath, bool isPrefabAsset, List<BindingIssue> issues)
        {
            int callCount = unityEvent.GetPersistentEventCount();

            for (int i = 0; i < callCount; i++)
            {
                UnityEngine.Object target = unityEvent.GetPersistentTarget(i);
                string methodName = unityEvent.GetPersistentMethodName(i);

                string where = owner.GetType().Name + "." + fieldName + "[" + i + "]";

                bool hasTarget = target != null;
                bool hasMethod = string.IsNullOrEmpty(methodName) == false;

                if (hasTarget == false && hasMethod == false)
                {
                    // 인스펙터에서 슬롯만 늘리고 비워 둔 상태. 실제로 아무 일도 하지 않는다.
                    if (_strict)
                    {
                        issues.Add(new BindingIssue(Severity.Warning, assetPath, objectPath,
                            where + " -> 빈 슬롯 (인스펙터에 남은 바인딩)"));
                    }
                    continue;
                }

                if (hasTarget == false)
                {
                    Severity severity = isPrefabAsset ? Severity.Warning : Severity.Error;
                    string note = isPrefabAsset ? " (씬 인스턴스에서 오버라이드될 수 있음)" : "";

                    issues.Add(new BindingIssue(severity, assetPath, objectPath,
                        where + " -> '" + methodName + "'의 호출 대상이 비어 있다" + note));
                    continue;
                }

                if (hasMethod == false)
                {
                    issues.Add(new BindingIssue(Severity.Error, assetPath, objectPath,
                        where + " -> 대상(" + target.GetType().Name + ")은 있으나 메소드가 지정되지 않았다"));
                    continue;
                }

                if (HasCallableMethod(target.GetType(), methodName) == false)
                {
                    issues.Add(new BindingIssue(Severity.Error, assetPath, objectPath,
                        where + " -> " + target.GetType().Name + "에 '" + methodName + "' 메소드가 없다"));
                }
                else if (_strict)
                {
                    // 지금은 살아 있는 바인딩이다. 그래도 인스펙터에 문자열로 남아 있는 한
                    // 메소드 이름이 바뀌는 순간 조용히 죽는다. 코드로 옮겨야 할 대상이다.
                    issues.Add(new BindingIssue(Severity.Warning, assetPath, objectPath,
                        where + " -> " + target.GetType().Name + "." + methodName + " (인스펙터에 남은 바인딩)"));
                }
            }
        }

        /// <summary>
        /// UnityEvent가 호출할 수 있는 public 인스턴스 메소드가 있는지 본다.
        /// 프로퍼티 setter도 set_XXX라는 이름의 메소드로 저장되므로 함께 걸린다.
        /// 인자 형태까지는 보지 않는다 - 여기서 잡으려는 것은 "이름이 사라진" 경우다.
        /// </summary>
        private static bool HasCallableMethod(Type targetType, string methodName)
        {
            MethodInfo[] methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].Name == methodName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
