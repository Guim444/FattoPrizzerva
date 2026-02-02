using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ShowCanva(GameObject canva)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        canva.SetActive(true);
        sender.SetActive(false);
    }
    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
