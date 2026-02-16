using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Splines.ExtrusionShapes;

public class KnightsBoardManager : MonoBehaviour
{
    public int height, width;
    public static KnightsBoardManager instance;
    public Dictionary<string, KnightsSquareScript> squares = new Dictionary<string, KnightsSquareScript>();
    public Dictionary<string, KnightsSquareScript> outsideSquares = new Dictionary<string, KnightsSquareScript>();
    public List<KnightBehavior> knightList = new List<KnightBehavior>();
    public List<KnightBehavior> deadKnightList;

    public bool player1StartZoneActive = true, player2StartZoneActive = true;
    public List<KnightsSquareScript> player1StartZone;
    public List<KnightsSquareScript> player2StartZone;

    public List<KnightsSquareScript> fragileFloorStartPlayer1 = new List<KnightsSquareScript>();
    public List<KnightsSquareScript> fragileFloorStartPlayer2 = new List<KnightsSquareScript>();

    public List<KnightsSquareScript> lavaStartSquaresPlayer1 = new List<KnightsSquareScript>();
    public List<KnightsSquareScript> lavaStartSquaresPlayer2 = new List<KnightsSquareScript>();

    public List<RockObstacleScript> obstacles = new List<RockObstacleScript>();
    public List<WaterCourse> waterCourses = new List<WaterCourse>();
    public List<KnightsSquareScript> lavaSquares = new List<KnightsSquareScript>();
    public List<KnightsSquareScript> fragileFloor = new List<KnightsSquareScript>();

    public GameObject whiteSquarePrefab;
    public GameObject blackSquarePrefab;
    public GameObject boardPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public KnightsSquareScript GetSquare(string squareName)
    {
        if (squares.TryGetValue(squareName, out KnightsSquareScript square))
            return square;
        return null;
    }

    public KnightsSquareScript GetOutsideSquare(string squareName)
    {
        if (outsideSquares.TryGetValue(squareName, out KnightsSquareScript square))
            return square;
        return null;
    }
    public void GenerateBoard()
    {
        squares.Clear();

        Transform squaresRoot = new GameObject("ChessSquares").transform;
        squaresRoot.SetParent(transform);

        if (boardPrefab != null)
        {
            Vector3 boardSize = new Vector3(height + 0.5f, 1, width + 0.5f);
            GameObject board = Instantiate(boardPrefab);
            board.transform.localScale = boardSize;
            board.transform.position = new Vector3(0, -0.25f, 0);
            board.name = "Board";
            squaresRoot.SetParent(transform);
        }

        Transform obstacles = new GameObject("Obstacles").transform;
        obstacles.SetParent(transform);

        for (int row = 1; row <= height; row++)
        {
            Transform rowParent = new GameObject("Row" + row).transform;
            rowParent.SetParent(squaresRoot);

            for (int colIndex = 0; colIndex < width; colIndex++)
            {
                bool isWhite = (row + colIndex) % 2 == 0;
                GameObject prefab = isWhite ? whiteSquarePrefab : blackSquarePrefab;

                GameObject squareObj = Instantiate(prefab, rowParent);

                float x = height / 2f - 0.5f - (row - 1);
                float z = -(width / 2f - 0.5f) + colIndex;

                squareObj.transform.position = new Vector3(x, 0, z);

                KnightsSquareScript sq = squareObj.GetComponent<KnightsSquareScript>();
                if (sq == null)
                    sq = squareObj.AddComponent<KnightsSquareScript>();

                sq.SquareColumn = (char)('A' + colIndex);
                sq.SquareRow = row;
                sq.empty = true;
                sq.knight = null;

                sq.name = sq.SquareColumn.ToString() + sq.SquareRow;
                sq.knightPosition = new Vector3(sq.transform.position.x, sq.transform.position.y + 0.85f, sq.transform.position.z);

                squares.Add(sq.name, sq);
                //StartCoroutine(sq.InitializeSquare());
            }
        }

        outsideSquares.Clear();

        Transform outsideRoot = new GameObject("OutsideSquares").transform;
        outsideRoot.SetParent(transform);

        Transform upParent = new GameObject("Up").transform;
        upParent.SetParent(outsideRoot);

        for (int colIndex = 0; colIndex < width; colIndex++)
        {
            for (int i = 0; i < 2; i++)
            {
                int row = height + 1 + i;

                CreateOutsideSquare(row, colIndex, upParent);
            }
        }


        Transform downParent = new GameObject("Down").transform;
        downParent.SetParent(outsideRoot);

        for (int colIndex = 0; colIndex < width; colIndex++)
        {
            for (int i = 0; i < 2; i++)
            {
                int row = -i;

                CreateOutsideSquare(row, colIndex, downParent);
            }
        }


        Transform leftParent = new GameObject("Left").transform;
        leftParent.SetParent(outsideRoot);

        for (int row = 1; row <= height; row++)
        {
            for (int i = 0; i < 2; i++)
            {
                int colIndex = -i;

                CreateOutsideSquare(row, colIndex, leftParent);
            }
        }


        Transform rightParent = new GameObject("Right").transform;
        rightParent.SetParent(outsideRoot);

        for (int row = 1; row <= height; row++)
        {
            for (int i = 0; i < 2; i++)
            {
                int colIndex = width + i;

                CreateOutsideSquare(row, colIndex, rightParent);
            }
        }

    }
    public void TestStartZone()
    {
        int spawned = 0;

        List<KnightsSquareScript> orderedSquares = new List<KnightsSquareScript>(player1StartZone);
        orderedSquares.Sort((a, b) =>
        {
            int rowCompare = a.SquareRow.CompareTo(b.SquareRow);
            if (rowCompare != 0)
                return rowCompare;

            return a.SquareColumn.CompareTo(b.SquareColumn);
        });

        foreach (KnightsSquareScript sq in orderedSquares)
        {
            if (spawned >= 3)
                break;

            if (!sq.empty)
                continue;

            GameObject randKnight = UnityEngine.Random.Range(0, 2) == 0 ? KnightsGameManager.instance.agileKnightPrefab : KnightsGameManager.instance.tucutuKnightPrefab;

            GameObject knightObj = Instantiate(randKnight);
            KnightBehavior knight = knightObj.GetComponent<KnightBehavior>();
            knightList.Add(knight);

            knight.GetComponent<MeshRenderer>().material.color = Color.skyBlue;
            knight.player = 1;
            knight.currentSquare = sq;
            knight.transform.position = sq.knightPosition;

            sq.knight = knight;
            sq.empty = false;

            spawned++;
        }

        spawned = 0;
        orderedSquares.Clear();

        orderedSquares = new List<KnightsSquareScript>(player2StartZone);
        orderedSquares.Sort((a, b) =>
        {
            int rowCompare = a.SquareRow.CompareTo(b.SquareRow);
            if (rowCompare != 0)
                return rowCompare;

            return a.SquareColumn.CompareTo(b.SquareColumn);
        });

        foreach (KnightsSquareScript sq in orderedSquares)
        {
            if (spawned >= 3)
                break;

            if (!sq.empty)
                continue;

            GameObject randKnight = UnityEngine.Random.Range(0, 2) == 0 ? KnightsGameManager.instance.agileKnightPrefab : KnightsGameManager.instance.tucutuKnightPrefab;

            GameObject knightObj = Instantiate(randKnight);
            KnightBehavior knight = knightObj.GetComponent<KnightBehavior>();
            knightList.Add(knight);

            knight.GetComponent<MeshRenderer>().material.color = Color.red;
            knight.player = 2;
            knight.currentSquare = sq;
            knight.transform.position = sq.knightPosition;

            sq.knight = knight;
            sq.empty = false;

            spawned++;
        }

        KnightsGameManager.instance.ConfirmStartPosition();
    }

