using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance;
    private GridManager gridManager;
    private bool isProcessingClick = false;  // Flag to prevent multiple clicks
    public GameObject failPopup;
    public LevelLoader levelLoader;  // Reference to LevelLoader
    public GameObject levelButton;   // Reference to the LevelButton
    public Button TryAgainButton;
    public Button CloseButton;
    public GameObject gridContainer;
    public GameObject winScreen;



    void Awake(){
        Instance = this;
        TryAgainButton.onClick.AddListener(TryAgain);
        CloseButton.onClick.AddListener(Close);
    }

    public void Initialize(GridManager manager){
        gridManager = manager;
    }

    public IEnumerator HandleTileClick(Tile tile){

        if (gridManager == null || isProcessingClick || tile is Box || tile is Stone || tile is Vase)
        {
            Debug.Log("Click ignored");
            yield break;
        }
        

        isProcessingClick = true;
        gridManager.moveCount--;
        gridManager.RemoveHints();


        yield return StartCoroutine(tile.PopTile(tile));

        gridManager.ShowMoveandGoals();

        if(gridManager.moveCount == 0 && !gridManager.levelDone){//lost the level
            //butonun da animasyonu olması lazım
            failPopup.SetActive(true);


        }else if(gridManager.levelDone){//won the level, later.
            levelLoader.IncreaseLevel();
            levelButton.GetComponent<LevelButton>().UpdateButtonText(levelLoader.GetLastPlayedLevel());
            StartCoroutine(HandleLevelCompletion());
        }

        isProcessingClick = false;
        gridManager.DetectRocketHints();

    }

    private void Close(){
        failPopup.SetActive(false);
        gridManager.uiTop.SetActive(false);
        gridContainer.SetActive(false);
        levelButton.SetActive(true);
    }

    public void TryAgain(){
        //eskileri harca aga.
        Debug.Log("tryagain called");
        failPopup.SetActive(false);
        levelLoader.LoadLevel();
    }

    private IEnumerator HandleLevelCompletion() {
        winScreen.SetActive(true);
        gridManager.uiAnimator.WinAnimation();
        Close();
        yield return new WaitForSeconds(2.2f); // ⏳ Wait for 2 seconds

        winScreen.SetActive(false);
    }



}
