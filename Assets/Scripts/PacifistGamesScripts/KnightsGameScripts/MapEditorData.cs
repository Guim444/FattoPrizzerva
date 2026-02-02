using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapEditorData : MonoBehaviour
{
    public static MapEditorData instance;
    public TextMeshProUGUI boardSizeX, boardSizeY;

    public bool rockIsTall;
    public bool rockIsFragile;
    public TextMeshProUGUI rockSpikes;
    public bool aligned; //True = aligned, false = perpendicular
    public GameObject rockIsFragileGameObj;
    public GameObject alignedButton;

    public GameObject selectedObject;

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
    }
}