    public void SpawnKnights(int spawnedKnights)
    {
        int spawned = 0;

        // Ordenar las casillas por fila y columna
        List<KnightsSquareScript> orderedSquares = new List<KnightsSquareScript>(squares.Values);
        orderedSquares.Sort((a, b) =>
        {
            int rowCompare = a.SquareRow.CompareTo(b.SquareRow);
            if (rowCompare != 0)
                return rowCompare;

            return a.SquareColumn.CompareTo(b.SquareColumn);
        });

        foreach (KnightsSquareScript square in orderedSquares)
        {
            if (spawned >= spawnedKnights)
                break;

            if (!square.empty)
                continue;

            GameObject knightObj = Instantiate(KnightsGameManager.instance.agileKnightPrefab);
            KnightBehavior knight = knightObj.GetComponent<KnightBehavior>();
            knightList.Add(knight);

            knight.player = 1;
            knight.currentSquare = square;
            knight.transform.position = square.knightPosition;

            square.knight = knight;
            square.empty = false;

            spawned++;
        }
    }

    public void Test()
    {
        int midRow = (height + 1) / 2;
        int midColIndex = (width - 1) / 2;
        char midColumn = (char)('A' + midColIndex);

        Vector2Int[][] LPaths = new Vector2Int[][]
        {
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(2,1) },
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(2,-1) },

