using System;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject replayButton;
    [SerializeField] private GameObject gameRoot;
    private void Start()
    {
        if (playButton != null) {
            playButton.GetComponent<FadeUI>().FadeIn();
        }
        if (replayButton != null) 
        { 
            replayButton.GetComponent<FadeUI>().FadeOut();
        }
        if(gameRoot != null) gameRoot.SetActive(false);
    }
    
    public void OnPlayPressed()
    {
        if (playButton != null)
        {
            playButton.GetComponent<FadeUI>().FadeOut();
        }
        if (gameRoot != null) gameRoot.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();

    }
    public void OnReplayPressed()
    {
        GameManager.Instance.ResetGame();
        GameManager.Instance.StartGame();
        if (replayButton) {
            replayButton.GetComponent<FadeUI>().FadeOut();
        }
        if(playButton) playButton.SetActive(false);
    }

    public void ShowReplayButton()
    {
        if (replayButton != null) replayButton.GetComponent<FadeUI>().FadeIn();
    }

    
}
