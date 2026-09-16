using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace SwapWeapons
{
    public sealed partial class BepInExPlugin : BaseUnityPlugin
    {

        private static ConfigEntry<bool> _enableDebugLogging;
        private static ConfigEntry<KeyboardShortcut> _swapWeaponsKey;
        private static ConfigEntry<bool> _autoEquipAfterSwimming;

        private static readonly MethodInfo _toggleEquippedMethod = typeof(Humanoid).GetMethod("ToggleEquipped", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _showHandItemsMethod = typeof(Humanoid).GetMethod("ShowHandItems", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _takeInputMethod = typeof(Player).GetMethod("TakeInput", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _hiddenLeftItemField = typeof(Humanoid).GetField("m_hiddenLeftItem", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _hiddenRightItemField = typeof(Humanoid).GetField("m_hiddenRightItem", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool ToggleEquipped(Humanoid humanoid, ItemDrop.ItemData item)
            => (bool)_toggleEquippedMethod.Invoke(humanoid, new object[] { item });

        private static void ShowHandItems(Humanoid humanoid, bool onlyRightHand = false, bool animation = true)
            => _showHandItemsMethod.Invoke(humanoid, new object[] { onlyRightHand, animation });

        private static bool TakeInput(Player player)
            => (bool)_takeInputMethod.Invoke(player, null);

        private static ItemDrop.ItemData GetHiddenLeftItem(Humanoid humanoid)
            => (ItemDrop.ItemData)_hiddenLeftItemField.GetValue(humanoid);

        private static ItemDrop.ItemData GetHiddenRightItem(Humanoid humanoid)
            => (ItemDrop.ItemData)_hiddenRightItemField.GetValue(humanoid);

        private static ItemDrop.ItemData _lastRightItem;
        private static ItemDrop.ItemData _lastLeftItem;
        private static ItemDrop.ItemData _cachedRightItem;
        private static ItemDrop.ItemData _cachedLeftItem;

        private static bool _cacheLastItems = true;
        private static bool _hiddenItemsForSwimming;

        private static ManualLogSource _logger;
        private static void DebugLog(string text)
        {
            if (_enableDebugLogging?.Value == true)
            {
                _logger.LogMessage(text);
            }
        }

        private void Awake()
        {
            _logger = Logger;

            string section = "General";

            _enableDebugLogging = Config.Bind(section, "EnableDebugLogging", false, "Enables detailed debug logging");
            _autoEquipAfterSwimming = Config.Bind(section, "AutoEquipOnLeavingWater", false, "Will automatically restore weapons that were unequipped on entering water");
            _swapWeaponsKey = Config.Bind(section, "SwapWeaponsKey", new KeyboardShortcut(KeyCode.Q), "Swaps between the current and previously equipped weapons");

            Harmony.CreateAndPatchAll(typeof(BepInExPlugin));
        }

        private void Update()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }
            if (TakeInput(player) && Input.GetKeyDown(_swapWeaponsKey.Value.MainKey))
            {
                SwapToLastEquippedItems();
            }
            if (_autoEquipAfterSwimming.Value && _hiddenItemsForSwimming && CanEquipItems())
            {
                DebugLog("Restoring hand items after swimming.");
                ShowHandItems(player);
                _hiddenItemsForSwimming = false;
            }
        }

        private static bool CanEquipItems()
        {
            Player player = Player.m_localPlayer;
            return player != null && (!player.IsSwimming() || player.IsOnGround());
        }

        private static bool HasAnyHiddenItem()
        {
            Player player = Player.m_localPlayer;
            return player != null && (GetHiddenLeftItem(player) != null || GetHiddenRightItem(player) != null);
        }

        private static bool IsRelevantItem(ItemDrop.ItemData item)
        {
            ItemDrop.ItemData.ItemType itemType = item.m_shared.m_itemType;
            return itemType == ItemDrop.ItemData.ItemType.Tool ||
                   itemType == ItemDrop.ItemData.ItemType.Torch ||
                   itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon ||
                   itemType == ItemDrop.ItemData.ItemType.Shield ||
                   itemType == ItemDrop.ItemData.ItemType.Bow ||
                   itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon;
        }

        private static bool IsSameItem(ItemDrop.ItemData firstItem, ItemDrop.ItemData secondItem)
        {
            return firstItem?.m_shared != null && secondItem?.m_shared != null &&
                   firstItem.m_shared.m_name == secondItem.m_shared.m_name;
        }

        private static void SetLastRightItem(ItemDrop.ItemData newRightItem)
        {
            _lastRightItem = newRightItem;
            if (_lastRightItem != null)
            {
                DebugLog($"SetLastRightItem: {_lastRightItem.m_shared.m_name} ({_lastRightItem.m_shared.m_itemType})");
            }
            else
            {
                DebugLog("SetLastRightItem: empty");
            }

            ItemDrop.ItemData.ItemType? rightItemType = _lastRightItem?.m_shared?.m_itemType;
            ItemDrop.ItemData.ItemType? leftItemType = _lastLeftItem?.m_shared?.m_itemType;
            if (rightItemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                rightItemType == ItemDrop.ItemData.ItemType.Tool ||
                leftItemType == ItemDrop.ItemData.ItemType.Bow)
            {
                _lastLeftItem = null;
                DebugLog("SetLastRightItem: cleared last left item");
            }
        }

        private static void SetLastLeftItem(ItemDrop.ItemData newLeftItem)
        {

            _lastLeftItem = newLeftItem;
            if (_lastLeftItem != null)
            {
                DebugLog($"SetLastLeftItem: {_lastLeftItem.m_shared.m_name} ({_lastLeftItem.m_shared.m_itemType})");
            }
            else
            {
                DebugLog("SetLastLeftItem: empty");
            }

            ItemDrop.ItemData.ItemType? rightItemType = _lastRightItem?.m_shared?.m_itemType;
            ItemDrop.ItemData.ItemType? leftItemType = _lastLeftItem?.m_shared?.m_itemType;
            if (rightItemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                rightItemType == ItemDrop.ItemData.ItemType.Tool ||
                leftItemType == ItemDrop.ItemData.ItemType.Bow)
            {
                _lastRightItem = null;
                DebugLog("SetLastLeftItem: cleared last right item");
            }
        }

        private static void SwapToLastEquippedItems()
        {
            Player player = Player.m_localPlayer;
            if (player == null || player.InAttack() || player.IsRunning())
            {
                return;
            }

            // If player used "R" to hide weapons, then make this mod just work like hitting "R" again and unholster them
            if (HasAnyHiddenItem())
            {
                if (CanEquipItems())
                {
                    DebugLog("Weapons are hidden. Restoring them instead of swapping.");
                    ShowHandItems(player);
                }

                return;
            }

            DebugLog("");
            DebugLog("--- [Swap Weapons] START ---");

            _cacheLastItems = false;

            try
            {

                ItemDrop.ItemData previousRightItem = null;
                ItemDrop.ItemData previousLeftItem = null;
                CacheEquippedItems(player, ref previousRightItem, ref previousLeftItem);

                bool rightItemEquipped = false;
                bool lastRightItemIsEquipped = IsSameItem(_lastRightItem, previousRightItem);
                bool lastLeftItemIsEquipped = IsSameItem(_lastLeftItem, previousLeftItem);

                if (lastRightItemIsEquipped)
                {
                    _lastRightItem = null;
                    DebugLog("Last right item is already equipped. Clearing it.");
                }

                if (_lastRightItem != null)
                {
                    if (_lastLeftItem?.m_shared?.m_itemType == ItemDrop.ItemData.ItemType.Torch)
                    {
                        _lastLeftItem = null;
                    }

                    DebugLog($"Trying to equip last right item: {_lastRightItem.m_shared.m_name} ({_lastRightItem.m_shared.m_itemType})");

                    rightItemEquipped = ToggleEquipped(player, _lastRightItem);
                    if (rightItemEquipped)
                    {
                        DebugLog("Right item equipped successfully.");

                        _lastRightItem = previousRightItem;
                        if (_lastRightItem != null)
                        {
                            DebugLog($"Last right item: {_lastRightItem.m_shared.m_name} ({_lastRightItem.m_shared.m_itemType})");
                        }
                        else
                        {
                            DebugLog("Last right item: empty");
                        }
                    }
                }

                if (lastLeftItemIsEquipped)
                {
                    _lastLeftItem = null;
                    DebugLog("Last left item is already equipped. Clearing it.");
                }

                if (_lastLeftItem != null)
                {
                    DebugLog($"Trying to equip last left item: {_lastLeftItem.m_shared.m_name} ({_lastLeftItem.m_shared.m_itemType})");

                    bool leftItemEquipped = rightItemEquipped
                        ? player.EquipItem(_lastLeftItem)
                        : ToggleEquipped(player, _lastLeftItem);

                    if (leftItemEquipped)
                    {
                        DebugLog("Left item equipped successfully.");

                        _lastLeftItem = previousLeftItem;
                        if (_lastLeftItem != null)
                        {
                            DebugLog($"Last left item: {_lastLeftItem.m_shared.m_name} ({_lastLeftItem.m_shared.m_itemType})");
                        }
                        else
                        {
                            DebugLog("Last left item: empty");
                        }
                    }
                }
                else
                {
                    //bool cachePreviousLeftItem = previousLeftItem != null &&
                    //                             _lastRightItem?.m_shared?.m_itemType == ItemDrop.ItemData.ItemType.Tool;
                    //if (cachePreviousLeftItem)
                    {
                        _lastLeftItem = previousLeftItem;
                    }
                }

            }
            finally
            {
                _cacheLastItems = true;
            }

            DebugLog("--- [Swap Weapons] END ---");
            DebugLog("");
        }


        private static void CacheEquippedItems(Humanoid humanoid, ref ItemDrop.ItemData rightItem, ref ItemDrop.ItemData leftItem)
        {
            if (humanoid == null || humanoid != Player.m_localPlayer)
            {
                return;
            }

            ItemDrop.ItemData equippedRightItem = humanoid.RightItem;
            if (equippedRightItem != null)
            {
                rightItem = equippedRightItem;

                if (equippedRightItem.m_shared != null)
                {
                    DebugLog($"Currently equipped right item: {equippedRightItem.m_shared.m_name} ({equippedRightItem.m_shared.m_itemType})");
                }
            }

            ItemDrop.ItemData equippedLeftItem = humanoid.LeftItem;
            if (equippedLeftItem != null)
            {
                leftItem = equippedLeftItem;

                if (equippedLeftItem.m_shared != null)
                {
                    DebugLog($"Currently equipped left item: {equippedLeftItem.m_shared.m_name} ({equippedLeftItem.m_shared.m_itemType})");
                }
            }
        }

        //-------------------------------------
        // Patches
        //-------------------------------------

        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        [HarmonyPrefix]
        private static void UnequipItem_Prefix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
        {
            if (__instance != Player.m_localPlayer || !_cacheLastItems || item == null || item.m_shared == null)
            {
                return;
            }

            DebugLog($"UnequipItem_Prefix: {item.m_shared.m_name} ({item.m_shared.m_itemType})");
            CacheEquippedItems(__instance, ref _cachedRightItem, ref _cachedLeftItem);
        }

        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        [HarmonyPostfix]
        private static void UnequipItem_Postfix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
        {
            if (__instance != Player.m_localPlayer || !_cacheLastItems || item == null || item.m_shared == null)
            {
                return;
            }

            DebugLog($"UnequipItem_Postfix: {item.m_shared.m_name} ({item.m_shared.m_itemType})");
            if (_cachedRightItem != __instance.RightItem)
            {
                SetLastRightItem(_cachedRightItem);
            }
            if (_cachedLeftItem != __instance.LeftItem)
            {
                SetLastLeftItem(_cachedLeftItem);
            }

            _cachedRightItem = null;
            _cachedLeftItem = null;
        }

        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        [HarmonyPrefix]
        private static void EquipItem_Prefix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
        {
            if (__instance != Player.m_localPlayer || item == null || item.m_shared == null)
            {
                return;
            }

            DebugLog($"EquipItem_Prefix: {item.m_shared.m_name} ({item.m_shared.m_itemType})");

            if (__instance.IsItemEquiped(item))
            {
                DebugLog("EquipItem_Prefix: Item is already equipped. Ignoring.");
                return;
            }

            if (!IsRelevantItem(item))
            {
                return;
            }

            ItemDrop.ItemData hiddenLeftItem = GetHiddenLeftItem(__instance);
            ItemDrop.ItemData hiddenRightItem = GetHiddenRightItem(__instance);

            if (hiddenRightItem != null && hiddenRightItem != item)
            {
                SetLastRightItem(hiddenRightItem);
            }

            if (hiddenLeftItem != null && hiddenLeftItem != item)
            {
                SetLastLeftItem(hiddenLeftItem);
            }
        }

        [HarmonyPatch(typeof(Humanoid), "HideHandItems")]
        [HarmonyPrefix]
        private static void HideHandItems_Prefix(Humanoid __instance)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }
            if (__instance.IsSwimming() && !__instance.IsOnGround() && (__instance.RightItem != null || __instance.LeftItem != null))
            {
                _hiddenItemsForSwimming = true;
                DebugLog("Hand items hidden while swimming. Marked for automatic restore.");
            }
            _cacheLastItems = false;
        }

        [HarmonyPatch(typeof(Humanoid), "HideHandItems")]
        [HarmonyFinalizer]
        private static void HideHandItems_Finalizer(Humanoid __instance)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }
            _cacheLastItems = true;
        }

    }
}
