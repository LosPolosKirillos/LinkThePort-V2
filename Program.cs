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
        private char[] _map;

        public Map(string path)
        {

        }
    }

    class Player
    {
        public int _x { get; private set; }
        public int _y { get; private set; }

        public Player(int x, int y)
        {
            _x = x;
            _y = y;
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
            _playerInPort = player._x == _portX && player._y == _portY;
        }

        public void BecomeVisited()
        {
            if (IsVisited == false && _playerInPort)
            {
                IsVisited = true;
            }
        }
    }

    class Storm : GameObject
    {

    }

    class Pirates : GameObject
    {

    }

}