        new[] { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-2,0), new Vector2Int(-2,1) },
        new[] { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-2,0), new Vector2Int(-2,-1) },

        new[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,2) },
        new[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(-1,2) },

        new[] { new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2), new Vector2Int(1,-2) },
        new[] { new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        };

        Vector2Int[] chosenPath = null;

        foreach (var path in LPaths)
        {
            bool valid = true;

            foreach (var offset in path)
            {
                char c = (char)(midColumn + offset.x);
                int r = midRow + offset.y;

                if (!squares.TryGetValue(c.ToString() + r, out var sq) || !sq.empty)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            Vector2Int upOffset = path[3] + Vector2Int.up;
            char uc = (char)(midColumn + upOffset.x);
            int ur = midRow + upOffset.y;

            if (!squares.TryGetValue(uc.ToString() + ur, out var upSq) || !upSq.empty)
                continue;

            chosenPath = path;
            break;
        }

        if (chosenPath == null)
            return;

        for (int i = 0; i < 3; i++)
        {
            char c = (char)(midColumn + chosenPath[i].x);
            int r = midRow + chosenPath[i].y;

            KnightsSquareScript sq = squares[c.ToString() + r];

            GameObject obj = Instantiate(KnightsGameManager.instance.agileKnightPrefab);
            KnightBehavior knight = obj.GetComponent<KnightBehavior>();

            knight.player = 1;
            knight.currentSquare = sq;
            knight.transform.position = sq.knightPosition;
            knight.GetComponent<MeshRenderer>().material.color = Color.cyan;

            sq.knight = knight;
            sq.empty = false;

            knightList.Add(knight);
        }

        Vector2Int destOffset = chosenPath[3];
        char dc = (char)(midColumn + destOffset.x);
        int dr = midRow + destOffset.y;

        KnightsSquareScript destSq = squares[dc.ToString() + dr];

        {
            GameObject obj = Instantiate(KnightsGameManager.instance.tucutuKnightPrefab);
            KnightBehavior knight = obj.GetComponent<KnightBehavior>();
            knightList.Add(knight);

            knight.player = 2;
            knight.currentSquare = destSq;
            knight.transform.position = destSq.knightPosition;
            knight.GetComponent<MeshRenderer>().material.color = Color.red;

            destSq.knight = knight;
            destSq.empty = false;
        }

        Vector2Int upOffsetFinal = destOffset + Vector2Int.up;
        char ucFinal = (char)(midColumn + upOffsetFinal.x);
        int urFinal = midRow + upOffsetFinal.y;

        KnightsSquareScript upSqFinal = squares[ucFinal.ToString() + urFinal];

        {
            GameObject obj = Instantiate(KnightsGameManager.instance.tucutuKnightPrefab);
            KnightBehavior knight = obj.GetComponent<KnightBehavior>();

            knight.player = 2;
            knight.currentSquare = upSqFinal;
            knight.transform.position = upSqFinal.knightPosition;
            knight.GetComponent<MeshRenderer>().material.color = Color.red;

            upSqFinal.knight = knight;
            upSqFinal.empty = false;

            knightList.Add(knight);
        }
    }

    public void CheckStartZone(int player)
    {
        bool startZoneIsEmpty = true;
        List<KnightsSquareScript> startZone;
        List<KnightsSquareScript> fragileFloor;
        if (player == 1)
        {
            fragileFloor = fragileFloorStartPlayer1;
            startZone = player1StartZone;
        }
        else
        {
            fragileFloor = fragileFloorStartPlayer2;
            startZone = player2StartZone;
        }

        foreach (KnightsSquareScript sq in startZone)
        {
            if (!sq.empty)
            {
                startZoneIsEmpty = false;
                break;
            }
        }
        if (startZoneIsEmpty)
        {
            if (player == 1)
                player1StartZoneActive = false;
            else
                player2StartZoneActive = false;

            foreach (KnightsSquareScript sq in fragileFloor)
            {
                sq.TurnVoid(true);
            }
        }
    }

    public void SetObstacles()
    {
        foreach (RockObstacleScript rock in obstacles)
        {
            rock.SetDangerousSquares();
        }

        List<KnightsSquareScript> squaresDone = new List<KnightsSquareScript>();
        foreach (WaterCourse water in waterCourses)
        {
            foreach (KnightsSquareScript sq in water.waterCourseSquares)
            {
                sq.isWaterSquare = true;
                if (!squaresDone.Contains(sq))
                {
                    sq.waterCourseDirection = water.courseDirection;
                    squaresDone.Add(sq);
                }
                else
                {
                    sq.isWaterCourseCrossing = true;
                    sq.waterCourseDirection = Vector2Int.zero;
                }
            }
        }
    }
    void CreateOutsideSquare(int row, int colIndex, Transform parent)
    {
        GameObject squareObj = new GameObject("OutsideSquare");
        squareObj.transform.SetParent(parent);

        float x = height / 2f - 0.5f - (row - 1);
        float z = -(width / 2f - 0.5f) + colIndex;

        squareObj.transform.position = new Vector3(x, 0, z);

        KnightsSquareScript sq = squareObj.AddComponent<KnightsSquareScript>();

        sq.SquareColumn = (char)('A' + colIndex);
        sq.SquareRow = row;
        sq.empty = true;

        string key = $"OUT_{sq.SquareColumn}{sq.SquareRow}";
        sq.name = key;

        outsideSquares.Add(key, sq);
    }

}