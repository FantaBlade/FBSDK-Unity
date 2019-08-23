using System;
using System.Collections;
using UnityEngine;

namespace FantaBlade.UI
{
    internal class Countdown
    {
        private static readonly WaitForSeconds WaitFor1Second = new WaitForSeconds(1);
        private readonly MonoBehaviour _context;
        private Coroutine _coroutine;
        private DateTime _startTime;
        private int _originSecond;
        private int _second;

        public Countdown(MonoBehaviour context)
        {
            _context = context;
        }

        public void Start(int second, Action<int> tick)
        {
            if (_coroutine != null)
            {
                _context.StopCoroutine(_coroutine);
            }

            _originSecond = second;
            _second = second;
            _startTime = DateTime.Now;
            _coroutine = _context.StartCoroutine(Process(tick));
        }

        public void Reset()
        {
            if (_coroutine != null)
            {
                _context.StopCoroutine(_coroutine);
            }
        }

        public void UpdateTime()
        {
            if (_context == null)
            {
                return;
            }

            _second = _originSecond - (int) (DateTime.Now - _startTime).TotalSeconds;
            if (_second < 0)
            {
                _second = 0;
            }
        }

        private IEnumerator Process(Action<int> tick)
        {
            while (_second >= 0)
            {
                if (tick != null) tick(_second);
                _second--;
                if (_second == 0)
                {
                    _coroutine = null;
                }

                yield return WaitFor1Second;
            }
        }
    }
}