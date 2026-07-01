using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public Image panel;

    IEnumerator Fadeout()
    {
        Color color = panel.color;

        while (color.a > 0)
        {
            color.a -= Time.deltaTime;
            panel.color = color;

            yield return null;
        }
    }

    IEnumerator Fadein()
    {
        Color color = panel.color;

        while (color.a < 1)
        {
            color.a += Time.deltaTime;
            panel.color = color;

            yield return null;
        }
    }

    IEnumerator StartGameCoroutine()
    {
        panel.gameObject.SetActive(true);
        yield return StartCoroutine(Fadein());
        SceneManager.LoadScene("Stage1");
    }

    void Awake()
    {
        panel.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }
}