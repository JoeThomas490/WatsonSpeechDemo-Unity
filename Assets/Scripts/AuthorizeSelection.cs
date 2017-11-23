using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthorizeSelection : MonoBehaviour
{
    public GridMaker gridMaker;
    public SpeechToTextExample speechToText;
    public Text cellNameText;
    public RectTransform authorizePanel;
    public Button yesBtn, noBtn;

    bool m_bIsAuthorizing;

	// Use this for initialization
	void Start ()
    {
        m_bIsAuthorizing = false;

    }
	
	// Update is called once per frame
	void Update ()
    {
        if(m_bIsAuthorizing == true)
        {
            speechToText.m_bIsAuthorizing = true;
        }
        else
        {
            speechToText.m_bIsAuthorizing = false;
        }
		
	}

    public void Authorize(string cellName)
    {
        m_bIsAuthorizing = true;

        cellNameText.text = cellName;
        authorizePanel.gameObject.SetActive(true);

        yesBtn.onClick.AddListener(() => {
            GameObject cell = gridMaker.GetGridCellByTag(cellName);
            if (cell != null)
            {
                cell.GetComponent<SpriteRenderer>().color = Color.green;
            }
            authorizePanel.gameObject.SetActive(false);

            yesBtn.onClick.RemoveAllListeners();

            m_bIsAuthorizing = false;
        });

        noBtn.onClick.AddListener(() =>
        {
            authorizePanel.gameObject.SetActive(false);
            m_bIsAuthorizing = false;
            noBtn.onClick.RemoveAllListeners();
        });

    }
}
