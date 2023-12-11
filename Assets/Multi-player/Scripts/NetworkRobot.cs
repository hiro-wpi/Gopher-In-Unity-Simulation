using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using Unity.XR.CoreUtils;

/// <summary>
///    Defines a network robot
///    Adjust joints if not owner
/// </summary>
public class NetworkRobot : NetworkBehaviour
{
    // Arictulation scripts under which to disable
    public GameObject Plugins;
    // Articulation scripts whom should not be disabled
    public GameObject[] PluginsToKeep;

    // Robot root
    public GameObject RobotRoot;

    public override void OnNetworkSpawn()
    {
        // Disable game object if not the owner and
        // if not in the ToKeep list
        if (!IsOwner)
        {
            foreach (Transform plugin in Plugins.transform)
            {
                // If the child is not in the ToKeep list
                plugin.gameObject.SetActive(false);
            }
            foreach (GameObject plugin in PluginsToKeep)
            {
                plugin.SetActive(true);
            }
        }
    }
}
