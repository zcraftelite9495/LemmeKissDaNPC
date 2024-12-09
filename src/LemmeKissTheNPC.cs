using BepInEx;
using BepInEx.Configuration;
using Nessie.ATLYSS.EasySettings;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

[BepInPlugin("com.zcraftelite.lemmekissdanpc", "Lemme Kiss Da NPC", "1.2.0")]
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
        Logger.LogInfo("LemmeKissDaNPC is version 1.2.0.");

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

    private IEnumerator CheckAndModifyNpcColliders()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            CapsuleCollider[] colliders = Object.FindObjectsOfType<CapsuleCollider>();
            foreach (CapsuleCollider collider in colliders)
            {
                GameObject npc = collider.gameObject;
                if (npc.name.StartsWith("_npc_") && !processedNpcsWithoutCollider.Contains(npc.name))
                {
                    // Process the NPC's CapsuleCollider
                    if (collider.radius != 0f || collider.height != 0f || collider.enabled != false)
                    {
                        collider.radius = 0f;
                        collider.height = 0f;
                        collider.enabled = false;
                        Logger.LogInfo($"Modified CapsuleCollider for {npc.name}.");
                    }

                    // Check for a child named _jumpOffCollider
                    Transform jumpOffCollider = npc.transform.Find("_jumpOffCollider");
                    if (jumpOffCollider != null)
                    {
                        CapsuleCollider childCollider = jumpOffCollider.GetComponent<CapsuleCollider>();
                        if (childCollider != null && (childCollider.radius != 0f || childCollider.height != 0f || childCollider.enabled != false))
                        {
                            childCollider.radius = 0f;
                            childCollider.height = 0f;
                            childCollider.enabled = false;
                            Logger.LogInfo($"Modified CapsuleCollider for {npc.name}'s _jumpOffCollider.");
                        }
                    }

                    // Check for a child named _collider
                    Transform weirdCollider = npc.transform.Find("_collider");
                    if (weirdCollider != null)
                    {
                        CapsuleCollider weirdChildCollider = weirdCollider.GetComponent<CapsuleCollider>();
                        if (weirdChildCollider != null && (weirdChildCollider.radius != 0f || weirdChildCollider.height != 0f || weirdChildCollider.enabled != false))
                        {
                            weirdChildCollider.radius = 0f;
                            weirdChildCollider.height = 0f;
                            weirdChildCollider.enabled = false;
                            Logger.LogInfo($"Modified CapsuleCollider for {npc.name}'s _collider.");
                        }
                    }
                }
            }

            GameObject[] npcs = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject npc in npcs)
            {
                if (npc.name.StartsWith("_npc_") && !processedNpcsWithoutCollider.Contains(npc.name))
                {
                    if (npc.GetComponent<CapsuleCollider>() == null)
                    {
                        processedNpcsWithoutCollider.Add(npc.name);
                    }
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
