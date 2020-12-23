using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Math = System.Math;
using System.Linq;
using static Tile;

public class WFC : MonoBehaviour
{
    public int chunkSize = 5; // Размер чанка в каждую сторону. Измеряется в тайлах.
    private Pos[] PossiblePositions; //Упрощенные координаты, в которых нет тайлов. 
    //Для того, чтобы не генерировать там, где level-дизайнер уже что-то расставил.
    public Tile[] TilesToSpawn; // Сюда будем помещать тайлы, которые мы сгенерировали, но еще не расставили
    public List<Tile> AllTiles = new List<Tile>(); // Здесь хранятся все варианты тайлов, с которыми будет работать генератор.
    int y = 0; // Высота, на которой спавнится чанк. Для удобства отладки.
    int backtrack = 0; // Кол-во неудачных попыток
    int maxAttemps = 100; // Максимальное кол-во неудачных попыток, после которых происходит исключение
    bool rotateTileSet = true; // Надо ли поворачивать заданные тайлы или нет
    bool setTileSet = true; // Надо ли применять изменения тайлсета (поворот и Weight)
    int k = 0; // Значение упрощения (нок) всех весов тайлов
    bool success = false; // Все ли тайлы правильно выбрались или нет
    private int GetNOKofWeights() // Упрощает веса каждого тайла, для лучшей скорости работы алгоритма
    {
        int NOD(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b) a = a % b;
                else b = b % a;
            }
            return a + b;
        }
        int NODofAll(List<Tile> tiles)
        {
            int n = 1;
            for (int i = 0; i < tiles.Count; i++)
            {
                n = NOD(tiles[i].Weight, n);
            }
            return n;
        }
        int GetMultWeigths(List<Tile> tiles)
        {
            int n = 1;
            foreach (Tile tile in tiles) n *= tile.Weight;
            return n;
        }
        int K = 0; // Нок весов всех тайлов.
        K = GetMultWeigths(AllTiles) / NODofAll(AllTiles);
        return K;
    }
    private void AddN(Tile tile, ref List<Tile> list, int n) // Добавляет tile в list n раз
    {
        for (int i = 0; i < n; i++) list.Add(tile);
    }
    private void Start()
    {
        print("Генератор начал работу..");
        //k = GetNOKofWeights();
        //print(k);
        //print(AllTiles.Count + "тайлов в текущем тайлсете");
        PossiblePositions = new Pos[chunkSize * chunkSize];
        TilesToSpawn = new Tile[chunkSize * chunkSize];
        //GetPossiblePositions();
        List<Tile> AllTilesVars = new List<Tile>();
        foreach (Tile tile in AllTiles)
        {
            //tile.Weight /= k;
            if (rotateTileSet)
            {
                switch (tile.mode)
                {
                    case Rotations.Four:
                        AddN(tile, ref AllTilesVars, tile.Weight);
                        Tile newtile1 = Instantiate(tile);

                        newtile1.Rotate(90);
                        AddN(newtile1, ref AllTilesVars, tile.Weight);

                        Tile newtile2 = Instantiate(tile);
                        newtile2.Rotate(180);
                        AddN(newtile2, ref AllTilesVars, tile.Weight);

                        Tile newtile3 = Instantiate(tile);
                        newtile3.Rotate(270);
                        AddN(newtile3, ref AllTilesVars, tile.Weight);

                        break;
                    case Rotations.One:
                        AddN(tile, ref AllTilesVars, tile.Weight);
                        break;
                }
            }
            else AddN(tile, ref AllTilesVars, tile.Weight);
        }
        if (setTileSet)
            AllTiles = AllTilesVars;
        float x = 0;
        foreach (Tile tile in AllTiles)
        {
            if (!(tile is null))
                Instantiate(tile, new Vector3(x, 0, -1), tile.transform.rotation);
            x += 0.8f;
        }
        //print($"Кол-во элементов в настроенном списке тайлов = {AllTiles.Count}");
    } // Инициализация данных и настройка параметров
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            TilesToSpawn = new Tile[chunkSize * chunkSize];
            Generate();
            y += chunkSize+1;
        }
    }
    private void GetPossiblePositions()
    {
        int i = 0; // Индекс, в который будем помещать незаполненную тайлом позицию
        for (int x = 0; x < chunkSize; x++) // Проверка генерируемого (до генерации) чанка на наличие тайлов. По x & z
        {
            for (int z = 0; z < chunkSize; z++)
            {
                var p = new Pos(x, z, 2);
                bool isTileHere = Physics.Raycast(p.GetPosition(), Vector3.down, 3, 1<<8);
                if (!isTileHere)
                {
                    PossiblePositions[i] = new Pos(x, z);
                }
            }
        }
    }
    private void Generate()
    {
        G();
        success = true;
        Tile SubTile(List<Tile> tiles1, List<Tile> tiles2)
        {
            List<Tile> tiles = new List<Tile>();
            foreach (Tile tile in tiles1)
            {
                foreach (Tile tile2 in tiles2)
                {
                    if (tile.IsEquals(tile2))
                    {
                        tiles.Add(tile);
                        print($"{tile} = {tile2}");
                        //print("Алгоритм нашел одинаковые тайлы в SubTile.");
                    }
                }
            }
            if (tiles.Any())
                return tiles[Random.Range(0, tiles.Count)]; ;
            print($"Функция SubTile вернула null, потому что tiles1 = {tiles1.Count}; tiles2 = {tiles2.Count}; tiles = {tiles.Count}");
            return null;

        } // Есть ли этот тайл в обеих массивах
        
        void TryAgain()
        {
            TilesToSpawn = new Tile[chunkSize * chunkSize];
            backtrack++;
            Debug.LogWarning($"Не получилось. Попробуем еще раз. Попытка номер {backtrack + 1}");
            if (backtrack > maxAttemps)
            {
                Debug.LogError("Не получилось разместить тайлы. Возможно надо изменить " +
                    "tileset или увеличить максимальное кол-во попыток.");
            }
            else
                G();
        } // Если не получилось, то

        void G() {
            if (PossiblePositions.Length == chunkSize * chunkSize)
            {
                print("Ни одного тайла в этом чанке не было до генерации.");
                int i = 0; // Индекс, на который ставим в TilesToSpawn новый тайл.
                for (int z = 0; z < chunkSize; z++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        //print(x + "," + z);
                        Tile tile = null;
                        if (x == 0 && z == 0) tile = Choice(AllTiles); // Первый (левый верхний) тайл выбирается рандомно
                        else if (x > 0 && z == 0) // Если мы на первой строке, то проверяем только левые тайлы
                        {
                            //print(x);
                            var l = GetPossibleTiles(TilesToSpawn[x - 1], Direction.Right);
                            //print($"l в   x>0 z=0  =  {l.Count}");
                            tile = Choice(l);
                            //tile = Choice(AllTiles);
                        }
                        else if (x == 0 && z > 0) // Если на второй и более строке, то проверяем только верхние тайлы
                        {
                            //print($"{TilesToSpawn[z * chunkSize + x - chunkSize]} - текущий тайл во втором условии..");
                            var l = GetPossibleTiles(TilesToSpawn[z * chunkSize + x - chunkSize], Direction.Back);
                            //print($"l в   x=0 z>0  =  {l.Count}");
                            tile = Choice(l);
                            //tile = Choice(AllTiles);
                        }
                        else if (x > 0 && z > 0) // Если на второй и более строке, то проверяем и верхние тайлы, и правые
                        {
                            tile = SubTile(
                                GetPossibleTiles(TilesToSpawn[z * chunkSize + x - chunkSize], Direction.Back),
                                GetPossibleTiles(TilesToSpawn[z * chunkSize + x - 1], Direction.Right));
                            //tile = Choice(AllTiles);
                        }
                        if (tile is null) // Если не получилось расположить тайл.
                        {
                            x = 0;
                            z = 0;
                            i = 0;
                            success = false;
                            TryAgain();
                        }
                        else
                        {
                            TilesToSpawn[i] = tile;
                            i++;
                        }
                    }
                }
            }
        }

        if (success)
            PlaceAllTiles();
    } // Основная функция для генерации чанка
    private void PlaceTile(Tile tile, Pos position)
    {
        Instantiate(tile.gameObject, position.GetPosition(), tile.transform.rotation);
    } // Спавнит один тайл в позиции position
    private void PlaceAllTiles()
    {
        Debug.LogWarning("PlaceAllTiles функция была вызвана!");
        int i = 0;
        for (int z = chunkSize; z > 0; z--)
        {
            for (int x = y; x < chunkSize+y; x++)
            {
                PlaceTile(TilesToSpawn[i], new Pos(x, z, 0));
                i++;
                //print(TilesToSpawn[i].gameObject.transform.position);
            }
        }
    } // Спавнит все тайлы из TilesToSpawn на высоте y
    private List<Tile> GetPossibleTiles(Tile tile, Direction direction)
    {
        List<Tile> tiles = new List<Tile>();
        if (tile is null) return null;
        foreach (Tile t in AllTiles)
        {
           // print("Следующий тайл на проверку!   " + $"tile = {tile}; t = {t}");
            switch (direction)
            {
                case Direction.Back:
                    //var l = t.forwardSide.Reverse().ToList();
                    //if (tile.backwardSide.ToList() == l)
                    if (AreSidesEqual(tile.backwardSide, t.forwardSide.Reverse().ToArray()))
                        tiles.Add(t);
                    break;
                /*case Direction.Forward:
                    if (tile.forwardSide == t.backwardSide) tiles.Add(t);
                    break;*/
                case Direction.Right:
                    //if (tile.rightSide.ToList() == t.leftSide.Reverse().ToList())
                    if (AreSidesEqual(tile.rightSide, t.leftSide.Reverse().ToArray())) //.Reverse()
                        tiles.Add(t);
                    break;
                /*case Direction.Left:
                    if (tile.leftSide == t.rightSide) tiles.Add(t);
                    break;*/
                default:
                    break;
            }
        }
        if (tiles.Count == 0) Debug.LogError($"GetPossibleTiles вернула пустой список; tile = {tile}");
        return tiles;
    } // Расчитывает все возможные тайлы для tile в направлении direction от него
    private Tile Choice(List<Tile> tiles)
    {
        //if (tiles is null) return null;
        if (tiles.Any())
            return tiles[Random.Range(0, tiles.Count)];
        print($"Функция Choice вернула null, tiles = {tiles.Count}");
        return null;
    } // Выбирает рандомно тайл из массива
    private bool AreSidesEqual(SideType[] tile1, SideType[] tile2)
    {
        if (tile1.Length != tile2.Length) return false;
        for (int i = 0; i < tile1.Length; i++)
        {
            if (tile1[i] != tile2[i]) return false;
        }
        return true;
    } // Сравнивает два массива SideTile
}
public class Pos // Класс для упрощенного хранения координат по сетке. Матричные координаты
{
    public int x; // Координаты по матрице. Надо умножить на длину тайла и получить мировые координаты
    public int z; // Координаты по матрице. Надо умножить на длину тайла и получить мировые координаты
    public float y; // Высота. Постоянна
    public static float TileSize = 0.8f; // Действительный размер одного тайла по x и z. Предполагается, что он квадратный. 
    public Pos(int x, int z, int y = 1)
    {
        this.x = x;
        this.z = z;
        this.y = y;
    }
    public Vector3 GetPosition() // Возвращает мировые координаты центра тайла.
    {
        return new Vector3(x * TileSize, y, z * TileSize);
    }
}