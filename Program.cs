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

namespace CSStudy
{
    internal class Program
    {
        static void Main(string[] args)
        {

        }

    }

    class Journey
    {

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
            foreach (Port port in ports)
            {
                if (random.Next(0, 2) == 1)
                    return new Player(port.X, port.Y, random);
            }

            return new Player(ports[0].X, ports[0].Y, random);
        }

        public DeliveryPoint GetDelivery(Random random)
        {
            int x, y;
            while (true)
            {
                x = random.Next(1, _chars.GetLength(0) - 1);
                y = random.Next(1, _chars.GetLength(1) - 1);

                if (_chars[x, y] != ' ' &&
                    _chars[x, y] != 'o')
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

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = defaultColor;

            for (int x = 0; x < _chars.GetLength(0); x++)
                for (int y = 0; y < _chars.GetLength(1); y++)
                    if (_chars[x, y] == 'W' || _chars[x, y] == 'V')
                        Console.Write(' ');
                    else
                        Console.Write(_chars[x, y]);
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
        private int _fuel;
        public bool IsEnoughFuel { get { return _fuel >= 0; } }


        public Player(int x, int y, Random random, char letter = 'S')
        {
            X = x;
            Y = y;
            _letter = letter;
            _fuel = random.Next(25, 45);
            _direction = new int[2];
        }

        public void GetFuelUsageFromField(Field field)
        {
            if (field.FuelUsage > _fuel)
                _fuel = 0;
            else
                _fuel -= field.FuelUsage;
        }

        public void GetFuelFromPort(Port port) { _fuel += port.FuelInPort; }

        public void Move()
        {
            _lastX = X;
            _lastY = Y;

            X += _direction[0];
            Y += _direction[1];

            if (_lastX != X && _lastY != Y)
            {
                _fuel--;
            }
        }

        public void GetDirectionOfMove(ConsoleKeyInfo pressedKey)
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

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(_lastX, _lastY);
            Console.BackgroundColor = defaultColor;
            Console.Write(' ');

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

        public DeliveryPoint (int x, int y, char letter = 'D')
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
        public int X { get; private set; }
        public int Y { get; private set; }
        private char _letter;
        private bool _playerInPort;
        private bool _isVisited;
        public int FuelInPort { get; private set; }

        public Port(int x, int y, Random random, char letter = 'o')
        {
            X = x;
            Y = y;
            _letter = letter;
            _isVisited = false;
            FuelInPort = random.Next(20, 45);
        }

        public void CheckVisitOfPlayer(Player player)
        {
            _playerInPort = player.X == X && player.Y == Y;
        }

        public void BecomeVisited()
        {
            if (_isVisited == false && _playerInPort)
            {
                _isVisited = true;
                FuelInPort = 0;
            }
        }

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(X, Y);
            if (_isVisited)
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
        protected List<int[]> _field;
        protected int _hoursBeforeUpdateState;
        public int FuelUsage { get; protected set; }
        public bool IsActive { get { return _hoursBeforeUpdateState > 0; } }

        public Field(Map map, char letter, Random random)
        {
            _field = map.GetCoordinatesOfChar(letter);
            _hoursBeforeUpdateState = random.Next(20, 30);
        }

        public abstract void SpendOneHour();

        public abstract void UpdateState(Random random);

        public virtual void Draw(ConsoleColor defaultColor)
        {
            for (int i = 0; i < _field.Count; i++)
            {
                Console.SetCursorPosition(_field[i][0], _field[i][1]);
                Console.Write('_');
            }
        }

        public bool IsPlayerInField(Player player)
        {
            for (int i = 0; i < _field.Count; i++)
            {
                if (_field[i][0] == player.X && _field[i][1] == player.Y)
                    return true;
            }

            return false;
        }
    }

    class Cyclone : Field, IDrawable
    {
        public Cyclone(Map map, char letter, Random random) : base(map, letter, random) 
        {
            FuelUsage = 1;
        }

        public override void SpendOneHour()
        {
            _hoursBeforeUpdateState--;
        }

        public override void UpdateState(Random random)
        {
            if (_hoursBeforeUpdateState <= random.Next(-20, -25))
            {
                _hoursBeforeUpdateState = random.Next(20, 30); 
            }
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
        public Pirates(Map map, char letter, Random random) : base(map, letter, random)
        {
            FuelUsage = 50;
        }

        public override void SpendOneHour() { }

        public override void UpdateState(Random random) { }

        public override void Draw(ConsoleColor defaultColor)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            base.Draw(defaultColor);
            Console.ForegroundColor = defaultColor;
        }
    }

}
