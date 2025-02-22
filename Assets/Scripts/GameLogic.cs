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

        if (tile is Rocket)
        {
            yield return StartCoroutine(RocketLogic(tile));
            
        }else{
            CubeLogic(tile);
        }

        //isProcessingClick = false;
        
        //TODO: bunların da coroutineden sonra olması lazım.
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

    }

    private IEnumerator RocketLogic(Tile tile){
        if (gridManager.MatchFinder.HasRocketNeighbor(tile))
            {
                Debug.Log("🚀 ROCKET COMBO!");
                yield return StartCoroutine(gridManager.ExplodeAndShiftCross(tile));
                yield break;
            }
        yield return StartCoroutine(gridManager.ExplodeAndShift(tile));
    }

    private void CubeLogic(Tile tile){
        List<Tile> matchGroup = gridManager.MatchFinder.FindMatches(tile);

        if (matchGroup.Count < 2)
        {
            Debug.Log("❌ Match is too small, nothing happens.");
            gridManager.DetectRocketHints();
            gridManager.moveCount++;
            return;
        }
        gridManager.DamageAdjacentCells(matchGroup);

        if (matchGroup.Count >= 4){
            Debug.Log($"🚀 Converting tile at ({tile.gridX}, {tile.gridY}) into a rocket!");
            matchGroup.Remove(tile);
            gridManager.RemoveTiles(matchGroup);
            gridManager.ConvertToRocketTile(tile);
        }
        else
        {
            gridManager.RemoveTiles(matchGroup);
        }
        
        
        
        gridManager.ShiftAndGenerateTiles(() =>
        {
            gridManager.DetectRocketHints();
        });
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
