using UnityEngine;
using NeonRush;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Telemetry")]
    public int currentHealth = 100;
    public int maxHealth = 100;
    public float currentSpeed = 0f;
    public float raceTime = 0f;
    public int score = 0;
    public float trackProgress = 0f;
    public float distanceToFinish = 500f;
    public int shardsCollected = 0;
    public int totalShards = 0;

    [Header("Anti-Gravity")]
    public bool isAntiGravityActive = false;
    public string antiGravityStatus = "READY [SPACE]";
    public float antiGravityRatio = 1f;

    [Header("Drift System")]
    public bool isDrifting = false;
    public float driftRatio = 0f;
    public string driftStatus = "";

    [Header("Notification / Prompts")]
    public string promptText = "";
    private string notificationText = "";
    private float notificationTimer = 0f;

    [Header("Screen State")]
    public bool showVictory = false;
    public bool showGameOver = false;
    public string gameOverReason = "";
    public int victoryStars = 3;

    // Damage Flash
    private float hitFlashTimer = 0f;

    // GUI Styles
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle smallStyle;
    private GUIStyle promptStyle;
    private GUIStyle notificationStyle;
    private GUIStyle bannerStyle;
    private GUIStyle cardStyle;
    private Texture2D whiteTexture;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    void Update()
    {
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f)
            {
                notificationText = "";
            }
        }

        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
        }

        if ((showVictory || showGameOver) && Input.GetKeyDown(KeyCode.R))
        {
            SceneLoader.LoadRace();
        }
    }

    void InitStyles()
    {
        if (headerStyle != null) return;

        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        headerStyle.normal.textColor = new Color(0f, 0.95f, 1f);

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        labelStyle.normal.textColor = Color.white;

        smallStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft
        };
        smallStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

        promptStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        promptStyle.normal.textColor = new Color(1f, 0.95f, 0.2f);

        notificationStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        notificationStyle.normal.textColor = new Color(0f, 1f, 0.8f);

        bannerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        cardStyle = new GUIStyle(GUI.skin.box);
    }

    void OnGUI()
    {
        // HUD always shown
        
        InitStyles();

        // Damage red border flash
        if (hitFlashTimer > 0f)
        {
            Color flashCol = new Color(1f, 0f, 0f, Mathf.Clamp01(hitFlashTimer * 1.5f) * 0.45f);
            Color oldCol = GUI.color;
            GUI.color = flashCol;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTexture);
            GUI.color = oldCol;
        }

        if (showVictory)
        {
            DrawVictoryBanner();
            return;
        }

        if (showGameOver)
        {
            DrawGameOverBanner();
            return;
        }

        // --- SIMPLIFIED HUD: Speed, Position, Lap, Time, Boost, Power-up, Minimap ---
        DrawSimplifiedHUD();

        // Notifications
        if (!string.IsNullOrEmpty(notificationText))
        {
            DrawNotification();
        }
    }

    void DrawSimplifiedHUD()
    {
        float margin = 20f;
        
        // Top Left: Speed (large)
        GUIStyle speedStyle = new GUIStyle(GUI.skin.label);
        speedStyle.fontSize = 48;
        speedStyle.fontStyle = FontStyle.Bold;
        speedStyle.normal.textColor = new Color(0f, 0.9f, 1f);
        GUI.Label(new Rect(margin, margin, 200, 60), Mathf.RoundToInt(currentSpeed).ToString(), speedStyle);
        
        GUIStyle labelSmall = new GUIStyle(GUI.skin.label);
        labelSmall.fontSize = 18;
        labelSmall.normal.textColor = Color.white;
        GUI.Label(new Rect(margin + 90, margin + 20, 100, 30), "KM/H", labelSmall);
        
        // Top Center: Position & Lap
        RaceManager rm = FindAnyObjectByType<RaceManager>();
        if (rm != null)
        {
            int position = rm.GetPlayerPosition();
            int totalRacers = 4; // Default to 4 racers
            
            // Get lap info from RaceManager
            int currentLap = rm.GetPlayerLap();
            int totalLaps = rm.TotalLaps;
            
            GUIStyle posStyle = new GUIStyle(GUI.skin.label);
            posStyle.fontSize = 36;
            posStyle.fontStyle = FontStyle.Bold;
            posStyle.alignment = TextAnchor.UpperCenter;
            posStyle.normal.textColor = Color.yellow;
            
            GUI.Label(new Rect(Screen.width/2 - 150, margin, 300, 50), $"{position}/{totalRacers}", posStyle);
            
            GUIStyle lapStyle = new GUIStyle(GUI.skin.label);
            lapStyle.fontSize = 24;
            lapStyle.alignment = TextAnchor.UpperCenter;
            lapStyle.normal.textColor = Color.white;
            
            GUI.Label(new Rect(Screen.width/2 - 150, margin + 50, 300, 40), $"LAP {currentLap}/{totalLaps}", lapStyle);
        }
        
        // Top Right: Race Time
        GUIStyle timeStyle = new GUIStyle(GUI.skin.label);
        timeStyle.fontSize = 28;
        timeStyle.fontStyle = FontStyle.Bold;
        timeStyle.alignment = TextAnchor.UpperRight;
        timeStyle.normal.textColor = new Color(0f, 1f, 0.8f);
        
        int minutes = Mathf.FloorToInt(raceTime / 60f);
        int seconds = Mathf.FloorToInt(raceTime % 60f);
        GUI.Label(new Rect(Screen.width - 200 - margin, margin, 200, 40), $"{minutes:00}:{seconds:00}", timeStyle);
        
        // Bottom Left: Boost Bar
        {
            float boostX = margin;
            float boostY = Screen.height - 80 - margin;
            
            GUI.Label(new Rect(boostX, boostY - 25, 200, 25), "BOOST", labelStyle);
            
            Rect boostRect = new Rect(boostX, boostY, 200, 20);
            DrawBar(boostRect, antiGravityRatio, new Color(1f, 0.1f, 0.85f), new Color(0.2f, 0.2f, 0.2f, 0.8f));
            
            GUI.Label(new Rect(boostX, boostY + 25, 200, 25), antiGravityStatus, smallStyle);
        }
    }

    void DrawProgressBar(Rect rect)
    {
        GUI.Box(rect, GUIContent.none, cardStyle);
        GUILayout.BeginArea(new Rect(rect.x + 8, rect.y + 3, rect.width - 16, rect.height - 6));
        GUILayout.Label($"GRAND PRIX CIRCUIT: {(trackProgress * 100f):F0}%  |  REMAINING: {distanceToFinish:F0}m", smallStyle);
        Rect pRect = GUILayoutUtility.GetRect(rect.width - 16, 9);
        DrawBar(pRect, trackProgress, new Color(0f, 0.95f, 1f), new Color(0.2f, 0.2f, 0.2f, 0.8f));
        GUILayout.EndArea();
    }

    void DrawStatusPanel(Rect rect)
    {
        GUI.Box(rect, GUIContent.none, cardStyle);
        GUILayout.BeginArea(new Rect(rect.x + 8, rect.y + 6, rect.width - 16, rect.height - 12));

        GUILayout.Label("ANTI-GRAVITY RUSH", headerStyle);

        float healthPct = Mathf.Clamp01((float)currentHealth / maxHealth);
        Color healthColor = (healthPct > 0.5f) ? new Color(0.2f, 1f, 0.4f) : (healthPct > 0.25f ? Color.yellow : Color.red);
        GUILayout.Label($"HULL INTEGRITY: {currentHealth}/{maxHealth}", labelStyle);
        Rect barRect = GUILayoutUtility.GetRect(rect.width - 16, 9);
        DrawBar(barRect, healthPct, healthColor, new Color(0.2f, 0.2f, 0.2f, 0.8f));

        GUILayout.Space(2);

        Color agColor = isAntiGravityActive ? new Color(1f, 0.1f, 0.85f) : (antiGravityRatio >= 0.99f ? new Color(0f, 0.9f, 1f) : new Color(0.3f, 0.5f, 0.8f));
        GUILayout.Label($"HOVER BOOST: {antiGravityStatus}", labelStyle);
        Rect agBarRect = GUILayoutUtility.GetRect(rect.width - 16, 9);
        DrawBar(agBarRect, antiGravityRatio, agColor, new Color(0.2f, 0.2f, 0.2f, 0.8f));

        GUILayout.EndArea();
    }

    void DrawTelemetryPanel(Rect rect)
    {
        GUI.Box(rect, GUIContent.none, cardStyle);
        GUILayout.BeginArea(new Rect(rect.x + 8, rect.y + 6, rect.width - 16, rect.height - 12));
        GUILayout.Label($"TIME: {raceTime:F1}s", labelStyle);
        GUILayout.Label($"SCORE: {score} PTS", labelStyle);
        GUILayout.Label($"SHARDS: 🔷 {shardsCollected} / {totalShards}", labelStyle);
        GUILayout.Label($"SPEED: {currentSpeed:F0} KM/H", headerStyle);
        GUILayout.EndArea();
    }

    void DrawDriftGauge()
    {
        float w = 260f;
        float h = 32f;
        float x = (Screen.width - w) / 2f;
        float y = Screen.height - 95f;

        GUI.Box(new Rect(x, y, w, h), GUIContent.none, cardStyle);
        GUILayout.BeginArea(new Rect(x + 6, y + 4, w - 12, h - 8));

        Color driftCol = (driftRatio >= 0.8f) ? new Color(1f, 0.2f, 0.9f) : ((driftRatio >= 0.4f) ? new Color(1f, 0.6f, 0.1f) : new Color(0f, 0.9f, 1f));
        string status = (driftRatio >= 0.8f) ? "🔥 MEGA TURBO READY! 🔥" : ((driftRatio >= 0.4f) ? "⚡ MINI-TURBO CHARGED" : "DRIFTING...");
        GUILayout.Label(status, smallStyle);

        Rect barRect = GUILayoutUtility.GetRect(w - 12, 8);
        DrawBar(barRect, driftRatio, driftCol, new Color(0.2f, 0.2f, 0.2f, 0.8f));

        GUILayout.EndArea();
    }

    void DrawNotification()
    {
        float notifW = Mathf.Min(500f, Screen.width - 40f);
        float notifH = 40f;
        float notifX = (Screen.width - notifW) / 2f;
        float notifY = Screen.height * 0.35f;

        GUI.Box(new Rect(notifX, notifY, notifW, notifH), notificationText, notificationStyle);
    }

    void DrawPrompt()
    {
        if (string.IsNullOrEmpty(promptText)) return;

        float promptW = Mathf.Min(600f, Screen.width - 24f);
        float promptH = 34f;
        float promptX = (Screen.width - promptW) / 2f;
        float promptY = Screen.height - 48f;

        GUI.Box(new Rect(promptX, promptY, promptW, promptH), promptText, promptStyle);
    }

    void DrawVictoryBanner()
    {
        float w = Mathf.Min(520f, Screen.width - 40f);
        float h = 230f;
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f - 20f;

        GUI.Box(new Rect(x, y, w, h), GUIContent.none, cardStyle);

        GUILayout.BeginArea(new Rect(x + 20, y + 20, w - 40, h - 40));
        bannerStyle.normal.textColor = new Color(0.2f, 1f, 0.4f);
        GUILayout.Label("🏁 GRAND PRIX CHAMPION!", bannerStyle);
        GUILayout.Space(6);

        string starStr = (victoryStars == 3) ? "⭐⭐⭐ 3-STAR CYBER LEGEND!" : ((victoryStars == 2) ? "⭐⭐ 2-STAR RACER!" : "⭐ 1-STAR FINISHER!");
        GUILayout.Label(starStr, headerStyle);

        GUILayout.Space(6);
        GUILayout.Label($"FINAL TIME: {raceTime:F2} seconds", labelStyle);
        GUILayout.Label($"FINAL SCORE: {score} PTS", labelStyle);
        GUILayout.Label($"SHARDS COLLECTED: {shardsCollected} / {totalShards}", labelStyle);
        GUILayout.Space(10);
        GUILayout.Label("PRESS [R] TO RACE AGAIN", promptStyle);
        GUILayout.EndArea();
    }

    void DrawGameOverBanner()
    {
        float w = Mathf.Min(460f, Screen.width - 40f);
        float h = 170f;
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f - 20f;

        GUI.Box(new Rect(x, y, w, h), GUIContent.none, cardStyle);

        GUILayout.BeginArea(new Rect(x + 20, y + 20, w - 40, h - 40));
        bannerStyle.normal.textColor = Color.red;
        GUILayout.Label(gameOverReason, bannerStyle);
        GUILayout.Space(10);
        GUILayout.Label("PRESS [R] TO RESTART", promptStyle);
        GUILayout.EndArea();
    }

    void DrawBar(Rect rect, float fillPct, Color fillColor, Color bgColor)
    {
        Color oldColor = GUI.color;

        GUI.color = bgColor;
        GUI.DrawTexture(rect, whiteTexture);

        GUI.color = fillColor;
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * fillPct, rect.height);
        GUI.DrawTexture(fillRect, whiteTexture);

        GUI.color = oldColor;
    }

    public void ShowGameHUD()
    {
        showVictory = false;
        showGameOver = false;
    }

    public void ShowVictoryScreen(float time, int finalScore, int stars = 3, int shards = 0, int total = 0)
    {
        raceTime = time;
        score = finalScore;
        victoryStars = stars;
        shardsCollected = shards;
        totalShards = total;
        showVictory = true;
    }

    public void ShowGameOverScreen(string reason)
    {
        gameOverReason = reason;
        showGameOver = true;
    }

    public void UpdateHealth(int health, int max)
    {
        currentHealth = health;
        maxHealth = max;
    }

    public void UpdateSpeed(float speed)
    {
        currentSpeed = speed;
    }

    public void UpdateTimer(float time)
    {
        raceTime = time;
    }

    public void UpdateScore(int s)
    {
        score = s;
    }

    public void UpdateShards(int collected, int total)
    {
        shardsCollected = collected;
        totalShards = total;
    }

    public void UpdateDrift(bool drifting, float ratio)
    {
        isDrifting = drifting;
        driftRatio = ratio;
    }

    public void UpdateProgress(float progress, float remainingDist)
    {
        trackProgress = progress;
        distanceToFinish = remainingDist;
    }

    public void UpdateAntiGravity(bool active, float timer, float duration, int charges, int maxCharges)
    {
        isAntiGravityActive = active;
        if (active)
        {
            antiGravityStatus = $"ACTIVE ({timer:F1}s / 3.5s)";
            antiGravityRatio = Mathf.Clamp01(timer / 3.5f);
        }
        else if (charges > 0)
        {
            antiGravityStatus = $"{charges} CHARGE [SPACE: 3.5s]";
            antiGravityRatio = (float)charges / maxCharges;
        }
        else
        {
            antiGravityStatus = "EMPTY (0 CHARGES)";
            antiGravityRatio = 0f;
        }
    }

    public void ShowNotification(string text, float duration = 2.5f)
    {
        notificationText = text;
        notificationTimer = duration;
    }

    public void SetPrompt(string text)
    {
        promptText = text;
    }

    public void TriggerHitFlash()
    {
        hitFlashTimer = 0.35f;
    }
}
