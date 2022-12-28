using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle_UI : MonoBehaviour
{
    public GameObject top;
    public GameObject stick;
    public float minHeight { get; private set; } = 64f;
    float maxHeight;
    float maxSize;
    PlayerStats playerStats;
    float m_maxLife;
    //public GameObject bottom;
    private void Start()
    {
        maxHeight = stick.GetComponent<RectTransform>().sizeDelta.y;
        maxSize = maxHeight - minHeight;

        playerStats = PlayerStats.instance;

        //TODO: this should be a variable that is set in the PlayerStats class and passed here 
        m_maxLife = playerStats.torchLife;
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rectTransform = top.GetComponent<RectTransform>();
        RectTransform stickRT = stick.GetComponent<RectTransform>();
        stickRT.sizeDelta = new Vector2( stickRT.sizeDelta.x , minHeight + (maxSize * (playerStats.torchLife / m_maxLife)));
        Vector2 delta = stickRT.sizeDelta;
        delta.y = delta.y < minHeight ? minHeight: delta.y;
        
        rectTransform.sizeDelta = delta;
    }
}
