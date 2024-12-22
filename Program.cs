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

    class Map //IDrawable
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
                    if(_chars[x, y] == letter)
                    {
                        coordinates.Add(new int[] { x, y });
                    }

            return coordinates;
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

    interface IDrawable
    {
        void Draw(ConsoleColor defaultColor);
    }

    class Player : IDrawable
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private int _lastX;
        private int _lastY;
        private int[] _direction;
        private char _letter;
        

        public Player(int x, int y, char letter = 'S')
        {
            X = x;
            Y = y;
            _letter = letter;
        }

        public void Move()
        {
            _lastX = X;
            _lastY = Y;
            X += _direction[0];
            Y += _direction[1];
        }

        public void Direction(ConsoleKeyInfo pressedKey)
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

    class Port : IDrawable
    {
        private int _portX;
        private int _portY;
        private bool _playerInPort;
        private bool _isVisited;

        public Port(int x, int y)
        {
            _portX = x;
            _portY = y;
            _isVisited = false;
        }

        public void CheckVisitOfPlayer(Player player)
        {
            _playerInPort = player.X == _portX && player.Y == _portY;
            //Add fuel for visit
        }

        public void BecomeVisited()
        {
            if (_isVisited == false && _playerInPort)
            {
                _isVisited = true;
            }
        }

        public void Draw(ConsoleColor defaultColor)
        {
            Console.SetCursorPosition(_portX, _portY);
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
        protected int _hoursRemaining;
        public bool IsActive
        {
            get
            {
                return _hoursRemaining > 0;
            }
        }

        public Field(Map map, char letter, Random random)
        {
            _field = map.GetCoordinatesOfChar(letter);
            _hoursRemaining = random.Next(20, 30);
        }

        public abstract void SpendOneHour();

        public abstract void Draw(ConsoleColor defaultColor);
    }

    class Cyclone : Field
    {
        public Cyclone(Map map, char letter, Random random) : base(map, letter, random) { }

        public override void SpendOneHour()
        {
            _hoursRemaining--;
        }

        //Draw
    }

    class Pirates : Field 
    {
        public Pirates(Map map, char letter, Random random) : base(map, letter, random) { }

        public override void SpendOneHour() { }

        //Draw
    }

}
