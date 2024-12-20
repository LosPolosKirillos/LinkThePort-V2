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

    class Map
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

    class Player
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public char Letter { get; private set; }

        public Player(int x, int y, char letter = 'S')
        {
            X = x;
            Y = y;
            Letter = letter;
        }
    }

    class GameObject
    {
        public virtual void ShowState()
        {

        }
    }

    class Port : GameObject
    {
        private int _portX;
        private int _portY;
        private bool _playerInPort;
        public bool IsVisited { get; private set; }

        public Port(int x, int y)
        {
            _portX = x;
            _portY = y;
            IsVisited = false;
        }

        public void CheckPosition(Player player)
        {
            _playerInPort = player.X == _portX && player.Y == _portY;
        }

        public void BecomeVisited()
        {
            if (IsVisited == false && _playerInPort)
            {
                IsVisited = true;
            }
        }
    }

    abstract class Field
    {
        protected List<int[]> _field;

        public Field(Map map, char letter)
        {
            _field = map.GetCoordinatesOfChar(letter);
            //if (null) ?
        }
    }

    class Cyclone : Field
    {
        public Cyclone(Map map, char letter) : base(map, letter) { }
    }

    class Pirates : Field 
    {
        public Pirates(Map map, char letter) : base(map, letter) { }
    }

}
