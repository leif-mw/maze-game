using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using TMPro;
public class Game : MonoBehaviour
{
    // Player and goal objects (and invisible object for camera to follow)
    [SerializeField]
    private GameObject player, goal, follow;

    // Prefabs
    [SerializeField]
    private GameObject floorPrefab, horizontalWallPrefab, verticalWallPrefab;

    // Transform for the current level
    [SerializeField]
    private Transform levelTransform;

    // Virtual camera
    [SerializeField]
    private CinemachineVirtualCamera vcam;

    [SerializeField]
    private TMP_Text levelText;

    // Controls the duration of the movement action
    [SerializeField]
    private float moveDuration = 0.15f, panDuration = 0.25f;

    // Number of current level
    public int levelNum = 0;

    // Maze dimensions
    private int rows = 3, cols = 3;

    // Object containing information for the current maze
    private Maze maze;
    
    // Player's row and column position in maze
    private Cell pos;

    // Array containing the floor objects of the current level
    private GameObject[,] floorGrid;

    // Records whether the player is currently moving
    private static bool isMoving, isZooming;

    // Anonymous function to check whether the player can successfully input an action at a given moment
    Func<bool> canAct = () => !isMoving && !isZooming; 

    // Start is called before the first frame update
    void Start()
    {
        NewLevel();
    }

    // Update is called once per frame
    void Update()
    {
        // Checks for level completion
        if(follow.transform.position == goal.transform.position)
            NewLevel();

        // Controls player movement
        if(Input.GetKey(KeyCode.W) && canAct())
        {
            if(MazeGenerator.CanMove(maze.grid, rows, cols, pos, new Cell { r = pos.r - 1, c = pos.c }))
            {
                pos.r -= 1;
                StartCoroutine(MovePlayer(Vector3.up, moveDuration));
            }
        }

        if(Input.GetKey(KeyCode.A) && canAct())
        {
            if(MazeGenerator.CanMove(maze.grid, rows, cols, pos, new Cell { r = pos.r, c = pos.c - 1 }))
            {
                pos.c -= 1;
                StartCoroutine(MovePlayer(Vector3.left, moveDuration));
            }
        }
        
        if(Input.GetKey(KeyCode.S) && canAct())
        {
            if(MazeGenerator.CanMove(maze.grid, rows, cols, pos, new Cell { r = pos.r + 1, c = pos.c }))
            {
                pos.r += 1;
                StartCoroutine(MovePlayer(Vector3.down, moveDuration));
            }
        }
        
        if(Input.GetKey(KeyCode.D) && canAct())
        {
            if(MazeGenerator.CanMove(maze.grid, rows, cols, pos, new Cell { r = pos.r, c = pos.c + 1 }))
            {
                pos.c += 1;
                StartCoroutine(MovePlayer(Vector3.right, moveDuration));
            }
        }

        // Controls the solution demo
        if(Input.GetKey(KeyCode.Space) && canAct())
        {
            StartCoroutine(DisplaySolution());
        }
    }

    // Creates and loads a new level (up until level 10 is completed)
    private void NewLevel()
    {
        // Clear previous maze
        foreach(Transform child in levelTransform)
            Destroy(child.gameObject);

        if(++levelNum < 11)
        {   
            // Update level text
            levelText.text = "Level " + levelNum;

            // Increment size and generate maze
            rows += 2;
            cols += 2;
            maze = MazeGenerator.Generate(rows, cols);

            // Initialize new array for the floor objects
            floorGrid = new GameObject[rows, cols];

            // Update maze position
            pos = new Cell { r = maze.start.r, c = maze.start.c };

            // Update player object position
            follow.transform.position = new Vector3(maze.start.c, -maze.start.r);
            player.transform.position = new Vector3(maze.start.c, -maze.start.r);

            // Pause trail renderer briefly 
            player.GetComponent<TrailRenderer>().Clear();

            // Update goal object position
            goal.transform.position = new Vector3(maze.goal.c, -maze.goal.r);

            // Display maze and update camera
            DisplayMaze();
            vcam.m_Lens.OrthographicSize = Mathf.Pow(cols / 3 + rows / 2, 0.7f) + 1;
        }

        else
        {
            SceneManager.LoadScene("EndScreen");
        }
    }

