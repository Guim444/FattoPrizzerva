using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class MapEditorData : MonoBehaviour
{
    public static MapEditorData instance;

    public bool editMode = true;
    public bool chooseHeaven;
    public bool chooseHell;
    public TextMeshProUGUI boardSizeX, boardSizeY;

    public bool rockIsTall;
    public bool rockIsFragile;
    public TextMeshProUGUI rockSpikes;
    public bool aligned; //True = aligned, false = perpendicular
    public GameObject rockIsFragileGameObj;
    public GameObject alignedButton;

    public bool voidSelected;
    public bool lavaSelected;

    public GameObject selectedObject;

    public GameObject heavenButton;
    public int heavenSelected = 6;
    public GameObject hellButton;
    public int hellSelected = 6;
    public GameObject backButton;

    public GameObject errorMessage;

    [Header("Prefabs")]
    public List<GameObject> rockPrefabs;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void LargeRockToggle()
    {
        Toggle sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>();

        rockIsTall = sender.isOn;

        rockIsFragileGameObj.SetActive(!rockIsTall);

        if (rockIsTall)
            rockIsFragile = false;

        rockSpikes.transform.parent.gameObject.SetActive(!rockIsFragile);
    }
    public void FragileToggle()
    {
        Toggle sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>();
        rockIsFragile = sender.isOn;

        rockSpikes.transform.parent.gameObject.SetActive(!rockIsFragile);

    }
    public void SpikeEdit(int num)
    {
        if (int.TryParse(rockSpikes.text, out int spikes))
        {
            spikes += num;

            if (spikes < 0)
                spikes = 4;
            if (spikes > 4)
                spikes = 0;

            if (spikes == 2)
                alignedButton.SetActive(true);
            else
                alignedButton.SetActive(false);

            rockSpikes.text = spikes.ToString();
        }
    }
    public void IsAligned()
    {
        TextMeshProUGUI sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (aligned)
        {
            aligned = false;
            sender.text = "Not aligned";
        }
        else
        {
            aligned = true;
            sender.text = "Aligned";
        }
    }

    public void AddRock()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
        }
        int i = -1;
        int spikes = int.Parse(rockSpikes.text);

        if (rockIsFragile)
        {
            i = 12;
        }
        else if (rockIsTall)
        {
            if (spikes == 2)
                i = aligned ? 8 : 9;
            else
                i = spikes == 0 ? 6 :
                        spikes == 1 ? 7 :
                        spikes == 3 ? 10 :
                        spikes == 4 ? 11 : -1;
        }
        else
        {
            if (spikes == 2)
                i = aligned ? 2 : 3;
            else
                i = spikes == 0 ? 0 :
                        spikes == 1 ? 1 :
                        spikes == 3 ? 4 :
                        spikes == 4 ? 5 : -1;
        }

        if (i < 0)
            return;

        GameObject rock = Instantiate(rockPrefabs[i]);

        RockObstacleScript rockScript = rock.GetComponent<RockObstacleScript>();

        rockScript.isTall = rockIsTall;

        if (!rockScript.isTall && rockIsFragile)
        {
            rockScript.isBreakable = true;
        }
        else
            rockScript.isBreakable = false;

        selectedObject = rock;

        rockScript.ActivateGlow(true);

        rock.transform.SetParent(GameObject.Find("Obstacles").transform);
    }

    public void AddVoid()
    {
        GameObject voidObj = new GameObject(); //Just so we don't set it null
        selectedObject = voidObj;
        voidSelected = true;
    }

    public void AddLava()
    {
        GameObject lavaObj = new GameObject();
        selectedObject = lavaObj;
        lavaSelected = true;
    }

    public void UIElementSelect(GameObject canvaGameObj)
    {
        canvaGameObj.SetActive(true);

        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;

        sender.SetActive(false);

        backButton.SetActive(true);
    }

    public void GoBack(GameObject selectionCanvas)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (KnightsBoardManager.instance.fragileFloorPlayer2.Count != KnightsBoardManager.instance.fragileFloorPlayer1.Count)
        {
            StartCoroutine(WarningMessage("Both players must have the same number of cracked squares."));
            return;
        }

        if (KnightsBoardManager.instance.lavaStartSquaresPlayer1.Count != KnightsBoardManager.instance.lavaStartSquaresPlayer2.Count)
        {
            StartCoroutine(WarningMessage("Both players must have the same number of lava squares."));
            return;
        }

        foreach (Transform child in sender.transform.parent)
        {
            child.gameObject.SetActive(false);
        }

        selectionCanvas.SetActive(true);
    }

    public void SelectStartSquare(string category)
    {
        if (category == "Heaven")
        {
            chooseHeaven = true;
            chooseHell = false;
        }
        else if (category == "Hell")
        {
            chooseHeaven = false;
            chooseHell = true;
        }
    }

    public void DoneSelecting(GameObject editorCanvas)
    {
        chooseHeaven = false;
        chooseHell = false;
        if (KnightsBoardManager.instance.player1StartZone.Count < 3 || KnightsBoardManager.instance.player2StartZone.Count < 3)
        {
            StartCoroutine(WarningMessage("There should be a minimum of 3 squares on each side."));
            return;
        }
        if (hellSelected != heavenSelected)
        {
            StartCoroutine(WarningMessage("Both players must have the same number of squares."));
            return;
        }

        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        sender.transform.parent.transform.parent.gameObject.SetActive(false); // Canvas

        editorCanvas.SetActive(true);
    }
    public IEnumerator WarningMessage(string message)
    {
        errorMessage.SetActive(true);
        errorMessage.GetComponentInChildren<TextMeshProUGUI>().text = message;
        yield return new WaitForSeconds(3);
        errorMessage.SetActive(false);
    }
}
