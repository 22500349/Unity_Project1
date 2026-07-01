using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int totalPoint = 0;
    public int stagePoint;
    public int stageIndex;
    public int health;

    // 씬 로드 후 동적으로 찾아서 연결됨
    [HideInInspector] public PlayerMove player;
    [HideInInspector] public Image[] UIhealth;
    [HideInInspector] public TextMeshProUGUI UI_point;
    [HideInInspector] public TextMeshProUGUI UI_stage;
    [HideInInspector] public GameObject RestartBtn;

    static readonly string[] stageScenes = { "Stage1", "Stage2", "Stage3", "ClearStage" };

    int maxHealth;
    GameObject resetBtnCanvas;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        maxHealth = health;

        // 씬에서 직접 플레이할 때 stageIndex를 씬 이름으로부터 자동 설정
        string sceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < stageScenes.Length; i++)
        {
            if (stageScenes[i] == sceneName)
            {
                stageIndex = i;
                break;
            }
        }

        CreateResetButton();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void CreateResetButton()
    {
        resetBtnCanvas = new GameObject("ResetButtonCanvas");
        resetBtnCanvas.transform.SetParent(transform);
        Canvas canvas = resetBtnCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        resetBtnCanvas.AddComponent<CanvasScaler>();
        resetBtnCanvas.AddComponent<GraphicRaycaster>();

        GameObject btnGO = new GameObject("ResetBtn");
        btnGO.transform.SetParent(resetBtnCanvas.transform, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-16f, 16f);
        rt.sizeDelta = new Vector2(100f, 36f);

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(FullRestart);

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRt = textGO.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        TextMeshProUGUI label = textGO.AddComponent<TextMeshProUGUI>();
        label.text = "초기화";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 18;
        label.color = Color.white;

        resetBtnCanvas.SetActive(false);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 비활성 오브젝트 포함해서 이름으로 찾기
    static GameObject FindByName(string name)
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == name) return t.gameObject;
        return null;
    }

    static readonly string[] stageScenesWithReset = { "Stage1", "Stage2", "Stage3" };

    // 씬이 로드될 때마다 UI와 플레이어를 새로 찾아서 연결
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 초기화 버튼: Stage1·2·3에서만 표시
        if (resetBtnCanvas != null)
        {
            bool show = System.Array.IndexOf(stageScenesWithReset, scene.name) >= 0;
            resetBtnCanvas.SetActive(show);
        }

        if (scene.name == "StartScene") return;

        player = FindFirstObjectByType<PlayerMove>(FindObjectsInactive.Include);

        GameObject life1 = FindByName("Life1");
        GameObject life2 = FindByName("Life2");
        GameObject life3 = FindByName("Life3");
        GameObject point = FindByName("Point");
        GameObject stage = FindByName("Stage");

        UIhealth = new Image[3];
        UIhealth[0] = life1 != null ? life1.GetComponent<Image>() : null;
        UIhealth[1] = life2 != null ? life2.GetComponent<Image>() : null;
        UIhealth[2] = life3 != null ? life3.GetComponent<Image>() : null;
        UI_point = point != null ? point.GetComponent<TextMeshProUGUI>() : null;
        UI_stage = stage != null ? stage.GetComponent<TextMeshProUGUI>() : null;
        RestartBtn = FindByName("재시작");

        if (RestartBtn != null)
        {
            Button btn = RestartBtn.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Restart);
            }
            RestartBtn.SetActive(false);
        }

        // 체력 UI 상태 복원
        for (int i = 0; i < UIhealth.Length; i++)
        {
            if (UIhealth[i] == null) continue;
            UIhealth[i].color = i < health ? Color.white : new Color(1, 0, 0, 0.4f);
        }

        if (UI_stage != null)
            UI_stage.text = scene.name == "ClearStage" ? "CLEAR!" : "STAGE " + (stageIndex + 1);
    }

    void Update()
    {
        if (UI_point != null)
            UI_point.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        totalPoint += stagePoint;
        stagePoint = 0;
        stageIndex++;

        if (stageIndex < stageScenes.Length)
        {
            SceneManager.LoadScene(stageScenes[stageIndex]);
        }
        else
        {
            // ClearStage에서 피니시 → 게임 완전 클리어
            if (RestartBtn != null)
            {
                RestartBtn.SetActive(true);
                TextMeshProUGUI btnText = RestartBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "GAME CLEAR";
            }
        }
    }

    public void HealthDown()
    {
        Debug.Log($"HealthDown 호출됨 - 현재 health: {health}");
        if (health > 1)
        {
            health--;
            Debug.Log($"health 감소 → {health}");
            if (UIhealth != null && UIhealth[health] != null)
                UIhealth[health].color = new Color(1, 0, 0, 0.4f);
        }
        else
        {
            Debug.Log("게임오버 처리");
            if (UIhealth != null && UIhealth[0] != null)
                UIhealth[0].color = new Color(1, 0, 0, 0.4f);

            player.OnDie();
            if (RestartBtn != null) RestartBtn.SetActive(true);
        }
    }

    public Vector3 spawnPoint = new Vector3(1, 3, -1);

    public void PlayerReporsition()
    {
        foreach (var t in FindObjectsByType<MapTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            t.ResetTrigger();
        foreach (var t in FindObjectsByType<MonsterTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            t.ResetTrigger();

        player.transform.position = spawnPoint;
        player.VelocityZero();
    }

    // 게임오버 후 재시작 — 죽은 스테이지에서 다시 시작
    public void Restart()
    {
        stagePoint = 0;
        health = maxHealth;

        Time.timeScale = 1;
        SceneManager.LoadScene(stageScenes[stageIndex]);
    }

    // 오른쪽 하단 초기화 버튼 — Stage1부터 완전 초기화
    void FullRestart()
    {
        totalPoint = 0;
        stagePoint = 0;
        stageIndex = 0;
        health = maxHealth;

        Time.timeScale = 1;
        SceneManager.LoadScene("Stage1");
    }
}
