using System.Collections.Generic;
using UnityEngine;

// Controls the selection of algorithm used for maze generation
public enum GenerationAlgorithm { DepthFirstSearch, RandomizedKruskals, RandomizedPrims }

// Defines the state of a cell in a maze
public enum WallState { Up = 1, Right = 2, Down = 4, Left = 8, Visited = 16 }

// Defines a maze cell positioned at row r and column c
public struct Cell 
{
    public int r, c; 
}

// Contains information for a maze to be passed to the main Game script
public class Maze
{
    public WallState[,] grid;
    public Cell start, goal;

    public Maze(WallState[,] grid, Cell start, Cell goal)
    {
        this.grid = grid;
        this.start = start;
        this.goal = goal;
    }
}

public static class MazeGenerator
{
    private static readonly System.Random rand = new System.Random();

    // Randomly generates a new maze using the selected algorithm (uses DFS by default)
    public static Maze Generate(int rows, int cols, GenerationAlgorithm alg = GenerationAlgorithm.DepthFirstSearch)
    {
        WallState[,] maze = InitMaze(cols, rows);

        switch(alg)
        {
            case GenerationAlgorithm.DepthFirstSearch: return DepthFirstSearch(maze, cols, rows);
            case GenerationAlgorithm.RandomizedKruskals: return RandomizedKruskals(maze, cols, rows);
            case GenerationAlgorithm.RandomizedPrims: return RandomizedPrims(maze, cols, rows);
            default: return DepthFirstSearch(maze, cols, rows);
        }
    }

    // Returns true if a WallState has the specified state, false otherwise
    public static bool HasState(WallState wallState, WallState state)
    {
        return (wallState & state) != 0;
    }

    // Returns true if the player can move from cell a to cell b in the maze, false otherwise (assumes a != b)
    public static bool CanMove(WallState[,] maze, int rows, int cols, Cell a, Cell b)
    {
        int rowDiff = a.r - b.r;

        // a is below b
        if(rowDiff == 1)
            return b.r >= 0 && !HasState(maze[b.r, b.c], WallState.Down);

        // a is above b
        else if(rowDiff == -1)
            return b.r < rows && !HasState(maze[a.r, a.c], WallState.Down);

        else
        {
            int colDiff = a.c - b.c;

            // a is right of b
            if(colDiff == 1)
                return b.c >= 0 && !HasState(maze[a.r, a.c], WallState.Left);

            // a is left of b
            else
                return b.c < cols && !HasState(maze[b.r, b.c], WallState.Left); 
        }
    }

    // Helper function to initialize a maze
    private static WallState[,] InitMaze(int rows, int cols)
    {
        WallState initState = WallState.Up | WallState.Right | WallState.Down | WallState.Left;
        WallState[,] maze = new WallState[rows, cols];

        for(int i = 0; i < rows; i++)
            for(int j = 0; j < cols; j++)
                maze[i, j] = initState;

        return maze;
    }

    // An randomized implementation of DFS with backtracking for maze generation
    private static Maze DepthFirstSearch(WallState[,] maze, int rows, int cols)
    {
        Cell start = new Cell { r = rand.Next(0, rows), c = rand.Next(0, cols) };
        Cell goal = new Cell { r = -1, c = -1 };
        maze[start.r, start.c] |= WallState.Visited;

        Stack<Cell> stack = new Stack<Cell>();
        stack.Push(start);

        while(stack.Count > 0)
        {
            Cell curr = stack.Pop();
            Cell next = GetRandomNeighbor(maze, rows, cols, curr);

            if(next.r != -1)
            {
                stack.Push(curr);
                RemoveWalls(maze, curr, next);
                maze[next.r, next.c] |= WallState.Visited;
                stack.Push(next);
            }
        }
        
        goal = FurthestCell(maze, rows, cols, start);

        return new Maze(maze, start, goal);
    }

    // TODO: A randomized implementation of Kruskal's algorithm for maze generation
    private static Maze RandomizedKruskals(WallState[,] maze, int rows, int cols)
    {
        return new Maze(maze, new Cell {}, new Cell {});
    }

    // TODO: A randomized implementation of Prim's algorithm for maze generation
    private static Maze RandomizedPrims(WallState[,] maze, int rows, int cols)
    {
        return new Maze(maze, new Cell {}, new Cell {});
    }

