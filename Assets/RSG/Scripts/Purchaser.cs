using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace CompleteProject
{
    public class Purchaser : MonoBehaviour, IStoreListener
    {
        public GameObject Controls;
        public GameObject InitializationProblem;
        public GameObject ServerConnectionText;
        public GameObject RewardedButton;

        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;
        float initializationTimeout = 7.5f;

        void Start()
        {
            ServerConnectionText.SetActive(true);
            if (m_StoreController == null)
            {
                InitializePurchasing();
            }
            else
            {
                ServerConnectionText.SetActive(false);
                Controls.SetActive(true);
            }

            if (!RewardedController.rewardedAd.IsLoaded())
            {
                RewardedButton.SetActive(false);
            }
        }

        void Update()
        {
            if (m_StoreController == null)
            {
                if (initializationTimeout <= 0)
                {
                    LoadingError();
                }
                else
                {
                    initializationTimeout -= Time.deltaTime;
                }
            }
        }

        public void InitializePurchasing()
        {
            if (!Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("monetization_enabled").BooleanValue) return;
            if (IsInitialized())
            {
                return;
            }

            initializationTimeout = 7.5f;
            ServerConnectionText.SetActive(true);

            if (!RewardedController.rewardedAd.IsLoaded())
            {
                RewardedController.LoadAd();
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct("gravityflight_crystals_35", ProductType.Consumable);
            builder.AddProduct("gravityflight_crystals_100", ProductType.Consumable);
            builder.AddProduct("gravityflight_crystals_200", ProductType.Consumable);
            UnityPurchasing.Initialize(this, builder);
        }

        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void BuyConsumable(string kProductIDConsumable)
        {
            BuyProductID(kProductIDConsumable);
        }

        void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
                LoadingError();
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;

            ServerConnectionText.SetActive(false);
            InitializationProblem.SetActive(false);
            Controls.SetActive(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            LoadingError();
        }

        // Called if the initialization failed, the ads return error or the 4s timeout is up
        public void LoadingError()
        {
            InitializationProblem.SetActive(true);
            Controls.SetActive(false);
            ServerConnectionText.SetActive(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (String.Equals(args.purchasedProduct.definition.id, "gravityflight_crystals_35", StringComparison.Ordinal))
            {
                GainCrystals(35);
            }
            else if (String.Equals(args.purchasedProduct.definition.id, "gravityflight_crystals_100", StringComparison.Ordinal))
            {
                GainCrystals(100);
            }
            else if (String.Equals(args.purchasedProduct.definition.id, "gravityflight_crystals_200", StringComparison.Ordinal))
            {
                GainCrystals(200);
            }
            else
            {
                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            }

            return PurchaseProcessingResult.Complete;
        }

        public void GainCrystals(int amount)
        {
            MainMenu.UpdateCrystals(MainMenu.crystals + amount, true);
        }

        public void RewardedCrystals()
        {
            RewardedController.ShowAd();
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }
    }
}