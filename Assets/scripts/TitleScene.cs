using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("Stage1");
    }
}
