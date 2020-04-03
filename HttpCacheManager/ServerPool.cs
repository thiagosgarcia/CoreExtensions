using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PenguinSoft.HttpCacheManager
{
    public class ServerPool
    {
        private readonly SynchronizedCollection<Uri> endpoints = new SynchronizedCollection<Uri>();

        private int _currentIndex;
#pragma warning disable IDE0044 // Add readonly modifier
        private static object _locker = new object();
#pragma warning restore IDE0044 // Add readonly modifier
        public int CurrentIndex
        {
            get
            {
                Task.Run(() =>
                {
                    lock (_locker)
                    {
                        if (++_currentIndex >= endpoints.Count)
                            _currentIndex = 0;
                    }
                });
                return _currentIndex;
            }
            set
            {
                lock (_locker)
                {
                    if (value >= 0 || value < endpoints.Count)
                        _currentIndex = value;
                    else
                        _currentIndex = 0;
                }
            }
        }

        public void Add(Uri url)
        {
            endpoints.Add(url);
        }

        public void RemoveAt(int index)
        {
            endpoints.RemoveAt(index);
        }

        public Uri Next()
        {
            return endpoints[CurrentIndex];
        }
    }
}