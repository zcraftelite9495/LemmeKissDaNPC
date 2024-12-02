using BepInEx;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[BepInPlugin("com.zcraftelite.lemmekissdanpc", "Lemme Kiss Da NPC", "1.0.1")]
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

            // Find all CapsuleCollider components in the scene
            CapsuleCollider[] colliders = Object.FindObjectsOfType<CapsuleCollider>();
            foreach (CapsuleCollider collider in colliders)
            {
                // Skip if this NPC has already been processed without a CapsuleCollider
                GameObject npc = collider.gameObject;
                if (npc.name.StartsWith("_npc_") && !processedNpcsWithoutCollider.Contains(npc.name))
                {
                    // Check if the CapsuleCollider radius and height are not already 0
                    if (collider.radius != 0f || collider.height != 0f)
                    {
                        collider.radius = 0f;
                        collider.height = 0f;
                        Logger.LogInfo($"Modified CapsuleCollider for {npc.name}.");
                    }
                }
            }

            // Now check for NPCs without a CapsuleCollider (only once for each)
            GameObject[] npcs = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject npc in npcs)
            {
                if (npc.name.StartsWith("_npc_") && !processedNpcsWithoutCollider.Contains(npc.name))
                {
                    // Check if the NPC has a CapsuleCollider
                    if (npc.GetComponent<CapsuleCollider>() == null)
                    {
                        Logger.LogWarning($"{npc.name} does not have a CapsuleCollider component!");
                        processedNpcsWithoutCollider.Add(npc.name); // Add to the list to avoid further checks
                    }
                }
            }
        }
    }
}
