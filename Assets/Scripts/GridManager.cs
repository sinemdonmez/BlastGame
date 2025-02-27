using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;



public class GridManager : MonoBehaviour{
    public static GridManager Instance;
    //creates a global reference to the gridmanager instance and ensures only one grid manager exists in the game
    //in other scripts you can access it via GridManager.Instance

    public GameObject tilePrefab;
    //gameobject is the fundamental building block of any scene. it serves as a container that holds the components
    //it has key components such as transform(mandatory (position,rotation,scale in scene)), rigidbody, script components
    //you can add components with gameobject.AddComponent<RigidBody>();
    //you can destroy them via Destroy(gameObject);
    //prefabs -> reusable gameobjects. a preconfigured model that can be *instantiated* multiple times.
    //        // Instantiate at position (0, 0, 0) and zero rotation.
    //prefab instantiation Instantiate(prefab,position);
    //Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity);

    //tilePrefab -> represents each tile in the grid.
    public GameObject goalItemPrefab;
    public UIAnimator uiAnimator;
    


    public Transform gridContainer;
    //this is UI Transform refer    public int columns = 6;
    public MatchFinder MatchFinder;

    public GameObject uiTop;

    public Sprite blueSprite;
    public Sprite yellowSprite;
    public Sprite redSprite;
    public Sprite greenSprite;

    public Sprite blueRocketHintSprite;
    public Sprite yellowRocketHintSprite;
    public Sprite redRocketHintSprite;
    public Sprite greenRocketHintSprite;

    public Sprite horizontalRocketSprite;
    public Sprite horizontalRightRocketSprite;
    public Sprite horizontalLeftRocketSprite;
    public Sprite verticalRocketSprite;
    public Sprite verticalTopRocketSprite;
    public Sprite verticalBottomRocketSprite;
    public GameObject explosionEffectPrefab;

    public Sprite boxSprite;
    public Sprite stoneSprite;
    public Sprite vaseSprite;
    public Sprite vaseCrackedSprite;
    public Sprite goalCheck;

    private Tile[,] grid;
    private UnityEngine.Vector3[,] positionGrid;
    public int gridWidth;
    public int gridHeight;
    public int moveCount;

    private float spacing = 5f; // Space between images
    private float imageSize = 70f; // Size of each square image
    private int activeTileMovements = 0;
    private int numBox = 0, numStone = 0,  numVase = 0;
    private HashSet<Coroutine> activeExplosions = new HashSet<Coroutine>(); //track all running explosions
    public bool levelDone = false;



    void Awake(){
        Instance = this;
        GameLogic.Instance?.Initialize(this);
    }

    public void InitializeGrid(LevelData levelData){
        levelDone=false;

        gridWidth = levelData.grid_width;
        gridHeight = levelData.grid_height;
        
        moveCount = levelData.move_count;
        //moveCount = 3;
        grid = new Tile[gridWidth, gridHeight];

        MatchFinder = new MatchFinder(grid, gridWidth, gridHeight);

        

        GenerateGrid(levelData);
        DetectRocketHints();
        ShowMoveandGoals();
    }

