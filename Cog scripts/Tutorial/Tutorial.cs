/* Frank Calabres
 * 3/6/21
 * Tutorial.cs
 * Singleton class responsible for holding and tracking all tutorials.
 * Will display specified tutorial when setCurrentTutorial(tutorial ID) is called
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

[System.Serializable]
public class TutorialNode
{
    public string tutorialName;
    public TutorialMessage[] tutorialMessages;
    public bool tutorialFinished;
}

[System.Serializable]
public class TutorialMessage
{
    [TextArea] public string message;

    [Header("Tutorial Effects")]
    [Foldout("Ghost Cursor Effects")] public bool ghostCursorHydroponics;
    [Foldout("Ghost Cursor Effects")] public bool ghostCursorChargingTerminal;
    [Foldout("Ghost Cursor Effects")] public bool ghostCursorArmorPlating;
    [Foldout("Ghost Cursor Effects")] public bool ghostCursorStatBar;
    [Foldout("Ghost Cursor Effects")] public bool ghostCursorCrewBunks;
    [Foldout("Other effects")] public bool selectRoom;

    [Header("Tutorial Continue Conditions")]
    public bool continueOnPlaceRoom;
    public bool continueOnDeleteRoom;
    public bool continueOnAddCrew;
    public bool continueOnRoomRotate;
    public bool continueOnToggleCV;

    //[HideInInspector] public bool dynamicActionCompleted = false;
}

public class Tutorial : Singleton<Tutorial>
{
    [SerializeField, Tooltip("Ability to disable all tutorial elements")] private bool disableTutorial;
    [SerializeField] TutorialNode[] tutorials = new TutorialNode[10];
    [SerializeField] TextMeshProUGUI tutorialTextbox;
    [SerializeField] TextMeshProUGUI tutorialTitleTextbox;
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] ButtonTwoBehaviour backButton;
    private Tick ticker;

    [SerializeField] GameObject highlightPanel;
    [SerializeField] GameObject ghostCursor;


    [SerializeField] float timeStartedLerping;
    [SerializeField] float lerpTime;
    private bool lerping = false;
    private float percentageComplete;
    private Vector3 lerpStart;
    private Vector3 lerpEnd;
    private RoomStats[] rooms;

    [SerializeField] GameObject vecInsideShip;
    [SerializeField] GameObject vecShopPanel;
    [SerializeField] GameObject vecStatsLeft;
    [SerializeField] GameObject vecStatsRight;
    [SerializeField] GameObject vecRoomDetails;

    private TutorialNode currentTutorial;
    private int index;
    private bool tutorialPrerequisitesComplete = false;

    private void Start()
    {
        if (SavingLoadingManager.instance.GetHasSave())
        {
            LoadTutorialStatus();
        }
        else
        {
            foreach (var tutorial in tutorials)
            {
                tutorial.tutorialFinished = false;
            }

            currentTutorial = tutorials[1];

            SaveTutorialStatus();
        }

        rooms = FindObjectsOfType<RoomStats>();
        ticker = FindObjectOfType<Tick>();
    }

    private void Update()
    {
        if (tutorialPanel.activeSelf && index == 0) backButton.SetButtonInteractable(false);
        else backButton.SetButtonInteractable(true);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ContinueButton();
        }
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            ContinueButton(true);
        }

        if(tutorialPanel.activeSelf == true && GameManager.instance.currentGameState == InGameStates.Events && !ticker.IsTickStopped())
        {
            Debug.LogWarning("stopping tick");
            ticker.StopTickUpdate();
        }

        #region EFFECTS
        if (tutorialPanel.activeSelf == true)
        {
            if (GameManager.instance.currentGameState == InGameStates.ShipBuilding)
            {
                if (currentTutorial.tutorialMessages[index].ghostCursorHydroponics) GhostCursorHydroponics();
                else if (currentTutorial.tutorialMessages[index].ghostCursorChargingTerminal) GhostCursorChargingTerminal();
                else if (currentTutorial.tutorialMessages[index].ghostCursorArmorPlating) GhostCursorArmorPlating();
                else if (currentTutorial.tutorialMessages[index].ghostCursorStatBar) GhostCursorStatBar();
                else if (currentTutorial.tutorialMessages[index].ghostCursorCrewBunks) GhostCursorCrewBunks();

                //Crew Management Effect
                else if (currentTutorial.tutorialMessages[index].selectRoom) EffectSelectRoom();
            }
        }
        #endregion

        if (lerping)
        {
            ghostCursor.transform.position = LerpGhostCursor(lerpStart, lerpEnd, timeStartedLerping, lerpTime);

            if (ghostCursor.transform.position == lerpEnd)
            {
                lerping = false;
                BeginLerping(lerpStart, lerpEnd);
            }
        }

    }


    //call this to display a tutorial. Tutorial IDs can be found in the inspector
    public void SetCurrentTutorial(int tutorialID, bool forcedTutorial)
    {
        if(disableTutorial) return;

        //if you're already in this same tutorial, stop. 
        if (tutorials[tutorialID] == currentTutorial && tutorialPanel.activeSelf) return;

        //if the game tries to force a tutorial the player has already seen, stop.
        if (tutorials[tutorialID].tutorialFinished == true && forcedTutorial == true) return;

        //if you're already in a tutorial, close it and continue
        if (tutorialPanel.activeSelf == true)
        {
            CloseCurrentTutorial(true);
        }

        //reset effects
        tutorialPrerequisitesComplete = false;

        currentTutorial = tutorials[tutorialID];

        if (tutorialPanel.activeSelf == false)
        {
            tutorialPanel.SetActive(true);
            tutorialTextbox.text = currentTutorial.tutorialMessages[0].message;
            tutorialTitleTextbox.text = currentTutorial.tutorialName;
            index = 0;
        }
    }

    public void CloseCurrentTutorial(bool finished = true)
    {
        if(disableTutorial) return;

        if (tutorialPanel.activeSelf == true)
        {
            if (GameManager.instance.currentGameState == InGameStates.Events && ticker.IsTickStopped())
            {
                Debug.LogWarning("resuming tick");
                if(!EventSystem.instance.eventActive) ticker.StartTickUpdate();
            }

            highlightPanel.SetActive(false);
            lerping = false;
            ghostCursor.SetActive(false);
            tutorialPanel.SetActive(false);
            index = 0;

            if(!currentTutorial.tutorialFinished) currentTutorial.tutorialFinished = finished;
            SaveTutorialStatus();
        }
    }

    //X and Y of center of box, with (0,0) being screen center. Width and height of box
    private void HighlightScreenLocation(int xPos, int yPos, int width, int height)
    {
        highlightPanel.SetActive(true);
        RectTransform loc = highlightPanel.GetComponent<RectTransform>();
        loc.sizeDelta = new Vector2(width, height);
        loc.localPosition = new Vector3(xPos, yPos, 1);
    }
    public void UnHighlightScreenLocation()
    {
        if(disableTutorial) return;

        highlightPanel.SetActive(false);
    }

    public void ContinueButton(bool back = false)
    {
        if(disableTutorial) return;

        if (tutorialPanel.activeSelf == true)
        {
            tutorialPrerequisitesComplete = false;

            //forward
            if (index < currentTutorial.tutorialMessages.Length - 1 && back == false)
            {
                index++;
                tutorialTextbox.text = currentTutorial.tutorialMessages[index].message;
                //currentTutorial.tutorialMessages[index - 1].dynamicActionCompleted = false;

                StopLerping();
                UnHighlightScreenLocation();
            }
            //backward
            else if(index < currentTutorial.tutorialMessages.Length && index > 0 && back == true)
            {
                index --;
                tutorialTextbox.text = currentTutorial.tutorialMessages[index].message;
                StopLerping();
                UnHighlightScreenLocation();
            }
            else if(back == false)
            {
                CloseCurrentTutorial();
            }
        }
    }

    public bool CheckActiveTutorial(int value)
    {
        return (tutorials[value] == currentTutorial && tutorialPanel.activeSelf);
    }

    #region LERPING METHODS
    // methods relevant to lerping the ghost cursor
    private void BeginLerping(Vector3 start, Vector3 end)
    {
        ghostCursor.SetActive(true);

        lerpStart = start;
        lerpEnd = end;

        timeStartedLerping = Time.time;
        lerping = true;
    }
    private void StopLerping()
    {
        lerping = false;

        ghostCursor.SetActive(false);
    }

    private Vector3 LerpGhostCursor(Vector3 start, Vector3 end, float timeStarted, float ltime = 1)
    {

        timeStarted = Time.time - timeStarted;
        percentageComplete = timeStarted / ltime;

        var result = Vector3.Lerp(start, end, percentageComplete);

        return result;
    }
    #endregion

    #region TUTORIAL SPECIAL EFFECTS
    // these methods display visual effects if the current tutorialMessage contains
    // the relevant boolean
    private void GhostCursorHydroponics()
    {
        if (tutorialPrerequisitesComplete == false)
        {
            if (FindObjectOfType<ShipBuildingShop>().GetCurrentTab() != "Food" || !FindObjectOfType<ShipBuildingShop>().GetShopOpen()) FindObjectOfType<ShipBuildingShop>().ToResourceTab("Food");
            tutorialPrerequisitesComplete = true;
        }
        if(lerping == false) BeginLerping(vecShopPanel.transform.position, vecInsideShip.transform.position);
    }
    private void GhostArmorPlating()
    {
        if (tutorialPrerequisitesComplete == false)
        {
            if (FindObjectOfType<ShipBuildingShop>().GetCurrentTab() != "HullDurability" || !FindObjectOfType<ShipBuildingShop>().GetShopOpen()) FindObjectOfType<ShipBuildingShop>().ToResourceTab("HullDurability");
            tutorialPrerequisitesComplete = true;
        }
        if (lerping == false) BeginLerping(vecShopPanel.transform.position, vecInsideShip.transform.position);
    }
    private void GhostCursorChargingTerminal()
    {
        if (tutorialPrerequisitesComplete == false)
        {
            if (FindObjectOfType<ShipBuildingShop>().GetCurrentTab() != "Energy" || !FindObjectOfType<ShipBuildingShop>().GetShopOpen()) FindObjectOfType<ShipBuildingShop>().ToResourceTab("Energy");
            tutorialPrerequisitesComplete = true;
        }
        if (lerping == false) BeginLerping(vecShopPanel.transform.position, vecInsideShip.transform.position);
    }
    private void GhostCursorArmorPlating()
    {
        if (tutorialPrerequisitesComplete == false)
        {
            if (FindObjectOfType<ShipBuildingShop>().GetCurrentTab() != "HullDurability" || !FindObjectOfType<ShipBuildingShop>().GetShopOpen()) FindObjectOfType<ShipBuildingShop>().ToResourceTab("HullDurability");
            tutorialPrerequisitesComplete = true;
        }
        if (lerping == false) BeginLerping(vecShopPanel.transform.position, vecInsideShip.transform.position);
    }
    private void GhostCursorStatBar()
    {
        if (tutorialPrerequisitesComplete == false)
        {
            tutorialPrerequisitesComplete = true;
        }
        if (lerping == false) BeginLerping(vecStatsLeft.transform.position, vecStatsRight.transform.position);
    }
    private void GhostCursorCrewBunks()
    {
        if (tutorialPrerequisitesComplete == false)
        {
            if (FindObjectOfType<ShipBuildingShop>().GetCurrentTab() != "Crew" || !FindObjectOfType<ShipBuildingShop>().GetShopOpen()) FindObjectOfType<ShipBuildingShop>().ToResourceTab("Crew");
            tutorialPrerequisitesComplete = true;
        }
        if (lerping == false) BeginLerping(vecShopPanel.transform.position, vecInsideShip.transform.position);
    }

    private void EffectSelectRoom()
    {
        if (tutorialPrerequisitesComplete == false)
        { 
            FindObjectOfType<RoomPanelToggle>().OpenPanel(0);
            FindObjectOfType<CrewManagementRoomDetailsMenu>().ChangeCurrentRoom(FindObjectsOfType<RoomStats>()[0].gameObject);
            tutorialPrerequisitesComplete = true;
        }
    }
    #endregion

    public bool GetTutorialActive()
    {
        return tutorialPanel.activeSelf;
    }

    #region CONDITIONAL CONTINUES
    // these alternatives to the ContinueButton method allow scripts outside of Tutorial.cs
    // to go forward a page if the associated tutorial has the required boolean
    // this allows player actions (like placing a room) to advance the tutorial in the right circumstances
    public void conditionalContinuePlaceRoom()
    {
        if (GetTutorialActive() && currentTutorial.tutorialMessages[index].continueOnPlaceRoom) ContinueButton();
    }
    public void ConditionalContinueDelete()
    {
        if (GetTutorialActive() && currentTutorial.tutorialMessages[index].continueOnDeleteRoom) ContinueButton();
    }
    public void ConditionalContinueAddCrew()
    {
        if (GetTutorialActive() && currentTutorial.tutorialMessages[index].continueOnAddCrew) ContinueButton();
    }
    public void ConditionalContinueRotateRoom()
    {
        if (GetTutorialActive() && currentTutorial.tutorialMessages[index].continueOnRoomRotate) ContinueButton();
    }
    public void ConditionalContinueToggleCrewView()
    {
        if (GetTutorialActive() && currentTutorial.tutorialMessages[index].continueOnToggleCV) ContinueButton();
    }
    #endregion

    #region SAVE LOAD
    public void SaveTutorialStatus()
    {
        SavingLoadingManager.instance.Save<TutorialNode[]>("tutorials", tutorials);
        SavingLoadingManager.instance.Save<TutorialNode>("currentTutorial", currentTutorial);
    }
    public void LoadTutorialStatus()
    {
        tutorials = SavingLoadingManager.instance.Load<TutorialNode[]>("tutorials");
        currentTutorial = SavingLoadingManager.instance.Load<TutorialNode>("currentTutorial");
    }
    #endregion
}
