using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public class SyncedProp<T> : ISharedModel
    {
        private T _value;

        public T Value { get { return _value; } set { _value = value; } }

        public static implicit operator T(SyncedProp<T> value)
        {
            return value.Value;
        }

        public static implicit operator SyncedProp<T>(T value)
        {
            return new SyncedProp<T> { Value = value };
        }
    }

    public class MyProp<T>
    {
        private T _value;

        public T Value
        {
            get
            {
                // insert desired logic here
                return _value;
            }
            set
            {
                // insert desired logic here
                _value = value;
            }
        }

        public static implicit operator T(MyProp<T> value)
        {
            return value.Value;
        }

        public static implicit operator MyProp<T>(T value)
        {
            return new MyProp<T> { Value = value };
        }
    }

    public class Status : ISharedModel
    {
        public MyProp<int> Countdown { get; set; }

        public Status()
        {
            Countdown = 10;
            _gameStarted = false;
            _showCountdown = false;
            //_countdown = 10;
        }

        private bool _gameStarted;
        public bool GameStarted
        {
            get { return _gameStarted; }
            set
            {
                if (value != _gameStarted)
                {
                    _gameStarted = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool _showCountdown;
        public bool ShowCountdown
        {
            get { return _showCountdown; }
            set
            {
                if (value != _showCountdown)
                {
                    _showCountdown = value;
                    OnPropertyChanged();
                }
            }
        }

        /*public int _countdown;
        public int Countdown
        {
            get { return _countdown; }
            set
            {
                if (value != _countdown)
                {
                    _countdown = value;
                    OnPropertyChanged();
                }
            }
        }*/
    }
}
