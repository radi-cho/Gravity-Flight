using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateStoreItems : MonoBehaviour
{
    static PurchaseSpaceship[] staticItems;
    public PurchaseSpaceship[] items;

    void Awake()
    {
        staticItems = items;
    }

    public static void UpdateAllItems()
    {
        foreach (PurchaseSpaceship item in staticItems)
        {
            item.UpdateShop();
        }
    }
}
