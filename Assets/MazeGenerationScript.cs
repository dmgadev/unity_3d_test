using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerationScript : MonoBehaviour
{
    public GameObject cubeWall;
    public Quaternion rotation;
    static public float spawnCoordsX = 1000; // -115
    static public float spawnCoordsY = 1000; // 15

    [HideInInspector]
    public bool mazeGenerated = false;

    public GameObject cubeWay;

    public Stack<Vector2> wayPointsCopied; // в этот стек запишем Vector2 координаты точек найденного пути в массиве 

    void Start()
    {
        rotation = new Quaternion(45, 0, 0, 0);

        Maze newMaze = new Maze();
        newMaze.Initialize(25, 25, cubeWall, cubeWay);

        wayPointsCopied = newMaze.wayPoints;
        mazeGenerated = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private class Maze
    {
        private GameObject cubeWallRef;
        private Quaternion rotationRef = new Quaternion(0, 0, 0, 0);
        private GameObject cubeWayRef;

        private int height;
        private int width;
        string[,] maze; //создаем матрицу - двумерный массив
        GameObject[,] mazeCubes;

        Vector2 currentCell; // записывавем сюда

        public Stack<Vector2> wayPoints = new Stack<Vector2>(); // в этот стек запишем Vector2 координаты точек найденного пути в массиве 

        public void Initialize(int a, int b, GameObject c, GameObject d)
        {
            this.height = a + 4; // упростим задачу при написании алгоритма - обернем прямоугольник лабиринта слоем "посещенных" клеток
            this.width = b + 4;   // upd: возникли трудности, поэтому обернем двумя слоями - потом переделаю, надо сначала заставить тестовый вариант работать
            this.cubeWallRef = c;
            this.cubeWayRef = d;

            this.currentCell = new Vector2(3, 3);

            maze = new string[height, width];

            mazeCubes = new GameObject[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    maze[i, j] = "visited";
                    mazeCubes[i, j] = null;
                }
            }

            GenerateMatrix();

            // сгенерировали исходную матрицу, теперь необходимо из "входа" в лабиринт начать обходить соседние клетки через 1 по прямой вертикально и горизонтально

            RecursionToDeleteWalls(currentCell);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    maze[i, j] = "dio";
                }
            }
            currentCell = new Vector2(3, 3);

            RecursionToShowTheWay(currentCell);
        }
        public void GenerateMatrix()
        {
            for (int i = 2; i < height - 2; i++)
            {
                for (int j = 2; j < width - 2; j++)
                {
                    if ((i % 2 != 0 && j % 2 != 0) && (i < height - 1 && j < width - 1))   // если ячейка нечетная по x и y, и при этом находится в пределах стен лабиринта
                    {
                        maze[i, j] = "notvisited";      // то это КЛЕТКА
                    }
                    else
                    {
                        maze[i, j] = "wall";            // в остальных случаях это СТЕНА.
                        mazeCubes[i, j] = Instantiate(cubeWallRef, new Vector3(spawnCoordsX + i * 5f, 0, spawnCoordsY + j * 5f), rotationRef);
                    }
                }
            }
        }

        private void RecursionToDeleteWalls(Vector2 curCell) // подаем выбранную текущую клетку(для нулевого вызова это будет старт, для последующих - 1 из непосещенных клеток)
        {
            maze[(int) curCell.x, (int) curCell.y] = "visited"; // как только зашли в функцию - помечаем текущую клетку как помеченную, чтобы не возвращаться в нее

            Vector2[] coordsOf4Cells = new Vector2[4]; // для каждого вызова рекурсии задаем 
            string[] contentOf4Cells = new string[4];

            int writingRandomGenerator; // так как длина массивов выше = 4 // переменная для генерации порядка заполнения массива
            int quanOfFilled = 0;
            
            Vector2 buffForCalculations = curCell;

            Stack<Vector2> neighbourCellsCoords = new Stack<Vector2>();

            buffForCalculations.x -= 2; 
            neighbourCellsCoords.Push(buffForCalculations);
            buffForCalculations.x += 2;
            buffForCalculations.y += 2;
            neighbourCellsCoords.Push(buffForCalculations);
            buffForCalculations.x += 2;
            buffForCalculations.y -= 2;
            neighbourCellsCoords.Push(buffForCalculations);
            buffForCalculations.x -= 2;
            buffForCalculations.y -= 2;
            neighbourCellsCoords.Push(buffForCalculations); // стек 4 клетками вокруг текущей заполнили - в цикле while мы разместим эти клетки рандомно в массивах (имитация рандома, чтобы рекурсия не начинала постоянно обход с одной и той же клетки)

            contentOf4Cells[0] = null;
            contentOf4Cells[1] = null;
            contentOf4Cells[2] = null;
            contentOf4Cells[3] = null;

            while (quanOfFilled != 4)
            {
                writingRandomGenerator = Random.Range(0, 4);
                if (contentOf4Cells[writingRandomGenerator] == null)
                {
                    coordsOf4Cells[writingRandomGenerator] = neighbourCellsCoords.Pop();
                    contentOf4Cells[writingRandomGenerator] = maze[(int) coordsOf4Cells[writingRandomGenerator].x, (int) coordsOf4Cells[writingRandomGenerator].y];
                    quanOfFilled++;
                }
                else
                {
                    continue;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (maze[(int) coordsOf4Cells[i].x, (int)coordsOf4Cells[i].y] == "notvisited") // changed
                {
                    float dX, dY;
                    dX = (curCell.x - coordsOf4Cells[i].x) / 2; // вычислили смещение от новой клетки по обеим осям - прибавим смещения к координатам новой клетки
                    dY = (curCell.y - coordsOf4Cells[i].y) / 2; // и получим координаты стены, которую надо удалить

                    if(mazeCubes[(int) (coordsOf4Cells[i].x + dX), (int) (coordsOf4Cells[i].y + dY)] != null)
                    {
                        Destroy(mazeCubes[(int)(coordsOf4Cells[i].x + dX), (int)(coordsOf4Cells[i].y + dY)]);
                        mazeCubes[(int)(coordsOf4Cells[i].x + dX), (int)(coordsOf4Cells[i].y + dY)] = null;
                    }

                    // перед вызовом рекурсии поработать со значениями массивов
                    RecursionToDeleteWalls(coordsOf4Cells[i]);
                }
            }

            return;
        }

        private bool RecursionToShowTheWay(Vector2 curCell) // обозначим путь среди лабиринта
        {
            maze[(int)curCell.x, (int)curCell.y] = "jotaro";

            if (curCell.x == (height - 4) && curCell.y == (width - 4)) // выход из рекурсии
            {
                Instantiate(cubeWayRef, new Vector3(1000 + curCell.x * 5f, 0, 1000 + curCell.y * 5f), rotationRef);
                wayPoints.Push(curCell);
                //Debug.Log(curCell);
                return true;
            }

            Vector2[] coordsOf4Cells = new Vector2[4];

            Vector2 buffForCalculations = curCell;

            buffForCalculations.x -= 1;
            coordsOf4Cells[0] = buffForCalculations;
            buffForCalculations.x += 1;
            buffForCalculations.y += 1;
            coordsOf4Cells[1] = buffForCalculations;
            buffForCalculations.x += 1;
            buffForCalculations.y -= 1;
            coordsOf4Cells[2] = buffForCalculations;
            buffForCalculations.x -= 1;
            buffForCalculations.y -= 1;
            coordsOf4Cells[3] = buffForCalculations;

            for (int i = 0; i < 4; i++)
            {
                if (mazeCubes[(int)coordsOf4Cells[i].x, (int)coordsOf4Cells[i].y] == null)
                {
                    if (maze[(int)coordsOf4Cells[i].x, (int)coordsOf4Cells[i].y] == "dio")
                    {
                        if (RecursionToShowTheWay(coordsOf4Cells[i]))
                        {
                            Instantiate(cubeWayRef, new Vector3(1000 + curCell.x * 5f, 0, 1000 + curCell.y * 5f), rotationRef);
                            wayPoints.Push(curCell);
                            //Debug.Log(curCell);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