    // Renders the maze
    private void DisplayMaze()
    {
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                Vector3 currPos = new Vector3(j, -i);
                floorGrid[i, j] = Instantiate(floorPrefab, currPos, Quaternion.identity, levelTransform);

                if(MazeGenerator.HasState(maze.grid[i, j], WallState.Down))
                {
                    GameObject downWall = Instantiate(horizontalWallPrefab, new Vector3(currPos.x, currPos.y - 0.5f), Quaternion.identity, floorGrid[i, j].transform);
                }

                if(MazeGenerator.HasState(maze.grid[i, j], WallState.Left))
                {
                    GameObject leftWall = Instantiate(verticalWallPrefab, new Vector3(currPos.x - 0.5f, currPos.y), Quaternion.identity, floorGrid[i, j].transform);
                }
            }
        }
    }

    // Coroutine that moves the player in the specified direction over the specified duration
    private IEnumerator MovePlayer(Vector3 direction, float duration)
    {
        isMoving = true;

        float elapsedTime = 0f;
        Vector3 originalPos = follow.transform.position;
        Vector3 targetPos = originalPos + direction;

        while(elapsedTime < duration)
        {
            Vector3 lerpPos = Vector3.Lerp(originalPos, targetPos, elapsedTime / duration);
            follow.transform.position = lerpPos;
            player.transform.position = lerpPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        follow.transform.position = targetPos;
        player.transform.position = targetPos;

        isMoving = false;
    }

    // Coroutine that visualizes the shortest path to the maze's solution from the specified position
    private IEnumerator DisplaySolution()
    {
        isMoving = true;

        StartCoroutine(PlayerToSolution(panDuration));

        Traversal traversal = MazePathfinder.Solve(maze, rows, cols, pos);
        List<Cell> seen = traversal.seen;
        List<Cell> shortest = traversal.shortest;
        float waitSeconds = 0.01f;

        yield return new WaitForSeconds(1f);

        // Visualizing cells seen
        foreach(Cell cell in seen)
        {
            floorGrid[cell.r, cell.c].GetComponent<SpriteRenderer>().color = UnityEngine.Color.yellow;
            yield return new WaitForSeconds(waitSeconds);
        }

        // Visualizing shortest path
        foreach(Cell cell in shortest)
        {
            floorGrid[cell.r, cell.c].GetComponent<SpriteRenderer>().color = UnityEngine.Color.green;
        }

        yield return new WaitForSeconds(1.5f);

        ResetColor(seen);
        StartCoroutine(SolutionToPlayer(panDuration));

        isMoving = false;
    }

    // Changes the virtual camera from player view to solution view
    private IEnumerator PlayerToSolution(float duration)
    {
        isZooming = true;

        float elapsedTime = 0f;
        Vector3 originalPos = follow.transform.position;
        Vector3 targetPos = new Vector3(cols / 2, -rows / 2, -10);
        float originalOrthographicSize = vcam.m_Lens.OrthographicSize;
        float targetOrthographicSize = rows / 2 + 2;

        while(elapsedTime < duration)
        {
            float smoothStep = Mathf.SmoothStep(0, 1, elapsedTime / duration);
            follow.transform.position = Vector3.Lerp(originalPos, targetPos, Mathf.SmoothStep(0, 1, smoothStep));
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(originalOrthographicSize, targetOrthographicSize, smoothStep);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        follow.transform.position = targetPos;
        vcam.m_Lens.OrthographicSize = rows / 2 + 2;

        isZooming = false;
    }

    // Changes the virtual camera from solution view to player view
    private IEnumerator SolutionToPlayer(float duration)
    {
        isZooming = true;

        float elapsedTime = 0f;
        Vector3 originalPos = follow.transform.position;
        Vector3 targetPos = player.transform.position;
        float originalOrthographicSize = vcam.m_Lens.OrthographicSize;
        float targetOrthographicSize = Mathf.Pow(cols / 3 + rows / 2, 0.7f) + 1;

        while(elapsedTime < duration)
        {
            float smoothStep = Mathf.SmoothStep(0, 1, elapsedTime / duration);
            follow.transform.position = Vector3.Lerp(originalPos, targetPos, smoothStep);
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(originalOrthographicSize, targetOrthographicSize, smoothStep);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        follow.transform.position = targetPos;
        vcam.m_Lens.OrthographicSize = targetOrthographicSize;

        isZooming = false;
    }

    // Resets any color changes made by displaying the solution
    private void ResetColor(List<Cell> cellsToReset)
    {
        foreach(Cell cell in cellsToReset)
        {
            floorGrid[cell.r, cell.c].GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
