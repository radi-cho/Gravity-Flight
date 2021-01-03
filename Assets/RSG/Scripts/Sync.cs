using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class UserData
{
    public int crystals;
    public string lastCompletedLevel;
    public string spaceships;

    public UserData(int crystals, string lastCompletedLevel, string spaceships)
    {
        this.crystals = crystals;
        this.lastCompletedLevel = lastCompletedLevel;
        this.spaceships = spaceships;
    }
}

public class Sync : MonoBehaviour
{
    public static void UpdateLocal(int crystals, int lastCompletedLevel, string spaceships)
    {
        MainMenu.UpdateCrystals(crystals, false);

        for (int i = 0; i < lastCompletedLevel; i++)
        {
            PlayerPrefs.SetString("Level" + (i + 1), "Completed");
        }

        PlayerPrefs.SetString("lastCompletedLevel", lastCompletedLevel.ToString());
        MainMenu.lastCompletedLevel = lastCompletedLevel.ToString();

        foreach (string ship in spaceships.Split(','))
        {
            PlayerPrefs.SetString("Starman/" + ship, "purchased");
        }

        MainMenu.UpdateLevelCounter();
        UpdateStoreItems.UpdateAllItems();
    }

    public static void SessionSync()
    {
        if (Profile.signedIn && Profile.userId != null)
        {
            int crystalsTemp = 0;
            string lastLevelTemp = "";
            string spaceships = "rocket";

            FirebaseDatabase.DefaultInstance.GetReference("users/" + Profile.userId)
                .RunTransaction(userData =>
                {
                    Dictionary<string, object> user = userData.Value as Dictionary<string, object>;

                    if (user == null)
                    {
                        user = new Dictionary<string, object>();
                        user.Add("crystals", MainMenu.crystals);
                        user.Add("lastCompletedLevel", MainMenu.lastCompletedLevel);
                        user.Add("spaceships", spaceships);
                    }
                    else
                    {
                        user["lastCompletedLevel"] = Math.Max(
                            int.Parse(user["lastCompletedLevel"].ToString()),
                            int.Parse(MainMenu.lastCompletedLevel)
                        ).ToString();

                        lastLevelTemp = user["lastCompletedLevel"].ToString();

                        int crystalsSinceLastSync = PlayerPrefs.GetInt("since_last_sync", 0);
                        user["crystals"] = int.Parse(user["crystals"].ToString()) + crystalsSinceLastSync;
                        if (int.Parse(user["crystals"].ToString()) < 0) user["crystals"] = 0;
                        crystalsTemp = int.Parse(user["crystals"].ToString());
                        spaceships = user["spaceships"].ToString();
                    }

                    userData.Value = user;
                    return TransactionResult.Success(userData);
                }).ContinueWith((task) =>
                {
                    if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
                    {
                        PlayerPrefs.SetInt("since_last_sync", 0);
                        UpdateLocal(crystalsTemp, int.Parse(lastLevelTemp), spaceships);
                    }
                });
        }
    }

    public static void InitializeSync()
    {
        if (Profile.signedIn && Profile.userId != null)
        {
            string spaceships = "rocket";
            if (PlayerPrefs.HasKey("Starman/ufo")) spaceships += ",ufo";
            if (PlayerPrefs.HasKey("Starman/spaceship")) spaceships += ",spaceship";
            if (PlayerPrefs.HasKey("Starman/car")) spaceships += ",car";
            if (PlayerPrefs.HasKey("Starman/cow")) spaceships += ",cow";

            UserData userData = new UserData(
                MainMenu.crystals,
                PlayerPrefs.GetString("lastCompletedLevel", "1"),
                spaceships
            );

            string json = JsonUtility.ToJson(userData);

            FirebaseDatabase.DefaultInstance.RootReference
                .Child("users")
                .Child(Profile.userId)
                .SetRawJsonValueAsync(json);
        }
    }

