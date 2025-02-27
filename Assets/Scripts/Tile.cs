using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public abstract class Tile: MonoBehaviour, IPointerClickHandler {
    public string tileType;
    protected Image image;
    public int gridX;
    public int gridY;
    protected GridManager gridManager;
    public bool isNonMoveable = false;
    public bool isAnimating = false;
    //todo delete
    public bool isCurrentlyAnimating = false;
    public bool isCurrentlyExploding = false; //without this if an explosion goes through where another explosion has already started, it fails.

    public virtual void Initialize(string type, Sprite sprite){
        tileType = type;
        image = GetComponent<Image>();
        image.sprite = sprite;
    }

    //aslında rengine burdan bakmak daha mantıklı mı?? benim bütün spriteları buraya çekesim var ama 
    public void UpdateSprite(Sprite newSprite){
        image.sprite = newSprite;
    }

    public void SetGridPosition(int x, int y){
        gridX = x;
        gridY = y;
    }

    public void SetGridManager(GridManager manager){
        gridManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData){
        if (GameLogic.Instance != null){
            GameLogic.Instance.StartCoroutine(GameLogic.Instance.HandleTileClick(this));
        }
        else{
            Debug.LogError("GameLogic instance is missing!");
        }
    } 

    public abstract bool IsDamagableByAdjMatch();
    public virtual IEnumerator PopTile(Tile tile){
        yield break;
    }


}


public class Cube : Tile{
    public override bool IsDamagableByAdjMatch(){
        return false;
    }

    public override IEnumerator PopTile(Tile tile){
        Debug.Log("cube basıldı");
        if(gridManager == null)
            Debug.Log("gridyko");
        List<Tile> matchGroup = gridManager.MatchFinder.FindMatches(tile);

        if (matchGroup.Count < 2)
        {
            Debug.Log("❌ Match is too small, nothing happens.");
            gridManager.DetectRocketHints();
            gridManager.moveCount++;
            yield break;
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
        
        gridManager.ShiftAndGenerateTiles1();

        yield return null;
    }
}

public class Rocket : Tile{
    public override bool IsDamagableByAdjMatch(){
        return false;
    }

    public override IEnumerator PopTile(Tile tile){
        if (gridManager.MatchFinder.HasRocketNeighbor(tile)){
                yield return StartCoroutine(gridManager.ExplodeRocketCrossCoroutine(tile));
        }
        else{
            yield return StartCoroutine(gridManager.ExplodeRocketCoroutine(tile));
        }

        gridManager.ShiftAndGenerateTiles1();
    }
}

public class Vase : Tile{
    public override bool IsDamagableByAdjMatch(){
        return true;
    }
}

public class Stone : Tile{
    public Stone(){
        isNonMoveable = true;
    }
    public override bool IsDamagableByAdjMatch(){
        return false;
    }
}

public class Box : Tile{
    public Box(){
        isNonMoveable = true;
    }
    public override bool IsDamagableByAdjMatch(){
        return true;
    }
}

