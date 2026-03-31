using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{   
    public int totalPoint = 0;
    public int stagePoint;
    public int stageIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void NextStage()
    {   
        stageIndex++;
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
