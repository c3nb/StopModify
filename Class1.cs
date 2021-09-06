using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using HarmonyLib;
using UnityEngine;
using ADOFAI;
using System.Reflection;
using UnityEngine.UI;

namespace StopModify
{
    public class Main
    {
        public static UnityModManager.ModEntry modEntry { get; private set; }
        public static Harmony harmony { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static Settings Settings { get; private set; }
        public static bool IsEnabled { get; private set; }
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.modEntry = modEntry;
            Logger = modEntry.Logger;
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = (entry, value) =>
            {
                IsEnabled = value;
                if(IsEnabled)
                {
                    harmony = new Harmony(entry.Info.Id);
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
                else
                {
                    harmony.UnpatchAll();
                    harmony = null;
                }
                return true;
            };
            modEntry.OnGUI = (entry) =>
            {
                Settings.Draw(entry);
                GUILayout.BeginVertical();
                GUILayout.Space(1);
                GUILayout.EndVertical();
                for(int i = 0; i < Settings.lets.Count; i++)
                {
                    if(GUILayout.Button(Settings.lets[i].ToString()))
                    {
                        Settings.lets.RemoveAt(i);
                    }
                }
            };
            modEntry.OnSaveGUI = entry => Settings.Save(entry);
            return true;
        }
    }
    public class Patches
    {
        [HarmonyPatch(typeof(scnEditor), "CopyEvent")]
        public class scnEditor_CopyEvent
        {
            public static bool Prefix(LevelEvent eventToCopy, int floor)
            {
                Main.Logger.Log("COPY");
                if (Main.Settings.StopCopyEvents)
                {
                    if(!Main.Settings.lets.Contains(eventToCopy.eventType))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(scnEditor), "CreateFloorWithCharOrAngle")]
        public class scnEditor_CreateFloorWithCharOrAngle
        {
            public static bool Prefix()
            {
                Main.Logger.Log("CREATE");
                return !Main.Settings.StopCreateFloor;
            }
        }
        [HarmonyPatch(typeof(scnEditor), "RemoveEventAtSelected")]
        public class scnEditor_RemoveEventAtSelected
        {
            public static bool Prefix(LevelEventType eventType)
            {
                Main.Logger.Log("REMOVE");
                if (Main.Settings.StopRemoveEvents)
                {
                    if (!Main.Settings.lets.Contains(eventType))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PropertyControl), "ValidateInput")]
        public class PropertyControl_ValidateInput
        {
            public static bool Prefix(PropertyControl __instance)
            {
                Main.Logger.Log("MODIFY");
                if (Main.Settings.StopModifyEvents)
                {
                    if (!Main.Settings.lets.Contains(__instance.propertiesPanel.levelEventType))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_Color), "Validate")]
        public class PropertyControl_Color_Validate
        {
            public static bool Prefix(PropertyControl_Color __instance)
            {
                Main.Logger.Log("MODIFY");
                if (Main.Settings.StopModifyEvents)
                {
                    if (!Main.Settings.lets.Contains(__instance.propertiesPanel.levelEventType))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PropertyControl_Text), "Validate")]
        public class PropertyControl_Text_Validate
        {
            public static bool Prefix(PropertyControl_Text __instance)
            {
                Main.Logger.Log("MODIFY");
                if (Main.Settings.StopModifyEvents)
                {
                    if (!Main.Settings.lets.Contains(__instance.propertiesPanel.levelEventType))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PropertyControl_Vector2), "Validate")]
        public class PropertyControl_Vector2_Validate
        {
            public static bool Prefix(PropertyControl_Vector2 __instance)
            {
                Main.Logger.Log("MODIFY");
                if (Main.Settings.StopModifyEvents)
                {
                    if (!Main.Settings.lets.Contains(__instance.propertiesPanel.levelEventType))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }
    }
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        public void OnChange()
        {
            if(!lets.Contains(let) && let != LevelEventType.None)
            {
                lets.Add(let);
            }
            Save(Main.modEntry);
        }
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
        [Draw("Stop CreateFloor")]
        public bool StopCreateFloor = true;
        [Draw("Stop RemoveEvents")]
        public bool StopRemoveEvents = true;
        [Draw("Stop CopyEvents")]
        public bool StopCopyEvents = true;
        [Draw("Stop ModifyEvents")]
        public bool StopModifyEvents = true;
        [Draw("Add Events")]
        public LevelEventType let = LevelEventType.None;
        public List<LevelEventType> lets = new List<LevelEventType>();
    }
}