    private void GenerateGrid(LevelData levelData){
        ClearExistingTiles();
        int rows = gridHeight;
        int columns = gridWidth;

        float totalWidth = columns * imageSize + (columns - 1) * spacing;
        float totalHeight = rows * imageSize + (rows - 1) * spacing;

        // Set container size and anchor it correctly
        RectTransform rectTransform = gridContainer.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

        // Set pivot to top-left
        rectTransform.pivot = new Vector2((float)0.5, (float)0.5);
        rectTransform.anchoredPosition = Vector2.zero;

        // Correct starting position: align first tile exactly at the top-left
        Vector2 startOffset = new Vector2(imageSize/2, totalHeight - imageSize/2); 

        positionGrid = new Vector3[gridWidth, gridHeight];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = (rows - 1 - y) * columns + x; // Reverse row index to read top-down
                string tileType = levelData.grid[index];

                // Fix Y positioning: Start from the very top and move downward
                Vector2 tilePosition = new Vector2(
                    startOffset.x + x * (imageSize + spacing),
                    startOffset.y - y * (imageSize + spacing) // Now aligns top correctly
                );

                GenerateNewTile(tilePosition, tileType, x, y);
            }
        }
    }
    

    public void RemoveHints(){
         for (int y = 0; y < gridHeight; y++){
            for (int x = 0; x < gridWidth; x++){
                if (grid[x, y] is Cube cube ){
                    cube.UpdateSprite(GetTileSprite(cube.tileType));
                }
            }
        }
    }

    public void DetectRocketHints() {
        for (int y = 0; y < gridHeight; y++){
            for (int x = 0; x < gridWidth; x++){
                if (grid[x, y] is Cube cube){
                    List<Tile> matchGroup = MatchFinder.FindMatches(cube);
                    if (matchGroup.Count >= 4){
                        cube.UpdateSprite(GetRocketSpriteForColor(cube.tileType));
                    }
                    else{
                        cube.UpdateSprite(GetTileSprite(cube.tileType)); // ✅ Reset to normal sprite if not a hint
                    }
                }
            }
        }
    }


    public void RemoveTiles(List<Tile> tiles) {
        foreach (Tile tile in tiles) {
            if(tile.tileType == "v"){ //crack the vase first
                tile.UpdateSprite(GetTileSprite("cv"));
                tile.tileType = "cv";
                uiAnimator.AnimateDestructionOnTile(tile);

            }
            else{
                grid[tile.gridX, tile.gridY] = null;
                tile.GetComponent<Image>().enabled = false;
                
                uiAnimator.AnimateDestructionOnTile(tile);
                Destroy(tile.gameObject);

            }
        }
    }
   



    public void DamageAdjacentCells(List<Tile> tiles) {
        HashSet<Tile> uniqueNeighbors = new HashSet<Tile>();

        foreach (Tile tile in tiles){
            List<Tile> neighbors = MatchFinder.GetNeighbors(tile);

            foreach (Tile neighbor in neighbors) {
                if (neighbor.IsDamagableByAdjMatch() && !tiles.Contains(neighbor)) {
                    uniqueNeighbors.Add(neighbor);
                }
            }
        }

        RemoveTiles(uniqueNeighbors.ToList());
    }

    private void ClearExistingTiles() {
        foreach (Transform child in gridContainer){
            if (child.gameObject.name == "GridBackground"){
                continue; // Skip this object
            }
            Destroy(child.gameObject);
        }
    }

    private Sprite GetTileSprite(string type) {
        if (type == "b") return blueSprite;
        if (type == "g") return greenSprite;
        if (type == "y") return yellowSprite;
        if (type == "r") return redSprite;
        if (type == "vro") return verticalRocketSprite;
        if (type == "hro") return horizontalRocketSprite;
        if (type == "bo") return boxSprite;
        if (type == "s") return stoneSprite;
        if (type == "v") return vaseSprite;
        if (type == "cv") return vaseCrackedSprite;
        return null;
    }

    private Sprite GetRocketSpriteForColor(string tileType) {
        if (tileType == "b") return blueRocketHintSprite;
        if (tileType == "g") return greenRocketHintSprite;
        if (tileType == "r") return redRocketHintSprite;
        if (tileType == "y") return yellowRocketHintSprite;
        return null;
    }


    public void ConvertToRocketTile(Tile tile){
        if (tile == null)
        {
            return;
        }

        int x = tile.gridX;
        int y = tile.gridY;
        Vector2 position = positionGrid[x, y]; // Get the stored position


        // Randomly choose "vro" (vertical rocket) or "hro" (horizontal rocket)
        string newTileType = Random.Range(0, 2) == 0 ? "vro" : "hro";

        // Remove the old tile
        Destroy(tile.gameObject);
        grid[x, y] = null; 

        // Generate the new rocket tile at the same position
        GenerateNewTile(position, newTileType, x, y);
    }

    private string GetRandomTileType() {
        string[] tileTypes = { "r", "g", "b", "y" };
        return tileTypes[Random.Range(0, tileTypes.Length)];
    }

    //TODO

    public void MoveGeneratedTile(Tile tile, int targetX, int targetY){
        // Get world positions from positionGrid
        Vector3 targetPosition = positionGrid[targetX, targetY];


        // Start movement coroutine
        StartCoroutine(MoveTileGeneric(tile, targetPosition, 500f));

    }

    private IEnumerator MoveTileGeneric(Tile tile, Vector3 targetPosition, float speed = 10f){
        if (tile == null)
        {
            yield break;
        }

        RectTransform tileRect = tile.GetComponent<RectTransform>(); 
        Vector2 startPosition = tileRect.anchoredPosition;
        Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.y);



        activeTileMovements++; // ✅ Track this movement

        while (Vector2.Distance(tileRect.anchoredPosition, targetPos2D) > 1f)
        {
            tileRect.anchoredPosition = Vector2.MoveTowards(tileRect.anchoredPosition, targetPos2D, speed * Time.deltaTime);
            yield return null;
        }

        tileRect.anchoredPosition = targetPos2D;

        // ✅ Ensure decrement happens only once
        if (activeTileMovements > 0)
        {
            activeTileMovements--;
        }

    }







    private Queue<(List<(Tile, Vector3)> animations, bool isGenerate)> animationQueue = new Queue<(List<(Tile, Vector3)>, bool)>();
    private Dictionary<Tile, Coroutine> activeCoroutines = new Dictionary<Tile, Coroutine>();

    private bool isProcessingQueue = false;



    public void ShiftAndGenerateTiles1(){
        // Step 1: Shift existing tiles down
        List<(Tile, Vector3)> shiftAnimations = ShiftTiles1();

        // Step 2: Generate new tiles (returns a list of column-wise lists)
        List<List<(Tile, Vector3)>> generateAnimations = GenerateNewTiles1();

        // Step 3: Add shift animations to the queue (false = no delay)
        if (shiftAnimations.Count > 0)
            animationQueue.Enqueue((shiftAnimations, false));

        // Step 4: Add generate animations to the queue (true = delay inside the column)
        foreach (var column in generateAnimations)
            animationQueue.Enqueue((column, true)); // ✅ Columns queued but run independently

        // Step 5: Start processing if not already running
        if (!isProcessingQueue)
            StartCoroutine(ProcessAnimationQueue());
    }

    private IEnumerator ProcessAnimationQueue(){
        isProcessingQueue = true;

        while (animationQueue.Count > 0){
            (List<(Tile, Vector3)> currentBatch, bool isGenerate) = animationQueue.Dequeue();

            // ✅ Start animations immediately (does NOT block queue)
            StartCoroutine(AnimateTilesWithDelay(currentBatch, isGenerate ? 0.1f : 0f));
        }

        isProcessingQueue = false;
        yield return null; // ✅ Ensures function always returns a value
    }

    private IEnumerator AnimateTilesWithDelay(List<(Tile, Vector3)> columnAnimations, float delayBetweenTiles){
        int activeAnimations = columnAnimations.Count;
        
        

        foreach (var (tile, targetPosition) in columnAnimations){
            if(delayBetweenTiles>0)
                yield return new WaitForSeconds(delayBetweenTiles);

            tile.gameObject.SetActive(true);

            // ✅ If a coroutine is already running on this tile, stop it
            if (activeCoroutines.TryGetValue(tile, out Coroutine runningCoroutine)){
                StopCoroutine(runningCoroutine);
                activeCoroutines.Remove(tile);
            }

            // ✅ Mark tile as animating
            tile.isAnimating = true;

            // Start and track the new coroutine
            Coroutine newCoroutine = StartCoroutine(MoveTileGeneric1(tile, targetPosition, 700f, () =>{
                activeAnimations--;
                tile.isAnimating = false;
                activeCoroutines.Remove(tile); // ✅ Remove from tracking after completion
            }));

            activeCoroutines[tile] = newCoroutine; // ✅ Store new coroutine reference

            //yield return new WaitForSeconds(delayBetweenTiles);
        }

        // ✅ Wait for all tiles in this column to finish before marking as done
        while (activeAnimations > 0)
            yield return null;

        PrintGrid();
    }



    public List<(Tile, Vector3)> ShiftTiles1(){
        List<(Tile, Vector3)> newAnimations = new List<(Tile, Vector3)>();

        for (int x = 0; x < gridWidth; x++){
            for (int y = gridHeight - 1; y >= 0; y--){
                if (grid[x, y] == null){
                    for (int aboveY = y - 1; aboveY >= 0; aboveY--){
                        if (grid[x, aboveY] != null && !grid[x, aboveY].isNonMoveable){
                            Vector3 targetPosition = positionGrid[x, y];

                            // ✅ If already animating, stop the tile's specific coroutine
                            if (activeCoroutines.TryGetValue(grid[x, aboveY], out Coroutine runningCoroutine)){
                                StopCoroutine(runningCoroutine);
                                activeCoroutines.Remove(grid[x, aboveY]);
                            }

                            newAnimations.Add((grid[x, aboveY], targetPosition));

                            // ✅ Mark as animating
                            grid[x, aboveY].isAnimating = true;

                            // Move tile down logically
                            grid[x, aboveY].SetGridPosition(x, y);
                            grid[x, y] = grid[x, aboveY];
                            grid[x, aboveY] = null;

                            break; // Stop searching after moving a tile
                        }

                        if (grid[x, aboveY] != null && grid[x, aboveY].isNonMoveable)
                            break; // Stop searching if a non-moveable tile is encountered
                    }
                }
            }
        }

        return newAnimations;
    }

    public List<List<(Tile, Vector3)>> GenerateNewTiles1(){
        List<List<(Tile, Vector3)>> allColumnAnimations = new List<List<(Tile, Vector3)>>();
        for (int x = 0; x < gridWidth; x++){
            List<(Tile, Vector3)> columnAnimations = new List<(Tile, Vector3)>();
            int firstEmptyY = -1;

            // Step 1: Identify the first empty space at the top of the column
            for (int y = 0; y < gridHeight; y++){
                if (grid[x, y] == null){ // Found an empty space
                    if (firstEmptyY == -1){
                        firstEmptyY = y; // Mark the first empty space
                    }
                }
                else
                    break;
            }

            // Step 2: If a top empty group exists, log from bottom of that group upwards
            if (firstEmptyY != -1){
                for (int y = gridHeight - 1; y >= firstEmptyY; y--){
                    if (grid[x, y] == null){
                        Debug.Log($"New tile at {x},{y}");
                        Tile newTile = InstantiateTileAtTop(x);
                        
                        
                        Vector3 targetPosition = positionGrid[x, y];
                        columnAnimations.Add((newTile, targetPosition));
                        newTile.SetGridPosition(x,y);
                        grid[x,y] = newTile;
                    }
                }
            }

        if (columnAnimations.Count > 0)
            allColumnAnimations.Add(columnAnimations);
        
        }

        return allColumnAnimations;
    }

    public Tile InstantiateTileAtTop(int x){
        GameObject newTile = Instantiate(tilePrefab, gridContainer);
        RectTransform tileRect = newTile.GetComponent<RectTransform>();
        newTile.SetActive(false);
        newTile.transform.SetAsLastSibling();

        // Set initial position at y = -1 (just above the grid)
        Vector3 spawnPosition = new Vector3(
            positionGrid[x, 0].x,
            positionGrid[x, 0].y + (2 * (imageSize + spacing)),
            0
        );

        tileRect.anchoredPosition = spawnPosition;
        tileRect.sizeDelta = new Vector2(imageSize, imageSize);

        // Assign a random tile type
        string tileType = GetRandomTileType();

        // Add Cube component and initialize
        Tile tileComponent = newTile.AddComponent<Cube>();
        tileComponent.Initialize(tileType, GetTileSprite(tileType));
        tileComponent.SetGridManager(this);

        return tileComponent;
    }




    private IEnumerator MoveTileGeneric1(Tile tile, Vector3 targetPosition, float speed, System.Action onComplete){
        if (tile == null)
            yield break;

        RectTransform tileRect = tile.GetComponent<RectTransform>(); 
        Vector2 startPosition = tileRect.anchoredPosition;
        Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.y);

        // ✅ Smoothly transition to new target position
        while (Vector2.Distance(tileRect.anchoredPosition, targetPos2D) > 1f){
            tileRect.anchoredPosition = Vector2.MoveTowards(tileRect.anchoredPosition, targetPos2D, speed * Time.deltaTime);
            yield return null;
        }

        tileRect.anchoredPosition = targetPos2D;

        onComplete?.Invoke();
    }




    public void PrintGrid(){
        string gridString = "";

        for (int y = 0; y <gridHeight; y++){
            for (int x = 0; x < gridWidth; x++){
                if (grid[x, y] == null)
                    gridString += "[  ] "; // Empty cell
                else
                    gridString += $"[{grid[x, y].tileType}] "; // Print tile type
            }
            gridString += "\n"; // New row
        }

        Debug.Log(gridString);
    }























    public void ShiftAndGenerateTiles(System.Action onComplete = null){
        StartCoroutine(ShiftAndGenerateTilesCoroutine(onComplete));
    }

    private IEnumerator ShiftAndGenerateTilesCoroutine(System.Action onComplete){
        Dictionary<int, int> emptyCells = CountEmptyCellsPerColumn();
        Tile[,] tempGrid = (Tile[,])grid.Clone();

        // Step 1: Shift tiles down (run as a coroutine)
        IEnumerator shiftCoroutine = ShiftTilesDown(tempGrid);

        // Step 2: Generate new tiles (run as a coroutine)
        IEnumerator generateCoroutine = GenerateNewTilesWithDelay(emptyCells);

        // ✅ Wait for both coroutines to complete
        yield return WaitForAll(shiftCoroutine, generateCoroutine);

        //Debug.Log("✅ All tile movements complete. Running DetectRocketHints().");

        // Step 3: Detect rocket hints after all movements are done
        DetectRocketHints();

        // ✅ Callback after everything is complete
        onComplete?.Invoke();
    }

    private IEnumerator ShiftTilesDown(Tile[,] tempGrid){
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = gridHeight - 1; y >= 0; y--)
            {
                if (grid[x, y] == null)
                {
                    for (int aboveY = y - 1; aboveY >= 0; aboveY--)
                    {
                        if (tempGrid[x, aboveY] != null && !tempGrid[x, aboveY].isNonMoveable)
                        {
                            MoveToTarget(x, aboveY, x, y);
                            grid[x, y] = tempGrid[x, aboveY];
                            grid[x, aboveY] = null;
                            tempGrid[x, aboveY] = null;
                            break;
                        }
                        if(tempGrid[x, aboveY] != null && tempGrid[x, aboveY].isNonMoveable){
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitUntil(() => activeTileMovements == 0); // ✅ Wait for all tile movements
    }


    private IEnumerator GenerateNewTilesWithDelay(Dictionary<int, int> emptyCells)
    {
        List<IEnumerator> coroutines = new List<IEnumerator>();

        foreach (var entry in emptyCells)
        {
            int x = entry.Key;
            int numNewTiles = entry.Value;
            
            // Start a separate coroutine for each column
            coroutines.Add(GenerateTilesForColumn(x, numNewTiles));
        }

        // Wait for all coroutines to complete
        yield return WaitForAll(coroutines.ToArray());
    }

    // ✅ Helper function to generate tiles for a single column
    private IEnumerator GenerateTilesForColumn(int x, int numNewTiles){

        List<Tile> generatedTiles = new List<Tile>();

        for (int i = 0; i < numNewTiles; i++)
        {
            int targetY = numNewTiles - 1 - i; // First tile goes to the highest empty space
            int spawnY = -1; // Always spawn above the grid

            //Debug.Log($"🆕 Spawning new tile at ({x}, {spawnY}) to fill ({x}, {targetY})");

            GameObject newTile = Instantiate(tilePrefab, gridContainer);
            RectTransform tileRect = newTile.GetComponent<RectTransform>();
            newTile.SetActive(false);
            newTile.transform.SetAsLastSibling();

            // Set initial position at y = -1 (just above the grid)
            Vector3 spawnPosition = new Vector3(
                positionGrid[x, 0].x,
                positionGrid[x, 0].y + (2 * (imageSize + spacing)),
                0
            );

            tileRect.anchoredPosition = spawnPosition;
            tileRect.sizeDelta = new Vector2(imageSize, imageSize);

            // Assign a random tile type
            string tileType = GetRandomTileType();

            // Add Cube component and initialize
            Tile tileComponent = newTile.AddComponent<Cube>();
            tileComponent.Initialize(tileType, GetTileSprite(tileType));
            tileComponent.SetGridManager(this);
            grid[x, targetY] = tileComponent;

            tileComponent.SetGridPosition(x, targetY);
            tileComponent.isCurrentlyAnimating = true;

            generatedTiles.Add(tileComponent);
            
        }

        yield return StartCoroutine(AnimateGeneratedFall(generatedTiles, x));
    }

    private IEnumerator AnimateGeneratedFall(List<Tile> generatedTiles, int x){
        for (int i = 0; i < generatedTiles.Count; i++)
        {
            int targetY = generatedTiles.Count - 1 - i;
            generatedTiles[i].gameObject.SetActive(true);
            MoveGeneratedTile(generatedTiles[i], x, targetY);
            yield return new WaitForSeconds(0.2f); // Delay for smooth animation
            generatedTiles[i].isCurrentlyAnimating = false;
        }
    }


    public void GenerateNewTile(Vector2 position, string tileType, int x, int y){
            
            if (tileType == "rand"){
                tileType = GetRandomTileType();
            } 

            // Instantiate tile
            GameObject newTile = Instantiate(tilePrefab, gridContainer);
            RectTransform tileRect = newTile.GetComponent<RectTransform>();
            Tile tileComponent;
            if (tileType == "vro" || tileType == "hro"){
                tileComponent = newTile.AddComponent<Rocket>();
            }else if (tileType == "bo"){
                tileComponent = newTile.AddComponent<Box>();
            }else if (tileType == "s"){
                tileComponent = newTile.AddComponent<Stone>();
            }else if (tileType == "v"){
                tileComponent = newTile.AddComponent<Vase>();
            }else{
                tileComponent = newTile.AddComponent<Cube>();
            }

            // Assign position to RectTransform
            tileRect.anchoredPosition = position;
            tileRect.sizeDelta = new Vector2(imageSize, imageSize);

            // Add Cube component and initialize
            
            tileComponent.Initialize(tileType, GetTileSprite(tileType));
            tileComponent.SetGridManager(this);
            tileComponent.SetGridPosition(x,y);
            grid[x, y] = tileComponent;
            positionGrid[x, y] = position;
    }


    public Dictionary<int, int> CountEmptyCellsPerColumn(){
        Dictionary<int, int> emptyCellsPerColumn = new Dictionary<int, int>();

        for (int x = 0; x < gridWidth; x++) // Loop through columns
        {
            int emptyCount = 0;

            for (int y = 0; y < gridHeight; y++) // Start from the top
            {
                if (grid[x, y] != null && grid[x, y].isNonMoveable) // Stop counting if immovable object is found
                {
                    break;
                }

                if (grid[x, y] == null)
                {
                    emptyCount++;
                }
            }

            emptyCellsPerColumn[x] = emptyCount; // Store empty count for this column
        }

        return emptyCellsPerColumn;
    }


    public void MoveToTarget(int currentX, int currentY, int targetX, int targetY){
        //benim taşımakta olduğum şeyi başkası silmeye çaışıo olabilirm i?
        
        // Ensure both positions are within bounds
        if (!IsValidTile(currentX, currentY))
        {

            return;
        }

        // Get the tile from the grid
        Tile tile = grid[currentX, currentY];

        if (tile == null)
        {
            //Debug.LogError($"No tile found at ({currentX}, {currentY})");
            return;
        }

        // Get world positions from positionGrid
        Vector3 targetPosition = positionGrid[targetX, targetY];

        StartCoroutine(MoveTileGeneric(tile, targetPosition, 500f));

        grid[targetX, targetY] = tile;
        grid[currentX, currentY] = null;

        tile.gridX = targetX;
        tile.gridY = targetY;
    }


    private bool IsValidTile(int x, int y){
        return IsTileInsideGrid(x,y) && grid[x, y] != null;
    }

    private bool IsTileInsideGrid(int x, int y){
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }


    private IEnumerator WaitForAll(params IEnumerator[] coroutines){
        List<Coroutine> runningCoroutines = new List<Coroutine>();

        // Start all coroutines and keep track of them
        foreach (var coroutine in coroutines)
        {
            runningCoroutines.Add(StartCoroutine(coroutine));
        }

        // Wait until all coroutines finish
        foreach (var coroutine in runningCoroutines)
        {
            yield return coroutine;
        }
    }


    //GOALS
    public void CountGoals(){
        numBox = 0;
        numStone = 0;
        numVase = 0;
        for (int y = 0; y < gridHeight; y++) // Iterate from top to bottom
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null && grid[x,y] is not Cube)
                {   
                    if(grid[x,y] is Box){
                        numBox++;
                    } else if(grid[x,y] is Stone){
                        numStone++;
                    } else if (grid[x,y] is Vase){
                        numVase++;
                    }
                }
            }
        }
    }

    public void ShowMoveandGoals(){
        
        CountGoals();
        // Find the GoalText and MoveText components inside uiTop
        TextMeshProUGUI moveText = uiTop.transform.Find("MoveBox")?.GetComponent<TextMeshProUGUI>();
        moveText.text = $"{moveCount}";
        //TODO:bunun yeri asla burası diil
        moveText.material.SetFloat("_UnderlayOffsetY", -2f);

        UpdateGoals();
    }

    public void UpdateGoals(){
        GameObject goalContainer = uiTop.transform.Find("GoalBox")?.gameObject;
        if (goalContainer == null) return;

        // Clear existing goals
        foreach (Transform child in goalContainer.transform)
        {
            Destroy(child.gameObject);
        }

       // Count goals first
        List<(Sprite, int)> goals = new List<(Sprite, int)>();
        if (numBox > 0) goals.Add((boxSprite, numBox));
        if (numStone > 0) goals.Add((stoneSprite, numStone));
        if (numVase > 0) goals.Add((vaseSprite, numVase));

        int goalCount = goals.Count;

        // Create goal items in correct positions
        for (int i = 0; i < goals.Count; i++)
        {
            CreateGoalItem(goals[i].Item1, goals[i].Item2, goalContainer, goalCount, i);
        }

        // If no goals, display a tick icon
        if (goalCount == 0)
        {
            CreateGoalItem(goalCheck, 0, goalContainer, 0, 0);
            levelDone = true;
        }



    }

    private void CreateGoalItem(Sprite icon, int count, GameObject goalContainer, int goalCount, int index){
        GameObject goalItem = Instantiate(goalItemPrefab, goalContainer.transform);
        Image iconImage = goalItem.transform.Find("Icon").GetComponent<Image>();
        TextMeshProUGUI countText = iconImage.transform.Find("Count").GetComponent<TextMeshProUGUI>();

        // Set icon and count
        iconImage.sprite = icon;
        countText.text = count.ToString();
        countText.fontWeight = FontWeight.Bold;

        RectTransform goalItemRect = goalItem.GetComponent<RectTransform>();
        RectTransform iconRect = iconImage.GetComponent<RectTransform>();

        // Adjust size based on goal count
        switch (goalCount)
        {
            case 3:
                goalItemRect.sizeDelta = new Vector2(90, 90);
                iconRect.sizeDelta = new Vector2(50, 50);
                countText.fontSize = 30;
                break;
            case 2:
                goalItemRect.sizeDelta = new Vector2(110, 110);
                iconRect.sizeDelta = new Vector2(65, 65);
                countText.fontSize = 35;
                break;
            case 1:
                goalItemRect.sizeDelta = new Vector2(140, 140);
                iconRect.sizeDelta = new Vector2(85, 85);
                countText.fontSize = 40;
                break;
            case 0:
                // Tick icon case
                goalItemRect.sizeDelta = new Vector2(80, 80);
                countText.gameObject.SetActive(false);
                return; // Skip positioning
        }


        // Manually adjust positioning for 3 goals (triangular layout)
        if (goalCount == 3)
        {
            if (index == 0) // First item (top-left)
                goalItemRect.anchoredPosition = new Vector2(-40, 35);
            else if (index == 1) // Second item (top-right)
                goalItemRect.anchoredPosition = new Vector2(40, 35);
            else if (index == 2) // Third item (bottom-center)
                goalItemRect.anchoredPosition = new Vector2(0, -40);
        }
        else if (goalCount == 2)
        {
            // Two items should be placed side-by-side
            if (index == 0) goalItemRect.anchoredPosition = new Vector2(-40, 0);
            if (index == 1) goalItemRect.anchoredPosition = new Vector2(40, 0);
        }
        else if (goalCount == 1)
        {
            // One item should be centered
            goalItemRect.anchoredPosition = new Vector2(0, 0);
        }
    }

    public IEnumerator ExplodeRocketCoroutine(Tile rocketTile) {
        if (rocketTile == null) yield break;
        rocketTile.isCurrentlyExploding = true;

        int x = rocketTile.gridX;
        int y = rocketTile.gridY;
        string rocketType = rocketTile.tileType;

        grid[x, y] = null;
        

        // Track explosion start
        rocketTile.GetComponent<Image>().enabled = false;
        yield return StartCoroutine(ExplosionRoutine(x, y, rocketType));
    }

    private IEnumerator ExplosionRoutine(int x, int y, string rocketType) {
        // Remove the original rocket tile
        grid[x, y] = null;
        yield return new WaitForSeconds(0.1f); // Simulate explosion effect

        List<Coroutine> subCoroutines = new List<Coroutine>();

        if (rocketType == "vro") {
            // Vertical Rocket -> Split into Top & Bottom
            subCoroutines.Add(StartCoroutine(MoveRocketPiece(x, y, 0, 1, verticalBottomRocketSprite)));
            subCoroutines.Add(StartCoroutine(MoveRocketPiece(x, y, 0, -1, verticalTopRocketSprite)));
        } else if (rocketType == "hro") {
            // Horizontal Rocket -> Split into Left & Right
            subCoroutines.Add(StartCoroutine(MoveRocketPiece(x, y, -1, 0, horizontalLeftRocketSprite)));
            subCoroutines.Add(StartCoroutine(MoveRocketPiece(x, y, 1, 0, horizontalRightRocketSprite)));
        }

        // Wait for all sub-coroutines to finish
        foreach (var coroutine in subCoroutines) {
            yield return coroutine;
        }
    }


    private IEnumerator MoveRocketPiece(int startX, int startY, int directionX, int directionY, Sprite sprite) {
        int x = startX;
        int y = startY;

        

        // Create the rocket piece once
        Vector3 startPosition = positionGrid[x, y];
        GameObject rocketObject = Instantiate(tilePrefab, gridContainer);
        RectTransform rectTransform = rocketObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = startPosition;
        rectTransform.sizeDelta = new Vector2(imageSize, imageSize);

        // Attach the tile logic
        Tile movingPiece = rocketObject.AddComponent<Rocket>();
        movingPiece.Initialize("rocketpart", sprite);
        movingPiece.SetGridManager(this);
        movingPiece.SetGridPosition(x, y);
        //AttachEffectsToTarget(movingPiece);
        uiAnimator.AnimateRocketMove(movingPiece, directionX, directionY);

        List<Coroutine> explosionCoroutines = new List<Coroutine>();

        // Move the rocket through the grid
        while (IsTileInsideGrid(x + directionX, y + directionY)) {
            x += directionX;
            y += directionY;


            Tile targetTile = grid[x, y];

            if (targetTile != null && targetTile.tileType != "rocketpart" && !targetTile.isCurrentlyExploding) {
                if (targetTile is Rocket) {
                    // Start explosion but do NOT wait (parallel execution)
                    explosionCoroutines.Add(StartCoroutine(ExplodeRocketCoroutine(targetTile)));
                } else {
                    // Destroy the tile it moves over
                    RemoveTiles(new List<Tile> { targetTile });
                }
            }

            // Move the same rocket piece to the new position
            Vector3 targetPosition = positionGrid[x, y];
            yield return StartCoroutine(MoveTileGeneric(movingPiece, targetPosition, 500f));
        }

        
        Destroy(rocketObject);
        foreach (Coroutine coroutine in explosionCoroutines) {
            yield return coroutine;  // Wait for each coroutine to finish
        }
    }


    public IEnumerator ExplodeRocketCrossCoroutine(Tile rocketTile) {
        if (rocketTile == null) yield break;
        rocketTile.isCurrentlyExploding = true;

        int x = rocketTile.gridX;
        int y = rocketTile.gridY;



        DestroySurroundingTiles(x, y);

        grid[x, y] = null;
        rocketTile.GetComponent<Image>().enabled = false;
        yield return StartCoroutine(ExplosionCrossRoutine(x, y));
    }

    private IEnumerator ExplosionCrossRoutine(int x, int y) {
        List<Coroutine> coroutines = new List<Coroutine>();

        // Execute vertical and horizontal explosions
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y, 0, 1, verticalBottomRocketSprite))); // Down
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y, 0, -1, verticalTopRocketSprite)));  // Up
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y, -1, 0, horizontalLeftRocketSprite))); // Left
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y, 1, 0, horizontalRightRocketSprite))); // Right
        coroutines.Add(StartCoroutine(MoveRocketPiece(x - 1, y, 0, 1, verticalBottomRocketSprite)));
        coroutines.Add(StartCoroutine(MoveRocketPiece(x - 1, y, 0, -1, verticalTopRocketSprite)));
        coroutines.Add(StartCoroutine(MoveRocketPiece(x + 1, y, 0, 1, verticalBottomRocketSprite)));
        coroutines.Add(StartCoroutine(MoveRocketPiece(x + 1, y, 0, -1, verticalTopRocketSprite)));
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y - 1, -1, 0, horizontalLeftRocketSprite)));
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y - 1, 1, 0, horizontalRightRocketSprite)));
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y + 1, -1, 0, horizontalLeftRocketSprite)));
        coroutines.Add(StartCoroutine(MoveRocketPiece(x, y + 1, 1, 0, horizontalRightRocketSprite)));

        // Wait for all coroutines to finish
        foreach (Coroutine coroutine in coroutines) {
            yield return coroutine;
        }
    }


    public void DestroySurroundingTiles(int x, int y){
        List<Tile> tilesToRemove = new List<Tile>();



        // Define the relative positions to destroy
        int[,] directions = {
            { 0, 1 },   // (x, y+1) - Above
            { 0, -1 },  // (x, y-1) - Below
            { -1, 0 },  // (x-1, y) - Left
            { 1, 0 }    // (x+1, y) - Right
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int targetX = x + directions[i, 0];
            int targetY = y + directions[i, 1];

            if (IsValidTile(targetX, targetY) && grid[targetX, targetY] != null) // Check if within bounds and not null
            {
                tilesToRemove.Add(grid[targetX, targetY]);
            }
        }

        // Call RemoveTiles with the collected tiles
        RemoveTiles(tilesToRemove);
    }

}