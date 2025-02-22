using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public abstract class Tile: MonoBehaviour, IPointerClickHandler {
    public string tileType;
    protected Image image;
    public int gridX;
    public int gridY;
    protected GridManager gridManager;
    public bool isNonMoveable = false;
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


}


public class Cube : Tile{
    public override bool IsDamagableByAdjMatch(){
        return false;
    }
}

public class Rocket : Tile{
    public override bool IsDamagableByAdjMatch(){
        return false;
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