    // Finds the furthest cell from a given cell using BFS (can be used to generate harder mazes)
    private static Cell FurthestCell(WallState[,] maze, int rows, int cols, Cell cell)
    {
        Queue<Cell> queue = new Queue<Cell>();
        HashSet<Cell> seen = new HashSet<Cell> { cell };

        queue.Enqueue(cell);
        Cell curr = cell;

        while(queue.Count > 0)
        {
            curr = queue.Dequeue();

            Cell upNeighbor = new Cell {r = curr.r - 1, c = curr.c};
            Cell rightNeighbor = new Cell { r = curr.r, c = curr.c + 1 };
            Cell downNeighbor = new Cell { r = curr.r + 1, c = curr.c };
            Cell leftNeighbor = new Cell { r = curr.r, c = curr.c - 1 };

            // Up
            if(CanMove(maze, rows, cols, curr, upNeighbor) && !seen.Contains(upNeighbor))
            {
                queue.Enqueue(upNeighbor);
                seen.Add(upNeighbor);
            }

            // Right
            if(CanMove(maze, rows, cols, curr, rightNeighbor) && !seen.Contains(rightNeighbor))
            {
                queue.Enqueue(rightNeighbor);
                seen.Add(rightNeighbor);
            }

            // Down
            if(CanMove(maze, rows, cols, curr, downNeighbor) && !seen.Contains(downNeighbor))
            {
                queue.Enqueue(downNeighbor);
                seen.Add(downNeighbor);
            }

            // Left
            if(MazeGenerator.CanMove(maze, rows, cols, curr, leftNeighbor) && !seen.Contains(leftNeighbor))
            {
                queue.Enqueue(leftNeighbor);
                seen.Add(leftNeighbor);
            }
        }

        return curr;
    }

    // Returns a random unvisited neighbor of a given cell if it exists, else returns a cell with x = -1 and y = -1
    private static Cell GetRandomNeighbor(WallState[,] maze, int rows, int cols, Cell cell)
    {
        List<Cell> unvisitedNeighbors = new List<Cell>();
        
        // Up
        if(cell.r > 0 && !HasState(maze[cell.r - 1, cell.c], WallState.Visited))
            unvisitedNeighbors.Add(new Cell { r = cell.r - 1, c = cell.c });

        // Right
        if(cell.c < cols - 1 && !HasState(maze[cell.r, cell.c + 1], WallState.Visited))
            unvisitedNeighbors.Add(new Cell { r = cell.r, c = cell.c + 1 });

        // Down
        if(cell.r < rows - 1 && !HasState(maze[cell.r + 1, cell.c], WallState.Visited))
            unvisitedNeighbors.Add(new Cell { r = cell.r + 1, c = cell.c });

        // Left
        if(cell.c > 0 && !HasState(maze[cell.r, cell.c - 1], WallState.Visited))
            unvisitedNeighbors.Add(new Cell { r = cell.r, c = cell.c - 1 });

        return unvisitedNeighbors.Count > 0 ? unvisitedNeighbors[rand.Next(0, unvisitedNeighbors.Count)] : new Cell { r = -1, c = -1};
    }

    // Removes the walls between cell a and cell b in a maze (assumes that both cells are valid and neighbors)
    private static void RemoveWalls(WallState[,] maze, Cell a, Cell b)
    {
        int rowDiff = a.r - b.r;

        // a is below b
        if(rowDiff == 1)
        {
            maze[a.r, a.c] &= ~WallState.Up;
            maze[b.r, b.c] &= ~WallState.Down;
        }

        // a is above b
        else if(rowDiff == -1)
        {
            maze[a.r, a.c] &= ~WallState.Down;
            maze[b.r, b.c] &= ~WallState.Up;
        }

        else
        {
            int colDiff = a.c - b.c;

            // a is right of b
            if(colDiff == 1)
            {
                maze[a.r, a.c] &= ~WallState.Left;
                maze[b.r, b.c] &= ~WallState.Right;
            }

            // a is left of b
            else
            {
                maze[a.r, a.c] &= ~WallState.Right;
                maze[b.r, b.c] &= ~WallState.Left;
            }
        }
    }

    // Finds the distance from cell a to cell b
    private static float GetDistance(Cell a, Cell b)
    {
        int rowDiff = a.r - b.r;
        int colDiff = a.c - b.c;
        return Mathf.Sqrt(rowDiff * rowDiff + colDiff * colDiff);
    }
}
