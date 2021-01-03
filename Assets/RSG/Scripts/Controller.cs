using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    float time = 0;
    float updateCount = 0;
    float tangentCount = 0;
    float reviveReamingTime = 0;
    float diff;
    float radius;
    float alpha;
    float x;
    float y;
    int reviveCount = 0;
    // direction: -1 - clockwise; 1 - counterclockwise
    float direction = 1f;
    public float speed;
    public float cameraSpeed = 1f;
    public float pauseMotion = 1f;
    public GameObject GamePlayObj;
    public GameObject GameOverObj;
    public GameObject starman;
    public GameObject orbitObject;
    GameObject GameOverControls;
    GameObject startingOrbitObj;
    GameObject Loading;
    public GameObject camera;
    public GameObject deadline;
    GameObject WatchAdButton;
    TextMeshProUGUI asteroidText;
    TextMeshProUGUI reviveCounterText;
    TextMeshProUGUI loadingText;

    Vector3 startingStarmanPositon;
    Vector3 startingRotationalPositon;
    Vector3 startingCameraPositon;
    Vector2 orbitPosition;

    public AudioSource backgroundGameMusic;
    public AudioSource soundFX;
    public AudioClip hitSound;
    public AudioClip switchSound;
    public AudioClip finishedSound;
    public AudioClip crystalSound;

    public static bool crystalSoundPlaying = false;
    public static bool isDying;
    public static bool finished;
    public static bool removeSFX;
    public static bool reward = false;
    public bool isAlive;
    public bool isAdRunning = false;
    public bool goTangent;
    bool begin = false;

    public LineFactory lineFactory;
    private Line connectingLine;
    public TutorialManager tutorial;

    void Awake()
    {
        startingOrbitObj = orbitObject;
        startingStarmanPositon = starman.transform.position;
        startingCameraPositon = camera.transform.position;
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        isDying = false;
        finished = false;
        isAlive = true;
        removeSFX = false;
        UpdateOrbit(orbitObject, true);

        GameOverControls = GameOverObj.transform.Find("Canvas/GameOverControls").gameObject;
        WatchAdButton = GameOverObj.transform.Find("Canvas/GameOverControls/WatchAd").gameObject;
        reviveCounterText =
            GameOverObj.transform.Find("Canvas/GameOverControls/ReviveCounter/Counter").gameObject
            .GetComponent<TextMeshProUGUI>();
        Loading = GameOverObj.transform.Find("Canvas/Loading").gameObject;
        loadingText = Loading.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        if (isDying) GameOver(hitSound);
        if (finished) GameOver(finishedSound);

        if (reviveReamingTime > 0)
        {
            reviveCounterText.SetText(reviveReamingTime.ToString("0.00"));
            if (!isAdRunning) reviveReamingTime -= Time.deltaTime;
        }
        else if (!isAlive)
        {
            reviveReamingTime = 4f;
            GotoMenu();
        }

        if (isAlive)
        {
            time += Time.deltaTime * speed * direction * pauseMotion;

            if (updateCount < 100)
            {
                radius = radius - (diff / 100);
                updateCount += 1;
            }

            orbitPosition = orbitObject.transform.position;
            if (tangentCount > 0)
            {
                tangentCount -= Time.deltaTime;
                asteroidText.SetText(tangentCount.ToString("0.0"));
            }
            else if (orbitObject.name == "asteroid")
            {
                Asteroid asteroid = orbitObject.GetComponent<Asteroid>();
                asteroid.asteroidText.SetText(asteroid.tangentCount.ToString("0.0"));
                begin = true;
                goTangent = true;
            }

            if (goTangent)
            {
                Vector3 normal = (new Vector2(x, y) - orbitPosition).normalized;
                Vector3 tangent = new Vector3(-normal.y, normal.x, 0) * direction;

                connectingLine.start = starman.transform.position;
                connectingLine.end = starman.transform.position;
                starman.transform.position = starman.transform.position + tangent * (Mathf.Abs(tangentCount) + Time.deltaTime) * 8 * pauseMotion;
                tangentCount = 0;
            }
            else
            {
                alpha = Mathf.Atan2(startingRotationalPositon.y - orbitPosition.y, startingRotationalPositon.x - orbitPosition.x);
                x = orbitPosition.x + Mathf.Cos(time + alpha) * radius;
                y = orbitPosition.y + Mathf.Sin(time + alpha) * radius;
                starman.transform.position = new Vector2(x, y);

                connectingLine.start = orbitPosition;
                connectingLine.end = starman.transform.position;
            }

            if (begin)
            {
                deadline.transform.position = new Vector2(deadline.transform.position.x, deadline.transform.position.y + cameraSpeed * 3f * Time.deltaTime * pauseMotion);
                camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y + cameraSpeed * 3f * Time.deltaTime * pauseMotion, -10f);
            }

            if (crystalSoundPlaying)
            {
                PlaySound(crystalSound);
                crystalSoundPlaying = false;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                if (
                    // time > 0.01 &&
                    hit.transform != null &&
                    hit.transform.gameObject &&
                    hit.transform.gameObject.tag.Contains("obstacle")
                )
                {
                    if (tutorial != null && tutorial.isTutorialEnabled && hit.transform.gameObject != orbitObject)
                    {
                        if (!tutorial.isTutorialActive) return;
                        tutorial.BeginRotation();
                        tutorial.isTutorialActive = false;

                        if (SceneManager.GetActiveScene().buildIndex - 2 == 3 && tutorial.nextTutorial != null)
                        {
                            tutorial.gameObject.SetActive(false);
                            tutorial = tutorial.nextTutorial.GetComponent<TutorialManager>();
                            tutorial.gameObject.SetActive(true);
                        }
                    }

                    pauseMotion = 1f;
                    UpdateOrbit(hit.transform.gameObject, false);
                    PlaySound(switchSound);
                }
            }
        }
    }

    void UpdateOrbit(GameObject newOrbit, bool isStart)
    {
        if (orbitObject != newOrbit || isStart)
        {
            startingRotationalPositon = starman.transform.position;
            orbitObject = newOrbit;
            orbitPosition = orbitObject.transform.position;
            radius = Vector2.Distance(startingRotationalPositon, orbitPosition);
            if (radius > 6f) radius = 6f;
            if (tutorial != null && tutorial.isTutorialEnabled && SceneManager.GetActiveScene().buildIndex - 2 == 3) radius = 2f;
            diff = radius - 2f;
            time = 0;
            updateCount = 0;
            goTangent = false;

            if (orbitObject.name == "asteroid")
            {
                Asteroid asteroid = orbitObject.GetComponent<Asteroid>();
                tangentCount = asteroid.tangentCount;
                asteroidText = asteroid.asteroidText;
                asteroidText.SetText(tangentCount.ToString("0.0"));
            }

            direction = orbitObject.tag.Contains("-") ? -1 : 1;

            if (isStart)
            {
                connectingLine = lineFactory.GetLine(orbitPosition, startingRotationalPositon, 0.02f, new Color(0.318f, 0.486f, 1));
            }
            else
            {
                begin = true;
            }
        }
    }

    public void PlaySound(AudioClip sound)
    {
        if (SoundFX.soundOn)
        {
            soundFX.clip = sound;
            soundFX.Play();
        }
    }

    void GameOver(AudioClip sound)
    {
        PlaySound(sound);
        if (isDying)
        {
            if (reviveCount == 0)
            {
                GamePlayObj.SetActive(false);
                GameOverObj.SetActive(true);

                if (tutorial == null)
                {
                    if (!RewardedController.rewardedAd.IsLoaded())
                    {
                        WatchAdButton.SetActive(false);
                        if (MainMenu.crystals < 5)
                        {
                            GameOverControls.SetActive(false);
                            Loading.SetActive(true);
                            loadingText.fontSize = 46;
                            loadingText.SetText("Reward not available!");
                            StartCoroutine(GotoMenuDelay(0.8f));
                        }
                    }

                    if (orbitObject.name == "asteroid")
                    {
                        GameOverControls.SetActive(false);
                        Loading.SetActive(true);
                        loadingText.fontSize = 48;
                        loadingText.SetText("You cannot revive from here!");
                        StartCoroutine(GotoMenuDelay(0.8f));
                    }
                }

                isAlive = false;
                isDying = false;
                finished = false;
                reviveReamingTime = 4f;
            }
            else
            {
                GotoMenu();
            }
        }
        else if (finished)
        {
            GamePlayObj.SetActive(false);
            GameOverObj.SetActive(true);
            GameOverControls.SetActive(false);
            Loading.SetActive(true);
            int levelId = SceneManager.GetActiveScene().buildIndex - 2;

            if (levelId == 3 && !PlayerPrefs.HasKey("tutorial_complete"))
            {
                loadingText.SetText("Tutorial Completed! Celebrate with 5 crystals!");
                MainMenu.UpdateCrystals(MainMenu.crystals + 5, true);
                PlayerPrefs.SetInt("tutorial_complete", 1);
                Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventTutorialComplete);
                Firebase.Messaging.FirebaseMessaging.SubscribeAsync("next_level");
                StartCoroutine(GotoMenuDelay(2.5f));
            }
            else
            {
                if (levelId == MainMenu.maxLevel)
                {
                    Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync("next_level");
                    Firebase.Messaging.FirebaseMessaging.SubscribeAsync("last_level");
                }

                loadingText.SetText("Level Completed!");
                StartCoroutine(GotoMenuDelay(0.8f));
            }

            finished = false;
        }
    }

    public void WatchAd()
    {
        RewardedController.Revive(this);
    }

    public void CloseAd()
    {
        if (reward)
        {
            StartCoroutine(ReviveMotionDelay(1.7f));
            reward = false;
        }
    }

    public void ReviveCrystals()
    {
        if (MainMenu.crystals >= 5)
        {
            MainMenu.UpdateCrystals(MainMenu.crystals - 5, true);
            StartAgain(false);

            pauseMotion = 0.01f;
            StartCoroutine(ReviveMotionDelay(1.2f));

            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                Firebase.Analytics.FirebaseAnalytics.EventSpendVirtualCurrency,
                new Firebase.Analytics.Parameter[] {
                    new Firebase.Analytics.Parameter (
                            Firebase.Analytics.FirebaseAnalytics.ParameterItemName, "revive"),
                        new Firebase.Analytics.Parameter (
                            Firebase.Analytics.FirebaseAnalytics.ParameterValue, 5),
                        new Firebase.Analytics.Parameter (
                            Firebase.Analytics.FirebaseAnalytics.ParameterVirtualCurrencyName, "crystal"),
                }
            );
        }
    }

    public void GotoMenu()
    {
        removeSFX = true;
        SceneManager.LoadScene(2);

        if (Random.Range(0, 3) != 1) InterstitialController.ShowAd();
    }

    IEnumerator GotoMenuDelay(float seconds)
    {
        cameraSpeed = 0;
        yield return new WaitForSeconds(seconds);
        GotoMenu();
    }

    public IEnumerator ReviveMotionDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pauseMotion = 1f;
    }

    public void StartAgain(bool isReplay)
    {
        time = 0;
        radius = 2f;
        begin = false;

        if (isReplay)
        {
            starman.transform.position = startingStarmanPositon;
            UpdateOrbit(startingOrbitObj, true);
            deadline.transform.position = new Vector2(deadline.transform.position.x, startingCameraPositon.y - 5.1f);
            camera.transform.position = startingCameraPositon;
        }
        else
        {
            reviveCount += 1;
            starman.transform.position = new Vector2(startingRotationalPositon.x, startingRotationalPositon.y);
            UpdateOrbit(orbitObject, true);
            radius = 2f;
            diff = 0;
            deadline.transform.position = new Vector2(deadline.transform.position.x, orbitPosition.y - 4.1f);
            camera.transform.position = new Vector3(camera.transform.position.x, orbitPosition.y + 1f, -10f);
        }

        isAlive = true;
        GamePlayObj.SetActive(true);
        GameOverObj.SetActive(false);

        if (tutorial != null && isReplay)
        {
            tutorial.BeginRotation();
            tutorial.isTutorialEnabled = true;
        }
    }
}