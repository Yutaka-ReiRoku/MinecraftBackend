using System;
using UnityEngine;

public static class GameEvents
{
    

    
    public static event Action OnCurrencyChanged;
    public static void TriggerCurrencyChanged() => OnCurrencyChanged?.Invoke();

    
    
    public static event Action OnStatsChanged;
    public static void TriggerStatsChanged() => OnStatsChanged?.Invoke();

    
    public static event Action OnPlayerDataRefreshNeeded;
    public static void TriggerRefreshAll() => OnPlayerDataRefreshNeeded?.Invoke();

    

    
    public static event Action OnInventoryChanged;
    public static void TriggerInventoryChanged() => OnInventoryChanged?.Invoke();

    
    
    public static event Action<string> OnEquipRequest;
    public static void TriggerEquipRequest(string itemId) => OnEquipRequest?.Invoke(itemId);

    

    
    public static event Action OnSessionExpired;
    public static void TriggerSessionExpired() => OnSessionExpired?.Invoke();

    
    public static event Action<string> OnNetworkError;
    public static void TriggerNetworkError(string message) => OnNetworkError?.Invoke(message);
}