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
    public TextMeshProUGUI stormToStart, stormToExpand;
    public Toggle stormEnabled;

    public bool rockIsTall;
    public bool rockIsFragile;
    public TextMeshProUGUI rockSpikes;
    public bool aligned; //True = aligned, false = perpendicular
    public GameObject rockIsFragileGameObj;
    public GameObject alignedButton;
    public TextMeshProUGUI lavaTurnsToKill;
    public TextMeshProUGUI fragileFloorCounter;
    public int fragileFloorMax;
    public Toggle isDry;

    public TextMeshProUGUI dryTurnsMaxCounter, turnsWithWaterCounter;
    public int dryTurnsMax, turnsWithWater;

    public bool voidSelected;
    public bool lavaSelected;
    public bool iceSelected;
    public bool fragileFloorSelected;

    public GameObject selectedObject;

    public GameObject heavenButton;
    public int heavenSelected = 6;
    public GameObject hellButton;
    public int hellSelected = 6;
    public GameObject backButton;

    public GameObject errorMessage;
    public GameObject scrollRect;

    [Header("Prefabs")]
    public List<GameObject> rockPrefabs;
    public List<GameObject> waterCoursePrefabs;
    public GameObject knightStatue;
    public GameObject snakePrefab;

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
        DeselectOptions();
        GameObject voidObj = new GameObject(); //Just so we don't set it null
        selectedObject = voidObj;
        voidSelected = true;
    }

    public void AddLava()
    {
        DeselectOptions();
        GameObject lavaObj = new GameObject();
        selectedObject = lavaObj;
        lavaSelected = true;
    }

    public void AddIce()
    {
        DeselectOptions();

        GameObject iceObj = new GameObject();
        selectedObject = iceObj;
        iceSelected = true;
    }

    public void AddFragileFloor()
    {
        DeselectOptions();

        GameObject fragileFloorObj = new GameObject();
        selectedObject = fragileFloorObj;
        fragileFloorSelected = true;
    }

    public void AddStatue()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
        }
        GameObject statue = Instantiate(knightStatue);
        statue.transform.position += new Vector3(statue.transform.position.x, statue.transform.position.y + 1, statue.transform.position.z);
        selectedObject = statue;
        statue.GetComponent<KnightStatue>().ToggleGlow(true, 1);
        statue.transform.SetParent(GameObject.Find("Obstacles").transform);
    }
    
    public void AddSnake()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
        }
        GameObject snake = Instantiate(snakePrefab);
        snake.transform.position += new Vector3(snake.transform.position.x, snake.transform.position.y + 1, snake.transform.position.z);
        selectedObject = snake;
        snake.GetComponent<SnakeBody>().ToggleGlow(true, 1);
    }
    public void LavaTurnsEdit(int num)
    {
        if (int.TryParse(lavaTurnsToKill.text, out int lavaTurns))
        {
            lavaTurns += num;

            if (lavaTurns < 1)
                lavaTurns = 3;
            if (lavaTurns > 3)
                lavaTurns = 1;

            KnightsGameManager.instance.lavaTurns = lavaTurns;
            lavaTurnsToKill.text = lavaTurns.ToString();
        }
    }

    public void FragileFloorEdit(int num)
    {
        if (int.TryParse(lavaTurnsToKill.text, out int lavaTurns))
        {
            lavaTurns += num;

            if (lavaTurns < 1)
                lavaTurns = 3;
            if (lavaTurns > 3)
                lavaTurns = 1;

            fragileFloorMax = lavaTurns;
            fragileFloorCounter.text = lavaTurns.ToString();
        }
    }

    public void AddWaterCourse(int length)
    {
        DeselectOptions();
        GameObject waterCourseObj = Instantiate(waterCoursePrefabs[length - 3]);
        selectedObject = waterCourseObj;

        if (waterCourseObj.TryGetComponent<WaterCourse>(out WaterCourse waterCourseScript))
        {
            waterCourseScript.length = length;
            waterCourseScript.dryCourse = isDry.isOn;
            waterCourseScript.courseDirection = Vector2Int.right;
        }

        if (waterCourseScript.dryCourse)
        {
            waterCourseScript.dryTurns = dryTurnsMax;
            waterCourseObj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
        }
    }

    public void DryCourseToggle()
    {
        Toggle sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>();

        turnsWithWaterCounter.transform.parent.gameObject.SetActive(sender.isOn);
        dryTurnsMaxCounter.transform.parent.gameObject.SetActive(sender.isOn);
    }
    public void DryTurnsMaxEdit(int num)
    {
        if (int.TryParse(dryTurnsMaxCounter.text, out int max))
        {
            max += num;

            if (max < 2)
                max = 4;
            if (max > 4)
                max = 2;

            dryTurnsMax = max;
            dryTurnsMaxCounter.text = max.ToString();
        }
    }

    public void TurnsWithWaterEdit(int num)
    {
        if (int.TryParse(turnsWithWaterCounter.text, out int turns))
        {
            turns += num;
            if (turns < 1)
                turns = 3;
            if (turns > 3)
                turns = 1;
            turnsWithWater = turns;
            turnsWithWaterCounter.text = turns.ToString();
        }
    }
    public void UIElementSelect(GameObject canvaGameObj)
    {
        canvaGameObj.SetActive(true);

        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;

        scrollRect.SetActive(false);

        backButton.SetActive(true);
    }

    public void GoBack(GameObject selectionCanvas)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (KnightsBoardManager.instance.fragileFloorStartPlayer2.Count != KnightsBoardManager.instance.fragileFloorStartPlayer1.Count)
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

        if (voidSelected)
            voidSelected = false;
        if (lavaSelected)
            lavaSelected = false;
        if (fragileFloorSelected)
            fragileFloorSelected = false;

        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
            selectedObject = null;
        }

        scrollRect.SetActive(true);
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

    public void DeselectOptions()
    {
        voidSelected = false;
        lavaSelected = false;
        iceSelected = false;
        fragileFloorSelected = false;
    }
}
