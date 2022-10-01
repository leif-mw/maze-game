using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls the selection of algorithm used for maze generation
public enum PathfindingAlgorithm { BreadthFirstSearch, AStar }

// Contains information for all of the cells seen in a traversal and for the shortest path in a maze
public class Traversal
{
    public List<Cell> seen;
    public List<Cell> shortest;

    public Traversal(List<Cell> seen, List<Cell> shortest)
    {
        this.seen = seen;
        this.shortest = shortest;
    }
}

public static class MazePathfinder
{
    // Returns a list containing the cells seen while finding the shortest path to the maze goal using the selected algorithm (uses BFS by default)
    public static Traversal Solve(Maze maze, int rows, int cols, Cell start, PathfindingAlgorithm alg = PathfindingAlgorithm.BreadthFirstSearch)
    {  
        switch(alg)
        {
            case PathfindingAlgorithm.BreadthFirstSearch: 
                return BreadthFirstSearch(maze, rows, cols, start);
            case PathfindingAlgorithm.AStar:
                return AStar(maze, rows, cols, start);
            default:
                return BreadthFirstSearch(maze, rows, cols, start);
        }
    }

    // Returns a list containing only the cells in the shortest path obtained using backtracking
    public static List<Cell> ShortestPath(Maze maze, Cell start, Dictionary<Cell, Cell> parentMap)
    {
        List<Cell> backtrackPath = new List<Cell>();
        Cell curr = maze.goal;

        while(curr.r != start.r || curr.c != start.c)
        {
            backtrackPath.Add(curr);
            curr = parentMap[curr];
        }

        backtrackPath.Add(curr);

        return backtrackPath;
    }

    // An implementation of BFS to find the shortest solution to a maze
    private static Traversal BreadthFirstSearch(Maze maze, int rows, int cols, Cell start)
    {
        List<Cell> allSeen = new List<Cell> { start };
        Dictionary<Cell, Cell> parentMap = new Dictionary<Cell, Cell>();
        Queue<Cell> queue = new Queue<Cell>();
        HashSet<Cell> seen = new HashSet<Cell> { start };

        queue.Enqueue(start);

        while(queue.Count > 0)
        {
            Cell curr = queue.Dequeue();

            Cell upNeighbor = new Cell {r = curr.r - 1, c = curr.c};
            Cell rightNeighbor = new Cell { r = curr.r, c = curr.c + 1 };
            Cell downNeighbor = new Cell { r = curr.r + 1, c = curr.c };
            Cell leftNeighbor = new Cell { r = curr.r, c = curr.c - 1 };

            if(curr.r == maze.goal.r && curr.c == maze.goal.c)
                break;

            // Up
            if(MazeGenerator.CanMove(maze.grid, rows, cols, curr, upNeighbor) && !seen.Contains(upNeighbor))
            {
                queue.Enqueue(upNeighbor);
                seen.Add(upNeighbor);
                parentMap.Add(upNeighbor, curr);
            }

            // Right
            if(MazeGenerator.CanMove(maze.grid, rows, cols, curr, rightNeighbor) && !seen.Contains(rightNeighbor))
            {
                queue.Enqueue(rightNeighbor);
                seen.Add(rightNeighbor);
                parentMap.Add(rightNeighbor, curr);
            }

            // Down
            if(MazeGenerator.CanMove(maze.grid, rows, cols, curr, downNeighbor) && !seen.Contains(downNeighbor))
            {
                queue.Enqueue(downNeighbor);
                seen.Add(downNeighbor);
                parentMap.Add(downNeighbor, curr);
            }

            // Left
            if(MazeGenerator.CanMove(maze.grid, rows, cols, curr, leftNeighbor) && !seen.Contains(leftNeighbor))
            {
                queue.Enqueue(leftNeighbor);
                seen.Add(leftNeighbor);
                parentMap.Add(leftNeighbor, curr);
            }
        }

        allSeen.AddRange(parentMap.Keys);

        return new Traversal(allSeen, ShortestPath(maze, start, parentMap));
    }

    // TODO: An implementation of A* to find the shortest solution to a maze 
    private static Traversal AStar(Maze maze, int rows, int cols, Cell start)
    {
        Dictionary<Cell, Cell> parentMap = new Dictionary<Cell, Cell>();
        return new Traversal(new List<Cell>(parentMap.Keys), ShortestPath(maze, start, parentMap));
    }
}
