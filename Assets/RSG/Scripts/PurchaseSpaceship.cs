using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseSpaceship : MonoBehaviour
{
    public static string defaultSprite = "Starman/rocket";
    public static bool purchasing = false;
    public TextMeshProUGUI buttonText;
    static TextMeshProUGUI selectedButtonText;
    public static GameObject CrystalStore;
    public static GameObject SpaceshipStore;
    public static GameObject Message;
    static Button selectedButton;
    public Button button;
    public int price;
    public string name;
    bool isPurchased;

    void Start()
    {
        UpdateShop();
        if (name == defaultSprite)
        {
            PlayerPrefs.SetString(name, "purchased");
        }
    }

    public void UpdateShop()
    {
        if (PlayerPrefs.GetString("spaceship", defaultSprite) == name)
        {
            isPurchased = true;
            button.interactable = false;
            buttonText.SetText("Selected");

            selectedButton = button;
            selectedButtonText = buttonText;
        }
        else if (PlayerPrefs.HasKey(name))
        {
            isPurchased = true;
            buttonText.SetText("Select");
        }
        else
        {
            isPurchased = false;
            buttonText.SetText("Purchase");
        }
    }

    public void ButtonClicked()
    {
        if (isPurchased)
        {
            SelectSpaceship();
        }
        else
        {
            BuySpaceship();
        }

        UpdateShop();
    }

    public void BuySpaceship()
    {
        if (price <= MainMenu.crystals)
        {
            Sync.HandleSpaceshipPurchase(name, price, (success) =>
            {
                if (success)
                {
                    SelectSpaceship();
                    UpdateShop();

                    Firebase.Analytics.FirebaseAnalytics.LogEvent(
                        Firebase.Analytics.FirebaseAnalytics.EventSpendVirtualCurrency,
                            new Firebase.Analytics.Parameter[] {
                                new Firebase.Analytics.Parameter (
                                    Firebase.Analytics.FirebaseAnalytics.ParameterItemName, name),
                                new Firebase.Analytics.Parameter (
                                    Firebase.Analytics.FirebaseAnalytics.ParameterValue, price),
                                new Firebase.Analytics.Parameter (
                                    Firebase.Analytics.FirebaseAnalytics.ParameterVirtualCurrencyName, "crystal"),
                        }
                    );
                }
                else
                {
                    Message.gameObject.SetActive(true);
                    StartCoroutine(HideAfter(3, Message.gameObject));
                }
            });
        }
        else
        {
            SpaceshipStore.gameObject.SetActive(false);
            CrystalStore.gameObject.SetActive(true);
        }
    }


    public static IEnumerator HideAfter(int seconds, GameObject obj)
    {
        yield return new WaitForSeconds(seconds);
        obj.SetActive(false);
    }

    public void SelectSpaceship()
    {
        selectedButton.interactable = true;
        selectedButtonText.SetText("Select");

        PlayerPrefs.SetString("spaceship", name);
        selectedButton = button;
        selectedButtonText = buttonText;
    }
}