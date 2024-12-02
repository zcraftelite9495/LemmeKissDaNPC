using BepInEx;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[BepInPlugin("com.zcraftelite.lemmekissdanpc", "Lemme Kiss Da NPC", "1.0.0")]
public class LemmeKissDaNPC : BaseUnityPlugin
{
    // A set to track NPCs that have been processed without a CapsuleCollider
    private HashSet<string> processedNpcsWithoutCollider = new HashSet<string>();

    private void Start()
    {
        Logger.LogInfo("LemmeKissDaNPC Loaded!");
        StartCoroutine(CheckAndModifyNpcColliders());
    }

    private IEnumerator CheckAndModifyNpcColliders()
    {
        while (true)
        {
            // Wait for 1 second between checks (adjust as needed)
            yield return new WaitForSeconds(1f);

            // Find all GameObjects with names starting with "_npc_"
            GameObject[] npcs = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject npc in npcs)
            {
                if (npc.name.StartsWith("_npc_"))
                {
                    // Skip if the NPC has already been processed without a CapsuleCollider
                    if (processedNpcsWithoutCollider.Contains(npc.name))
                    {
                        continue;
                    }

                    // Get the CapsuleCollider component
                    CapsuleCollider collider = npc.GetComponent<CapsuleCollider>();
                    if (collider != null)
                    {
                        // Check if the values are already 0
                        if (collider.radius != 0f || collider.height != 0f)
                        {
                            collider.radius = 0f;
                            collider.height = 0f;
                            Logger.LogInfo($"Modified CapsuleCollider for {npc.name}.");
                        }
                    }
                    else
                    {
                        // Warn once and add to the processed list
                        if (!processedNpcsWithoutCollider.Contains(npc.name))
                        {
                            Logger.LogWarning($"{npc.name} does not have a CapsuleCollider component!");
                            processedNpcsWithoutCollider.Add(npc.name); // Add to the list to avoid further checks
                        }
                    }
                }
            }
        }
    }
}
