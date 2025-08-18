
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;

public class SnakeGame
{
    public class Point
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point Add(Point other) => new Point(X + other.X, Y + other.Y);

        public Point Inverse() => new Point(-this.X, -this.Y);

        public Point Multiply(int num) => new Point(this.X * num, this.Y * num);

        public string Key() => $"{this.X}, {this.Y}";
    }

    public string[][] grid;
    public Point[] snakeBody;
    public Point currentDirection;
    public int gridSize;
    public bool gameOver;
    private Random rand = new Random();

    private WebSocket socket;

    public SnakeGame(int gridSize, WebSocket socket)
    {
        this.socket = socket;
        grid = new string[gridSize][];
        for (int i = 0; i < gridSize; i++)
        {
            this.grid[i] = new string[gridSize];
            for (int j = 0; j < gridSize; j++)
            {
                this.grid[i][j] = "";
            }
        }
        this.gridSize = gridSize;
        int startX = rand.Next(2, gridSize - 3);
        int startY = rand.Next(2, gridSize - 3);
        Point[] directions = new Point[]
        {
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1)
        };
        Point startDirection = directions[rand.Next(0, directions.Length)];
        this.currentDirection = startDirection;
        Point startPoint = new Point(startX, startY);

        this.snakeBody = new Point[] { startPoint, startPoint.Add(startDirection.Inverse()), startPoint.Add(startDirection.Inverse().Multiply(2)) };
        this.SpawnFruit();
        this.gameOver = false;

    }

    

    public bool GetGameOver()
    {
        return this.gameOver;
    }

    public void SetDirection(Point direction)
    {
        if (direction.Y != 0 && this.currentDirection.Y == 0)
        {
            this.currentDirection = new Point(0, direction.Y);
        }
        else if (direction.X != 0 && this.currentDirection.X == 0)
        {
            this.currentDirection = new Point(direction.X, 0);
        }
    }

    public bool CheckCollideWithBody(Point headPoint)
    {
        HashSet<string> invalidPoints = new HashSet<string>();
        for (int i = 1; i < this.snakeBody.Length; i++)
        {
            invalidPoints.Add($"{this.snakeBody[i].X}-{this.snakeBody[i].Y}");
        }

        string headString = $"{headPoint.X}-{headPoint.Y}";

        if (invalidPoints.Contains(headString))
        {
            return true;
        }
        return false;
    }

    public bool CheckCollideWithWall(Point headPoint)
    {
        if (headPoint.X >= this.grid.Length || headPoint.Y >= this.grid.Length || headPoint.X < 0 || headPoint.Y < 0)
        {
            return true;
        }
        return false;
    }

    public void MoveBody(Point direction)
    {
        Point head = this.snakeBody[0];
        Point newHeadPoint = head.Add(direction);

        if (this.CheckCollideWithBody(newHeadPoint) || this.CheckCollideWithWall(newHeadPoint))
        {
            this.gameOver = true;
            return;
        }

        Point lastModifiedPoint = null;
        for (int i = 1; i < this.snakeBody.Length; i++)
        {
            if (lastModifiedPoint != null)
            {
                Point nextModifiedPoint = this.snakeBody[i];
                this.snakeBody[i] = lastModifiedPoint;
                lastModifiedPoint = nextModifiedPoint;
            }
            else
            {
                lastModifiedPoint = this.snakeBody[i];
                this.snakeBody[i] = this.snakeBody[i - 1];
            }
        }

        this.snakeBody[0] = newHeadPoint;

        if (this.AttemptToEatFruit() && lastModifiedPoint != null)
        {
            this.snakeBody.Append(lastModifiedPoint);
            this.SpawnFruit();
        }
    }

    public Point[] GetPossibleFruitSpawns()
    {
        Point[] validSpawns = new Point[] { };
        HashSet<string> invalidSpawnSet = new HashSet<string>();

        for (int i = 0; i < this.snakeBody.Length; i++)
        {
            Point point = this.snakeBody[i];
            invalidSpawnSet.Add($"{point.X}-{point.Y}");
        }

        for (int x = 0; x < this.gridSize; x++)
        {
            for (int y = 0; y < this.gridSize; y++)
            {
                if (!invalidSpawnSet.Contains($"{x}-{y}") && this.grid[x][y] == "")
                {
                    validSpawns.Append(new Point(x, y));
                }
            }
        }
        return validSpawns;
    }

    public void SpawnFruit()
    {
        Point[] validSpawnPoints = this.GetPossibleFruitSpawns();

        Point randomSpawnPoint = validSpawnPoints[rand.Next(0, validSpawnPoints.Length)];

        this.grid[randomSpawnPoint.X][randomSpawnPoint.Y] = "F";
    }

    // Returns true when a fruit was eaten, false otherwise
    public bool AttemptToEatFruit()
    {
        Point head = this.snakeBody[0];
        if (this.grid[head.X][head.Y] == "F")
        {
            this.grid[head.X][head.Y] = "";
            return true;
        }
        return false;
    }

    public void Step()
    {
        this.MoveBody(this.currentDirection);
    }

    public HashSet<string> GetPossibleDirectionKeys()
    {
        Point[] directions = new Point[]
        {
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1)
        };

        Point invalidDirection = this.currentDirection.Inverse(); // 180 not allowed

        HashSet<string> validDirections = new HashSet<string>();
        Point headLocation = this.snakeBody[0];

        for (int i = 0; i < directions.Length; i++)
        {
            if (!(directions[i].X == invalidDirection.X && directions[i].Y == invalidDirection.Y)
                && !this.CheckCollideWithBody(headLocation.Add(directions[i]))
                && !this.CheckCollideWithBody(headLocation.Add(directions[i])))
            {
                validDirections.Add(directions[i].Key());
            }
        }

        return validDirections;
    }

    public Point GetFruitLocation()
    {
        for (int x = 0; x < this.grid.Length; x++)
        {
            for (int y = 0; y < this.grid.Length; y++)
            {
                if (this.grid[x][y] == "F")
                {
                    return new Point(x, y);
                }
            }
        }
        return new Point(0, 0);
    }
}

