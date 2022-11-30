using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantaBlade.Internal
{
    public class MonoUpdateManager
    {
        private Queue<Action> _msgQueue = new Queue<Action>();

        public void RunOnMonoThread(Action action)
        {
            _msgQueue.Enqueue(action);
        }

        public Action Dequeue()
        {
            return _msgQueue.Dequeue();
        }
        
        public void Update(float deltaTime)
        {
            if (null != _msgQueue)
            {
                while (0 < _msgQueue.Count)
                {
                    var act = Dequeue();
                    act();
                }
            }
        }
    }
}