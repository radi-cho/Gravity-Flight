using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsInitializer : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID
    string appId = "ca-app-pub-8093575736477454~8256749146";
#elif UNITY_IPHONE
    string appId = "ca-app-pub-1535751300111201~3592763175";
#else
        string appId = "unexpected_platform";
#endif

        MobileAds.SetiOSAppPauseOnBackground(true);
        MobileAds.Initialize(appId);
    }
}

public class InterstitialController : MonoBehaviour
{
    public static InterstitialAd interstitial;

    public InterstitialController()
    {
        LoadAd();
    }

    public void LoadAd()
    {
        if (interstitial != null) interstitial.Destroy();

        // Prepare interstitial
#if UNITY_ANDROID
    string adUnitId = "ca-app-pub-8093575736477454/8444451400";
#elif UNITY_IPHONE
    string adUnitId = "ca-app-pub-1535751300111201/7698216068";
#else
        string adUnitId = "unexpected_platform";
#endif

        interstitial = new InterstitialAd(adUnitId);
        interstitial.OnAdLoaded += HandleOnAdLoaded;
        interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        interstitial.OnAdOpening += HandleOnAdOpened;
        interstitial.OnAdClosed += HandleOnAdClosed;
        interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        AdRequest request = new AdRequest.Builder().AddTestDevice("78BF671EE6BEA2A0483C3CFB2A2E01B1").Build();
        interstitial.LoadAd(request);
    }

    public static void ShowAd()
    {
        if (interstitial.IsLoaded() && Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("monetization_enabled").BooleanValue)
        {
            interstitial.Show();
        }
    }

    public void HandleOnAdLoaded(object sender, EventArgs args) { }
    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) { }
    public void HandleOnAdOpened(object sender, EventArgs args) { }
    public void HandleOnAdClosed(object sender, EventArgs args) { LoadAd(); }
    public void HandleOnAdLeavingApplication(object sender, EventArgs args) { }
}

public class RewardedController : MonoBehaviour
{
    public static RewardBasedVideoAd rewardedAd;
    static Controller controller;

    public RewardedController()
    {
        rewardedAd = RewardBasedVideoAd.Instance;
        rewardedAd.OnAdLoaded += HandleRewardBasedVideoLoaded;
        rewardedAd.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        rewardedAd.OnAdOpening += HandleRewardBasedVideoOpened;
        rewardedAd.OnAdStarted += HandleRewardBasedVideoStarted;
        rewardedAd.OnAdRewarded += HandleRewardBasedVideoRewarded;
        rewardedAd.OnAdClosed += HandleRewardBasedVideoClosed;
        rewardedAd.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        LoadAd();
    }

    public static void LoadAd()
    {
#if UNITY_ANDROID
    string adUnitId = "ca-app-pub-8093575736477454/3359645684";
#elif UNITY_IPHONE
    string adUnitId = "ca-app-pub-1535751300111201/2450415351";
#else
        string adUnitId = "unexpected_platform";
#endif

        AdRequest request = new AdRequest.Builder()
          .AddTestDevice("78BF671EE6BEA2A0483C3CFB2A2E01B1")
          .AddTestDevice("B2083EA48990D33B62A63D8F26C7DEFA")
          .Build();

        if (Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("monetization_enabled").BooleanValue) rewardedAd.LoadAd(request, adUnitId);
    }

    public static void ShowAd()
    {
        if (rewardedAd.IsLoaded() && Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("monetization_enabled").BooleanValue)
        {
            rewardedAd.Show();
        }
    }

    public static void Revive(Controller cntr)
    {
        controller = cntr;
        ShowAd();
    }

    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args) { }
    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args) { }
    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        if (controller != null)
        {
            controller.isAdRunning = true;
            SoundFX.musicOn = false;
            controller.backgroundGameMusic.Pause();
        }
    }
    public void HandleRewardBasedVideoStarted(object sender, EventArgs args) { }
    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        if (controller != null)
        {
            controller.isAdRunning = false;
            SoundFX.musicOn = true;
            controller.backgroundGameMusic.Play();
            controller.CloseAd();
        }
        LoadAd();
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        if (controller != null)
        {
            controller.StartAgain(false);
            controller.pauseMotion = 0.01f;
            Controller.reward = true;
        }
        else
        {
            MainMenu.UpdateCrystals(MainMenu.crystals + 3, true);
        }

        LoadAd();
    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args) { }
}