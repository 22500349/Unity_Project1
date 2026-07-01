using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class StageSceneSplitter : EditorWindow
{
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

    [MenuItem("Tools/DeathZone 자동 생성 (전체 씬)")]
    static void CreateDeathZones()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("플레이 모드에서는 실행할 수 없습니다.");
            return;
        }

        string[] scenePaths =
        {
            "Assets/Scenes/Stage1.unity",
            "Assets/Scenes/Stage2.unity",
            "Assets/Scenes/Stage3.unity",
            "Assets/Scenes/ClearStage.unity",
        };

        foreach (string scenePath in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 기존 DeathZone 제거
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "DeathZone")
                {
                    Object.DestroyImmediate(root);
                    break;
                }
            }

            // 새 DeathZone 생성
            var go = new GameObject("DeathZone");
            EditorSceneManager.MoveGameObjectToScene(go, scene);

            var dz = go.AddComponent<DeathZone>();
            dz.killY = -8f;

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{scenePath}: DeathZone 생성 완료");
        }

        Debug.Log("전체 씬 DeathZone 생성 완료!");
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
}
