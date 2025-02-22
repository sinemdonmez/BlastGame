using System.Collections.Generic;
using UnityEngine;

public class MatchFinder {
    private Tile[,] grid;
    private int gridWidth, gridHeight;

    public MatchFinder(Tile[,] grid, int width, int height) {
        this.grid = grid;
        gridWidth = width;
        gridHeight = height;
    }

    public List<Tile> FindMatches(Tile startTile) {
        List<Tile> matchGroup = new List<Tile>();
        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();

        // Start tile should not be animating
        if (startTile.isCurrentlyAnimating) {
            return matchGroup;
        }

        queue.Enqueue(startTile);
        visited.Add(startTile);

        while (queue.Count > 0) {
            Tile current = queue.Dequeue();
            matchGroup.Add(current);

            foreach (Tile neighbor in GetNeighbors(current)) {
                if (!visited.Contains(neighbor) &&
                    !neighbor.isCurrentlyAnimating &&  // Check if the tile is not animating
                    neighbor.tileType == startTile.tileType) {
                    
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
        return matchGroup;
    }

    public bool HasRocketNeighbor(Tile startTile){
        foreach (Tile neighbor in GetNeighbors(startTile)) {
            if(neighbor.tileType == "vro" || neighbor.tileType == "hro"){
                return true;
            }
        }
        return false;
    }

    public List<List<Tile>> FindAllRocketHintGroups() {
        List<List<Tile>> rocketHintGroups = new List<List<Tile>>();
        bool[,] visited = new bool[gridWidth, gridHeight];


        for (int y = 0; y < gridHeight; y++) {
            for (int x = 0; x < gridWidth; x++) {
                if (!visited[x, y] && grid[x, y] is Cube) {
                    //Debug.Log($"checking tile at {x} {y}");
                    List<Tile> matchGroup = FindMatches(grid[x, y]);

                    if (matchGroup.Count >= 4) {
                        rocketHintGroups.Add(matchGroup);
                    }

                    foreach (Tile tile in matchGroup) {
                        visited[tile.gridX, tile.gridY]= true;
                    }
                }
            }
        }
        return rocketHintGroups;
    }

    public List<Tile> GetNeighbors(Tile tile) {
        List<Tile> neighbors = new List<Tile>();

        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        

        foreach (Vector2Int dir in directions) {
            int newX = tile.gridX + dir.x;
            int newY = tile.gridY + dir.y;

            if (IsValidTile(newX, newY)) {
                neighbors.Add(grid[newX, newY]);
            }
        }
        return neighbors;
    }

    private bool IsValidTile(int x, int y) {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && grid[x, y] != null;
    }
}
