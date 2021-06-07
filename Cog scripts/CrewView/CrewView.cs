/* Frank Calabrese
 * 3/9/21
 * CrewView.cs
 * handles adding/removing crew visual changes in crewView. Also assists CrewViewAutoPopulator.cs
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrewView : MonoBehaviour
{
    [SerializeField] Sprite vacantSprite;
    [SerializeField] Sprite occupiedSprite;

    List<GameObject> crewSlots = new List<GameObject>();
    [SerializeField] GameObject crewView;
    private bool finishedPopulating = false;
    [SerializeField] GameObject redOverlay;
    [SerializeField] float vacantOpacity;

    private RoomStats roomStats;
    
    private void Start()
    {
        roomStats = GetComponent<RoomStats>();
    }

    private void Update()
    {
        crewView.SetActive(CrewViewManager.Instance.GetCrewViewStatus());

        if(finishedPopulating)
        {
            //Color a sprite opaque for every crew member in the room
            for (int i = 0; i < roomStats.currentCrew; i++)
            {
                Color tempcolor = crewSlots[i].GetComponent<Image>().color;
                tempcolor.a = 1f;
                crewSlots[i].GetComponent<Image>().color = tempcolor;
            }

            //Color the rest of the sprites transparent
            for (int i = roomStats.currentCrew; i < roomStats.maxCrew; i++)
            {
                Color tempcolor = crewSlots[i].GetComponent<Image>().color;
                tempcolor.a = vacantOpacity;
                crewSlots[i].GetComponent<Image>().color = tempcolor;
            }

            UpdateCrewViewRotation();
        }
    }

    //Rotates crew icons upright relative to how their room is rotated
    private void UpdateCrewViewRotation()
    {
        for (int i = 0; i < roomStats.maxCrew; i++)
        {
            crewSlots[i].transform.rotation = Quaternion.identity;
        }
    }

    //called by AutoPopulator to populate list with icons
    public void ActivateCrewSlot(GameObject crewViewSlotPrefab)
    {
        crewSlots.Add(crewViewSlotPrefab);
    }

    public void FinishPopulatingCrewSlots()
    {
        finishedPopulating = true;
    }


        
}
