using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Unity.Editor;

public class MainMenu : MonoBehaviour
{
    public AudioSource backgroundMenuMusic;
    public static bool removeSFX;

    public Sprite audioIconOn;
    public Sprite audioIconOff;
    public Button musicButton;
    public Button soundButton;

    public static int crystals;
    public static string lastCompletedLevel;
    public TextMeshProUGUI crystalCounter;
    public TextMeshProUGUI levelCounter;
    public static TextMeshProUGUI crystalCounterStatic;
    public static TextMeshProUGUI levelCounterStatic;
    public GameObject CrystalStore;
    public GameObject SpaceshipStore;
    public GameObject StoreMessage;

    public static bool firstInstance = true;

    // TODO: UPDATE TO THE HIGHEST LEVEL
    public static int maxLevel = 28;

    public void Awake()
    {
        if (SoundFX.musicOn)
        {
            backgroundMenuMusic.Play();
        }

        crystalCounterStatic = crystalCounter;
        levelCounterStatic = levelCounter;
        PurchaseSpaceship.CrystalStore = CrystalStore;
        PurchaseSpaceship.SpaceshipStore = SpaceshipStore;
        PurchaseSpaceship.Message = StoreMessage;

        if (firstInstance)
        {
            new AdsInitializer();
            new InterstitialController();
            new RewardedController();

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus != Firebase.DependencyStatus.Available)
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                }

                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceivedFCM;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceivedFCM;

                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://project-starman.firebaseio.com/");

                System.Collections.Generic.Dictionary<string, object> firebaseDefaults =
                    new System.Collections.Generic.Dictionary<string, object>();
                firebaseDefaults.Add("monetization_enabled", true);
                Firebase.RemoteConfig.FirebaseRemoteConfig.SetDefaults(firebaseDefaults);

                FetchDataAsync();
            });

            firstInstance = false;
        }

        SoundFX.backgroundMenuMusic = backgroundMenuMusic;
        UpdateAudioIcons();
        UpdateLevelCounter();
        UpdateCrystalCounter();

        if (!PlayerPrefs.HasKey(Application.version))
        {
            if (PlayerPrefs.GetString("lastCompletedLevel", "1") != maxLevel.ToString())
            {
                Firebase.Messaging.FirebaseMessaging.SubscribeAsync("next_level");
                Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync("last_level");
            }
            PlayerPrefs.SetString(Application.version, "Done");
        }
    }

    public void Start()
    {
        removeSFX = false;
        UpdateAudioIcons();
    }

    public void OnTokenReceivedFCM(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceivedFCM(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }

    public Task FetchDataAsync()
    {
        Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(TimeSpan.Zero);
        return fetchTask.ContinueWith(FetchComplete);
    }

    void FetchComplete(Task fetchTask)
    {
        var info = Firebase.RemoteConfig.FirebaseRemoteConfig.Info;
        switch (info.LastFetchStatus)
        {
            case Firebase.RemoteConfig.LastFetchStatus.Success:
                Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched();
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Failure:
                /// Something went wrong!
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Pending:
                break;
        }
    }

    public void OpenScene(int scene)
    {
        removeSFX = true;
        SceneManager.LoadScene(scene);
    }

    public void UpdateAudioIcons()
    {
        if (SoundFX.musicOn)
        {
            musicButton.GetComponent<Image>().sprite = audioIconOn;
        }
        else
        {
            musicButton.GetComponent<Image>().sprite = audioIconOff;
        }

        if (SoundFX.soundOn)
        {
            soundButton.GetComponent<Image>().sprite = audioIconOn;
        }
        else
        {
            soundButton.GetComponent<Image>().sprite = audioIconOff;
        }
    }

    public static void UpdateCrystals(int amount, bool sync)
    {
        if (sync)
        {
            int crystalsSinceLastSync = PlayerPrefs.GetInt("since_last_sync", 0);
            PlayerPrefs.SetInt("since_last_sync", crystalsSinceLastSync + amount - crystals);
        }

        if (amount > crystals)
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(
                Firebase.Analytics.FirebaseAnalytics.EventEarnVirtualCurrency,
                new Firebase.Analytics.Parameter[] {
                    new Firebase.Analytics.Parameter (
                        Firebase.Analytics.FirebaseAnalytics.ParameterValue, amount - crystals),
                    new Firebase.Analytics.Parameter (
                        Firebase.Analytics.FirebaseAnalytics.ParameterVirtualCurrencyName, "crystal"),
                }
            );
        }

        crystals = amount;
        PlayerPrefs.SetInt("crystals", crystals);
        UpdateCrystalCounter();

        if (sync)
        {
            Sync.SessionSync();
        }
    }

    public static void UpdateCrystalCounter()
    {
        crystals = PlayerPrefs.GetInt("crystals", 0);
        crystalCounterStatic.SetText(crystals.ToString());
    }

    public void UpdateCrystalCounterInstance()
    {
        UpdateCrystalCounter();
    }

    public static void UpdateLevelCounter()
    {
        lastCompletedLevel = PlayerPrefs.GetString("lastCompletedLevel", "1");
        levelCounterStatic.SetText(lastCompletedLevel);
    }

    public void UpdateLevelCounterInstance()
    {
        UpdateLevelCounter();
    }
}