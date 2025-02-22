using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelButton : MonoBehaviour
{
    public Button button;
    public TMP_Text buttonText;
    public LevelLoader levelLoader;
    public Animator buttonAnimator; 
    public float animationDuration = 0.5f;

    private void Start()
    {
        int lastPlayedLevel = levelLoader.GetLastPlayedLevel();

        if (lastPlayedLevel > levelLoader.totalLevels)
        {
            SetFinished();
        }
        else
        {
            UpdateButtonText(lastPlayedLevel);
        }

        button.onClick.AddListener(OnLevelButtonClicked);
    }

    public void UpdateButtonText(int levelNumber)
    {
        
        if(levelNumber>10){
            SetFinished();
            return;
        } 
        buttonText.text = "Level " + levelNumber;
    }

    public void SetFinished()
    {
        buttonText.text = "Finished";
        button.interactable = false;
    }

    private void OnLevelButtonClicked()
    {
        StartCoroutine(PlayAnimationThenLoadLevel());
    }

    private IEnumerator PlayAnimationThenLoadLevel()
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger("Pressed");
        }

        yield return new WaitForSeconds(animationDuration);

        levelLoader.LoadLevel(); // Delegate UI changes to LevelLoader
        gameObject.SetActive(false); // Hide Level Button
    }
}
