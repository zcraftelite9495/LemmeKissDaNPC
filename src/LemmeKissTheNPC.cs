using BepInEx;
using BepInEx.Configuration;
using Nessie.ATLYSS.EasySettings;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

[BepInPlugin("com.zcraftelite.lemmekissdanpc", "Lemme Kiss Da NPC", "1.3.0")]
public class LemmeKissDaNPC : BaseUnityPlugin
{
    private HashSet<string> processedNpcsWithoutCollider = new HashSet<string>();
    private bool skritProcessed = false;

    private static ConfigEntry<bool> ConfigEnableEasterEgg;

    // --------------------------------------------
    // Plugin Startup
    // --------------------------------------------

    private void Awake()
    {
        InitConfig();

        Settings.OnInitialized.AddListener(AddSettings);
        Settings.OnApplySettings.AddListener(() => { Config.Save(); });
    }

    private void Start()
    {
        Logger.LogInfo("LemmeKissDaNPC Loaded!");
        Logger.LogInfo("LemmeKissDaNPC is version 1.3.0.");

        FuckSkritOver();
        StartCoroutine(CheckAndModifyNpcColliders());
    }

    // --------------------------------------------
    // EasySettings Integration
    // --------------------------------------------

    // Initialize EasySettings configuration
    private void InitConfig()
    {
        var easterEggDefinition = new ConfigDefinition("Easter Egg", "EnableEasterEgg");
        var easterEggDescription = new ConfigDescription("Enable or disable the Skrit Easter Egg.");
        ConfigEnableEasterEgg = Config.Bind(easterEggDefinition, true, easterEggDescription);
    }

    // Add EasySettings UI elements
    private void AddSettings()
    {
        SettingsTab tab = Settings.ModTab;

        tab.AddHeader("Lemme Kiss Da NPC");

        tab.AddToggle("Enable Easter Egg", ConfigEnableEasterEgg);
    }

    // Check if Easter egg is enabled in the configuration
    private bool IsEasterEggEnabled()
    {
        return ConfigEnableEasterEgg.Value;
    }

    // ---------------------------------------------
    // Remove NPC Collision
    // ---------------------------------------------

    private Dictionary<string, (bool hasMainColliderChecked, bool hasJumpOffColliderChecked, bool hasWeirdColliderChecked)> npcColliderStatus = new Dictionary<string, (bool, bool, bool)>();

    private IEnumerator CheckAndModifyNpcColliders()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            GameObject[] npcs = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject npc in npcs)
            {
                if (npc.name.StartsWith("_npc_"))
                {
                    // Initialize status if the NPC is new
                    if (!npcColliderStatus.ContainsKey(npc.name))
                    {
                        npcColliderStatus[npc.name] = (false, false, false);
                    }

                    var status = npcColliderStatus[npc.name];

                    // Check if the main CapsuleCollider exists and hasn't been checked
                    if (!status.hasMainColliderChecked)
                    {
                        CapsuleCollider mainCollider = npc.GetComponent<CapsuleCollider>();
                        if (mainCollider == null)
                        {
                            Logger.LogInfo($"No CapsuleCollider found for {npc.name}.");
                            status.hasMainColliderChecked = true; // Mark as checked, no need to look again
                        }
                    }

                    // Check if the _jumpOffCollider exists and hasn't been checked
                    if (!status.hasJumpOffColliderChecked)
                    {
                        Transform jumpOffCollider = npc.transform.Find("_jumpOffCollider");
                        if (jumpOffCollider == null || jumpOffCollider.GetComponent<CapsuleCollider>() == null)
                        {
                            Logger.LogInfo($"No _jumpOffCollider found for {npc.name}.");
                            status.hasJumpOffColliderChecked = true; // Mark as checked, no need to look again
                        }
                    }

                    // Check if the _collider exists and hasn't been checked
                    if (!status.hasWeirdColliderChecked)
                    {
                        Transform weirdCollider = npc.transform.Find("_collider");
                        if (weirdCollider == null || weirdCollider.GetComponent<CapsuleCollider>() == null)
                        {
                            Logger.LogInfo($"No _collider found for {npc.name}.");
                            status.hasWeirdColliderChecked = true; // Mark as checked, no need to look again
                        }
                    }

                    // Update the dictionary with the latest status
                    npcColliderStatus[npc.name] = status;
                }
            }
        }
    }

    // ---------------------------------------------
    // Easter Egg Stuffs
    // ---------------------------------------------

    private void FuckSkritOver()
    {
        StartCoroutine(CheckAndModifySkrit());
    }

    private IEnumerator CheckAndModifySkrit()
    {
        while (!skritProcessed)
        {
            yield return new WaitForSeconds(1f);

            if (!IsEasterEggEnabled())
            {
                Logger.LogInfo("Easter egg is disabled in the config.");
                skritProcessed = true;
                yield break;
            }

            GameObject npcSkrit = GameObject.Find("_npc_Skrit");
            if (npcSkrit != null)
            {
                NetNPC netNpc = npcSkrit.GetComponent<NetNPC>();
                if (netNpc != null)
                {
                    float randomChance = Random.Range(0f, 1f);
                    if (randomChance < 0.1f)
                    {
                        string[] possibleNames = new string[]
                        {
                            "Skrit <color=red>(Smelly Gambler)</color>",
                            "Skrit <color=black>(Submissive Bitch)</color>",
                            "Skrit <color=green>(Stinker Dinker)</color>",
                            "Skrit <color=purple>(Toxic Stench)</color>",
                            "Skirt <color=yellow>(Gambler)</color>"
                        };
                        string randomName = possibleNames[Random.Range(0, possibleNames.Length)];

                        FieldInfo npcNameField = typeof(NetNPC).GetField("_npcName", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (npcNameField != null)
                        {
                            npcNameField.SetValue(netNpc, randomName);
                            Logger.LogInfo("YOU GOT THE EASTER EGG :O");
                            Logger.LogInfo($"Changed poor Skrit's name to: {randomName}");
                        }
                        else
                        {
                            Logger.LogWarning("Failed to access _npcName field via reflection.");
                        }
                    }
                }
                else
                {
                    Logger.LogWarning("_npc_Skrit does not have a NetNPC component!");
                }

                skritProcessed = true;
            }
        }
    }
}
