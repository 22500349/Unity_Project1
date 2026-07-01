using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;

public class StageSceneSplitter : EditorWindow
{
    [MenuItem("Tools/스테이지 씬 분리 실행")]
    static void SplitScenes()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("플레이 모드에서는 실행할 수 없습니다.");
            return;
        }

        // StageNull을 단독으로 열기
        EditorSceneManager.OpenScene("Assets/Scenes/StageNull.unity", OpenSceneMode.Single);
        var stageNull = EditorSceneManager.GetSceneByName("StageNull");
        GameObject[] roots = stageNull.GetRootGameObjects();

        // 모든 씬에 공통으로 들어가야 할 오브젝트
        string[] sharedNames = { "player", "Canvas", "Global Light 2D", "EventSystem", "Main Camera" };

        // (StageNull의 오브젝트 이름, 타겟 씬 경로, GameManager 포함 여부)
        var stageMap = new (string stageObjName, string scenePath, bool includeGameManager)[]
        {
            ("Stage1",      "Assets/Scenes/Stage1.unity",     true),
            ("Stage2",      "Assets/Scenes/Stage2.unity",     false),
            ("Stage3",      "Assets/Scenes/Stage3.unity",     false),
            ("Stage_clear", "Assets/Scenes/ClearStage.unity", false),
        };

        foreach (var (stageObjName, scenePath, includeGM) in stageMap)
        {
            var targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            // 스테이지 고유 맵 오브젝트 복사
            CopyRootObject(roots, stageObjName, targetScene);

            // 공유 오브젝트 복사
            foreach (string name in sharedNames)
                CopyRootObject(roots, name, targetScene);

            // GameManager는 Stage1에만 (DontDestroyOnLoad로 이후 씬에 살아남음)
            if (includeGM)
                CopyRootObject(roots, "Game Manager", targetScene);

            EditorSceneManager.SaveScene(targetScene);
            EditorSceneManager.CloseScene(targetScene, true);
            Debug.Log($"완료: {scenePath}");
        }

        Debug.Log("모든 씬 분리 완료! Stage1~ClearStage를 확인하세요.");
    }

    [MenuItem("Tools/Build Settings에 씬 등록")]
    static void RegisterScenes()
    {
        var scenes = new[]
        {
            "Assets/Scenes/StartScene.unity",
            "Assets/Scenes/Stage1.unity",
            "Assets/Scenes/Stage2.unity",
            "Assets/Scenes/Stage3.unity",
            "Assets/Scenes/ClearStage.unity",
        };

        var buildScenes = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            buildScenes[i] = new EditorBuildSettingsScene(scenes[i], true);

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("Build Settings 씬 등록 완료!");
    }

    [MenuItem("Tools/Spike 태그 일괄 수정 (Enemy → Spike)")]
    static void FixSpikeTags()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("플레이 모드에서는 실행할 수 없습니다.");
            return;
        }

        string[] scenePaths =
        {
            "Assets/Scenes/StageNull.unity",
            "Assets/Scenes/Stage1.unity",
            "Assets/Scenes/Stage2.unity",
            "Assets/Scenes/Stage3.unity",
            "Assets/Scenes/ClearStage.unity",
        };

        int totalFixed = 0;

        foreach (string scenePath in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int fixedInScene = 0;

            // 씬 내 모든 GameObject 검색 (비활성 포함)
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Spike" && t.gameObject.CompareTag("Enemy"))
                    {
                        t.gameObject.tag = "Spike";
                        fixedInScene++;
                    }
                }
            }

            if (fixedInScene > 0)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"{scenePath}: Spike 태그 {fixedInScene}개 수정 완료");
                totalFixed += fixedInScene;
            }
            else
            {
                Debug.Log($"{scenePath}: 수정할 Spike 없음");
            }
        }

        Debug.Log($"태그 수정 완료! 총 {totalFixed}개 수정됨.");
    }

    static void CopyRootObject(GameObject[] roots, string name, UnityEngine.SceneManagement.Scene target)
    {
        GameObject original = System.Array.Find(roots, go => go.name == name);
        if (original == null)
        {
            Debug.LogWarning($"'{name}' 오브젝트를 StageNull에서 찾지 못했습니다. 건너뜁니다.");
            return;
        }

        // 비활성 상태여도 복사할 수 있도록 잠깐 활성화
        bool wasActive = original.activeSelf;
        original.SetActive(true);
        GameObject copy = Object.Instantiate(original);
        original.SetActive(wasActive);

        copy.name = original.name;
        copy.SetActive(true);
        EditorSceneManager.MoveGameObjectToScene(copy, target);
    }
}
