using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Globalization;
using System.Diagnostics.Metrics;
using System.Drawing;

namespace CSStudy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.BufferHeight = 1000;
            Console.BufferWidth = 1000;

            Journey journey = new Journey("world_map.txt");
            journey.Start();
        }

    }

    class Journey
    {
        private Map _map;
        private List<Port> _ports;
        private List<Field> _fields = new List<Field>();
        private Player _player;
        private DeliveryPoint _deliveryPoint;
        private ConsoleColor _defaultBackColor;
        private ConsoleColor _defaultForeColor;

        public Journey(string path)
        {
            Random random = new Random();
            _defaultBackColor = Console.BackgroundColor;
            _defaultForeColor = Console.ForegroundColor;

            _map = new Map(path);
            _ports = _map.GetPortsList(random);
            _fields.Add(new Cyclone(_map, 'W', random));
            _fields.Add(new Cyclone(_map, 'V', random));
            _fields.Add(new Pirates(_map, 'P', random));
            _player = _map.GetPlayerAndStart(_ports, random);
            _deliveryPoint = _map.GetDelivery(random);
        }

        public void Start()
        {
            bool pointIsReached = false;

            _map.Draw(_defaultBackColor);

            while (true)
            {
                foreach (Port port in _ports)
                {
                    if (port.IsAlreadyVisited) { continue; }
                    else
                    {
                        if (port.CheckVisitOfPlayer(_player))
                        {
                            _player.GetFuelFromPort(port);
                            port.BecomeVisited();
                        }
                    }
                }

                foreach (Field field in _fields)
                {
                    field.UpdateState(new Random());
                }

                DrawGameObjects();
                _map.DrawInfoBoard(_player, _defaultBackColor, _defaultForeColor);
                Console.CursorVisible = false;

                if (_player.IsEnoughFuel && pointIsReached == false)
                {
                    _player.SetDirectionOfMove(Console.ReadKey());
                    _player.Move();
                    if (_map.DidPlayerCollide(_player))
                        if (_deliveryPoint.IsPointReached(_player))
                            pointIsReached = true;
                        else
                            _player.CancelMove();
                    else
                        foreach (Field field in _fields)
                        {
                            if (field.IsActive)
                                if (field.IsPlayerInField(_player))
                                {
                                    _player.GetFuelUsageFromField(field);
                                }
                            field.SpendOneHour();
                        }
                }
                else { break; }
            }

            _map.DrawGameResult(pointIsReached, _defaultForeColor);
            Console.ReadKey();
        }

        private void DrawGameObjects()
        {
            _player.DrawLastPosition(_defaultBackColor);

            foreach (Port port in _ports)
                port.Draw(_defaultBackColor);

            foreach (Field field in _fields)
                field.Draw(_defaultForeColor);

            _deliveryPoint.Draw(_defaultBackColor);

            _player.Draw(_defaultBackColor);
        }
    }

    interface IDrawable
    {
        void Draw(ConsoleColor defaultColor);
    }

    class Map : IDrawable
    {
        private char[,] _chars;

        public Map(string path)
        {
            string[] file = File.ReadAllLines(path);
            _chars = new char[GetMaxLengthOfLine(file), file.Length];

            for (int x = 0; x < _chars.GetLength(0); x++)
                for (int y = 0; y < _chars.GetLength(1); y++)
                    _chars[x, y] = file[y][x];
        }

        public List<int[]> GetCoordinatesOfChar(char letter)
        {
            List<int[]> coordinates = new List<int[]>();

            for (int x = 0; x < _chars.GetLength(0); x++)
                for (int y = 0; y < _chars.GetLength(1); y++)
                    if (_chars[x, y] == letter)
                    {
                        coordinates.Add(new int[] { x, y });
                    }

            return coordinates;
        }

        public Player GetPlayerAndStart(List<Port> ports, Random random)
        {
            while (true)
            {
                foreach (Port port in ports)
                {
                    if (random.Next(0, 20) == 1)
                        return new Player(port.X, port.Y, random);
                }
            }
        }

        public DeliveryPoint GetDelivery(Random random)
        {
            int x, y;
            while (true)
            {
                x = random.Next(1, _chars.GetLength(0) - 1);
                y = random.Next(1, _chars.GetLength(1) - 1);

                if (_chars[x, y] != ' ' && _chars[x, y] != 'o' &&
                    _chars[x, y] != 'W' && _chars[x, y] != 'V' &&
                    _chars[x, y] != 'P')
                {
                    if (_chars[x, y - 1] == ' ')
                        break;
                    else if (_chars[x, y + 1] == ' ')
                        break;
                    else if (_chars[x - 1, y] == ' ')
                        break;
                    else if (_chars[x + 1, y] == ' ')
                        break;
                }
            }

            return new DeliveryPoint(x, y);
        }

        public List<Port> GetPortsList(Random random, char letter = 'o')
        {
            List<Port> ports = new List<Port>();

            for (int x = 0; x < _chars.GetLength(0); x++)
                for (int y = 0; y < _chars.GetLength(1); y++)
                    if (_chars[x, y] == letter)
                    {
                        ports.Add(new Port(x, y, random));
                    }

            return ports;
        }

        public bool DidPlayerCollide(Player player)
        {
            switch (_chars[player.X, player.Y])
            {
                case ' ':
                case 'o':
                case 'P':
                case 'W':
                case 'V':
                    return false;
                default:
                    return true;
            }
        }

        public void DrawInfoBoard(Player player, ConsoleColor defaultBackColor, ConsoleColor defaultForeColor)
        {
            Console.SetCursorPosition(0, _chars.GetLength(1));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"--- Fuel: {player.Fuel} --- | '");
            Console.ForegroundColor = defaultForeColor;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Write("o");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = defaultBackColor;
            Console.Write("' - Active port  | '");
            Console.ForegroundColor = defaultForeColor;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("o");
            Console.BackgroundColor = defaultBackColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("' - Visited port  | '");
            Console.ForegroundColor = defaultForeColor;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.Write("S");
            Console.BackgroundColor = defaultBackColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("' - Your transport (ship)  | '");
            Console.ForegroundColor = defaultForeColor;
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Write("D");
            Console.BackgroundColor = defaultBackColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("' - Delivery point  | '");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("_");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("' - Pirate's activity zone (risk of large fuel loss)\n");
            Console.ForegroundColor = defaultForeColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("                 | '");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("_");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("' - Cyclone's activity zone (x2 losing fuel)");
            Console.ForegroundColor = defaultForeColor;
        }

        public void DrawGameResult(bool pointIsReached, ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(0, _chars.GetLength(1) + 1);
            if (pointIsReached)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Success! \n");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Fail! \n");
            }
            Console.ForegroundColor = defaultColor;
        }

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = defaultColor;

            for (int y = 0; y < _chars.GetLength(1); y++)
            {
                for (int x = 0; x < _chars.GetLength(0); x++)
                    Console.Write(_chars[x, y]);

                Console.WriteLine();
            }
        }

        private int GetMaxLengthOfLine(string[] lines)
        {
            int maxLength = lines[0].Length;

            foreach (string line in lines)
            {
                if (line.Length > maxLength)
                    maxLength = line.Length;
            }

            return maxLength;
        }
    }

    class Player : IDrawable
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private int _lastX;
        private int _lastY;
        private int[] _direction;
        private char _letter;
        public int Fuel { get; private set; }
        public bool IsEnoughFuel { get { return Fuel > 0; } }


        public Player(int x, int y, Random random, char letter = 'S')
        {
            X = x;
            Y = y;
            _letter = letter;
            Fuel = random.Next(25, 45);
            _direction = new int[2];
        }

        public void GetFuelUsageFromField(Field field)
        {
            if (field.FuelUsage > Fuel)
                Fuel = 0;
            else
                Fuel -= field.FuelUsage;
        }

        public void GetFuelFromPort(Port port) { Fuel += port.FuelInPort; }

        public void Move()
        {
            _lastX = X;
            _lastY = Y;

            X += _direction[0];
            Y += _direction[1];

            Fuel--;
        }

        public void CancelMove()
        {
            X = _lastX;
            Y = _lastY;

            Fuel++;
        }

        public void SetDirectionOfMove(ConsoleKeyInfo pressedKey)
        {
            _direction = new int[2];

            if (pressedKey.Key == ConsoleKey.UpArrow)
                _direction[1] = -1;
            else if (pressedKey.Key == ConsoleKey.DownArrow)
                _direction[1] = 1;
            else if (pressedKey.Key == ConsoleKey.LeftArrow)
                _direction[0] = -1;
            else if (pressedKey.Key == ConsoleKey.RightArrow)
                _direction[0] = 1;
        }

        public void DrawLastPosition(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(_lastX, _lastY);
            Console.BackgroundColor = defaultColor;
            Console.Write(' ');

            Console.BackgroundColor = defaultColor;
        }

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(X, Y);
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.Write(_letter);

            Console.BackgroundColor = defaultColor;
        }
    }

    class DeliveryPoint : IDrawable
    {
        private int _x;
        private int _y;
        private char _letter;

        public DeliveryPoint(int x, int y, char letter = 'D')
        {
            _x = x;
            _y = y;
            _letter = letter;
        }

        public bool IsPointReached(Player player)
        {
            return player.X == _x && player.Y == _y;
        }

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(_x, _y);
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Write(_letter);

            Console.BackgroundColor = defaultColor;
        }
    }

    class Port : IDrawable
    {
        private char _letter;
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsAlreadyVisited { get; private set; }
        public int FuelInPort { get; private set; }

        public Port(int x, int y, Random random, char letter = 'o')
        {
            X = x;
            Y = y;
            _letter = letter;
            IsAlreadyVisited = false;
            FuelInPort = random.Next(20, 45);
        }

        public bool CheckVisitOfPlayer(Player player)
        {
            return player.X == X && player.Y == Y;
        }

        public void BecomeVisited()
        {
            IsAlreadyVisited = true;
            FuelInPort = 0;
        }

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(X, Y);
            if (IsAlreadyVisited)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            }
            Console.Write('o');

            Console.BackgroundColor = defaultColor;
        }
    }

    abstract class Field : IDrawable
    {
        protected List<int[]> FieldCoordinates;
        protected int HoursBeforeUpdateState;
        protected Random RandomOfField;
        public int FuelUsage { get { return GetFieldFuelUsage(RandomOfField); } protected set { } }
        public bool IsActive { get { return HoursBeforeUpdateState > 0; } }

        public Field(Map map, char letter, Random random)
        {
            RandomOfField = random;
            FieldCoordinates = map.GetCoordinatesOfChar(letter);
            HoursBeforeUpdateState = random.Next(10, 30);
        }

        public abstract void SpendOneHour();

        public abstract void UpdateState(Random random);

        public abstract int GetFieldFuelUsage(Random random);

        public virtual void Draw(ConsoleColor defaultColor)
        {
            for (int i = 0; i < FieldCoordinates.Count; i++)
            {
                Console.SetCursorPosition(FieldCoordinates[i][0], FieldCoordinates[i][1]);
                if (IsActive)
                    Console.Write('_');
                else
                    Console.Write(' ');
            }
        }

        public bool IsPlayerInField(Player player)
        {
            for (int i = 0; i < FieldCoordinates.Count; i++)
            {
                if (FieldCoordinates[i][0] == player.X && FieldCoordinates[i][1] == player.Y)
                    return true;
            }

            return false;
        }
    }

    class Cyclone : Field, IDrawable
    {
        public Cyclone(Map map, char letter, Random random) : base(map, letter, random) { }

        public override void SpendOneHour()
        {
            HoursBeforeUpdateState--;
        }

        public override void UpdateState(Random random)
        {
            if (HoursBeforeUpdateState <= random.Next(-25, -15))
            {
                HoursBeforeUpdateState = random.Next(10, 30);
            }
        }

        public override int GetFieldFuelUsage(Random random)
        {
            return random.Next(0, 2);
        }
        public override void Draw(ConsoleColor defaultColor)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            base.Draw(defaultColor);
            Console.ForegroundColor = defaultColor;
        }
    }

    class Pirates : Field, IDrawable
    {
        public Pirates(Map map, char letter, Random random) : base(map, letter, random) { }


        public override void SpendOneHour() { }

        public override void UpdateState(Random random) { }

        public override int GetFieldFuelUsage(Random random)
        {
            int[] values = new int[30];
            values[0] = 50;
            random.Shuffle(values);

            return values[0];
        }

        public override void Draw(ConsoleColor defaultColor)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            base.Draw(defaultColor);
            Console.ForegroundColor = defaultColor;
        }

    }

}