    public static void ExistingSync()
    {
        if (Profile.signedIn && Profile.userId != null)
        {
            FirebaseDatabase.DefaultInstance.GetReference("users/" + Profile.userId)
                .RunTransaction(userData =>
                {
                    Dictionary<string, object> user = userData.Value as Dictionary<string, object>;
                    string spaceships = "";

                    if (user == null)
                    {
                        user = new Dictionary<string, object>();
                        user.Add("crystals", MainMenu.crystals);
                        user.Add("lastCompletedLevel", MainMenu.lastCompletedLevel);
                        spaceships = "rocket";
                        user.Add("spaceships", spaceships);
                    }
                    else
                    {
                        user["crystals"] = int.Parse(user["crystals"].ToString()) + MainMenu.crystals;
                        user["lastCompletedLevel"] = Math.Max(int.Parse(user["lastCompletedLevel"].ToString()), int.Parse(MainMenu.lastCompletedLevel)).ToString();

                        string[] spaceshipsArray = user["spaceships"].ToString().Split(',');
                        for (int i = 0; i < spaceshipsArray.Length; i++)
                        {
                            if (i > 0) spaceships += ",";
                            spaceships += spaceshipsArray[i];
                        }

                        if (PlayerPrefs.HasKey("Starman/ufo") && !spaceships.Contains("ufo")) spaceships += ",ufo";
                        if (PlayerPrefs.HasKey("Starman/spaceship") && !spaceships.Contains("spaceship")) spaceships += ",spaceship";
                        if (PlayerPrefs.HasKey("Starman/car") && !spaceships.Contains("car")) spaceships += ",car";
                        if (PlayerPrefs.HasKey("Starman/cow") && !spaceships.Contains("cow")) spaceships += ",cow";

                        user["spaceships"] = spaceships;
                    }

                    UpdateLocal(int.Parse(user["crystals"].ToString()), int.Parse(user["lastCompletedLevel"].ToString()), user["spaceships"].ToString());
                    userData.Value = user;

                    return TransactionResult.Success(userData);
                }).ContinueWith((task) =>
                {
                    if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
                    {
                        PlayerPrefs.SetInt("since_last_sync", 0);
                    }
                });
        }
    }

    public static void HandleSpaceshipPurchase(string item, int amount, Action<bool> callback)
    {
        if (Profile.signedIn && Profile.userId != null && !PurchaseSpaceship.purchasing)
        {
            PurchaseSpaceship.purchasing = true;

            int crystalsTemp = 0;
            string lastLevelTemp = "";
            string spaceships = "";

            FirebaseDatabase.DefaultInstance.GetReference("users/" + Profile.userId)
                .RunTransaction(userData =>
                {
                    Dictionary<string, object> user = userData.Value as Dictionary<string, object>;
                    spaceships = "";

                    if (user == null)
                    {
                        user = new Dictionary<string, object>();
                        user.Add("crystals", MainMenu.crystals);
                        user.Add("lastCompletedLevel", MainMenu.lastCompletedLevel);
                        spaceships = "rocket";
                        user.Add("spaceships", spaceships);
                    }
                    else
                    {
                        int newCrystals = int.Parse(user["crystals"].ToString()) - amount;
                        if (newCrystals >= 0)
                        {
                            user["crystals"] = newCrystals;
                            crystalsTemp = newCrystals;

                            string[] spaceshipsArray = user["spaceships"].ToString().Split(',');
                            for (int i = 0; i < spaceshipsArray.Length; i++)
                            {
                                if (i > 0) spaceships += ",";
                                spaceships += spaceshipsArray[i];
                            }

                            if (PlayerPrefs.HasKey("Starman/ufo") && !spaceships.Contains("ufo")) spaceships += ",ufo";
                            if (PlayerPrefs.HasKey("Starman/spaceship") && !spaceships.Contains("spaceship")) spaceships += ",spaceship";
                            if (PlayerPrefs.HasKey("Starman/car") && !spaceships.Contains("car")) spaceships += ",car";
                            if (PlayerPrefs.HasKey("Starman/cow") && !spaceships.Contains("cow")) spaceships += ",cow";

                            string newItem = item.Replace("Starman/", "");
                            if (!spaceships.Contains(newItem)) spaceships += "," + newItem;


                            user["spaceships"] = spaceships;
                        }
                        else
                        {
                            return TransactionResult.Abort();
                        }
                    }


                    userData.Value = user;
                    lastLevelTemp = user["lastCompletedLevel"].ToString();
                    return TransactionResult.Success(userData);
                }).ContinueWith((task) =>
                {
                    PurchaseSpaceship.purchasing = false;

                    if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
                    {
                        PlayerPrefs.SetInt("since_last_sync", 0);
                        UpdateLocal(crystalsTemp, int.Parse(lastLevelTemp), spaceships);
                        callback(true);
                    }
                    else
                    {
                        callback(false);
                    }
                });
        }
        else if (!PurchaseSpaceship.purchasing)
        {
            MainMenu.UpdateCrystals(MainMenu.crystals - amount, true);
            PlayerPrefs.SetString(item, "purchased");
            UpdateStoreItems.UpdateAllItems();
        }
    }
}