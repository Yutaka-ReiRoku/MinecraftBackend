using System;
using UnityEngine;

public static class GameEvents
{
    // --- ECONOMY & STATS EVENTS ---

    // Sự kiện khi tiền tệ thay đổi (Mua, Bán, Nhặt tiền)
    public static event Action OnCurrencyChanged;
    public static void TriggerCurrencyChanged() => OnCurrencyChanged?.Invoke();

    // Sự kiện khi chỉ số nhân vật thay đổi (Máu, Exp, Level)
    // Dùng để cập nhật thanh HP/Exp trên HUD
    public static event Action OnStatsChanged;
    public static void TriggerStatsChanged() => OnStatsChanged?.Invoke();

    // Sự kiện yêu cầu làm mới toàn bộ dữ liệu (Khi đăng nhập lại hoặc reset)
    public static event Action OnPlayerDataRefreshNeeded;
    public static void TriggerRefreshAll() => OnPlayerDataRefreshNeeded?.Invoke();

    // --- INVENTORY & ITEM EVENTS ---

    // Sự kiện khi Inventory thay đổi (Thêm/Bớt đồ) -> Reload UI Inventory
    public static event Action OnInventoryChanged;
    public static void TriggerInventoryChanged() => OnInventoryChanged?.Invoke();

    // Sự kiện yêu cầu trang bị vật phẩm (Dùng cho Drag & Drop)
    // Tham số: ItemID
    public static event Action<string> OnEquipRequest;
    public static void TriggerEquipRequest(string itemId) => OnEquipRequest?.Invoke(itemId);

    // --- SYSTEM EVENTS ---

    // Sự kiện khi Token hết hạn (401 Unauthorized) -> Logout
    public static event Action OnSessionExpired;
    public static void TriggerSessionExpired() => OnSessionExpired?.Invoke();

    // Sự kiện khi Server bảo trì hoặc mất kết nối
    public static event Action<string> OnNetworkError;
    public static void TriggerNetworkError(string message) => OnNetworkError?.Invoke(message);
}