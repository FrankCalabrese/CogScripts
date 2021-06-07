/* Frank Calabrese
 * 3/9/21
 * CrewViewManager.cs
 * allows player to toggle crewView on or off.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewViewManager : Singleton<CrewViewManager>
{
    private bool crewViewEnabled = false;
    private Tick ticker;

    private void Start()
    {
        ticker = FindObjectOfType<Tick>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (crewViewEnabled == false && (!EventSystem.instance.eventActive || GameManager.instance.currentGameState == InGameStates.ShipBuilding)) EnableCrewView();
            else DisableCrewView();

            Tutorial.Instance.ConditionalContinueToggleCrewView();
        }
    }

    public void EnableCrewView()
    {
        crewViewEnabled = true;
    }
    public void DisableCrewView()
    {
        crewViewEnabled = false;
    }
    public bool GetCrewViewStatus()
    {
        return crewViewEnabled;
    }

}
