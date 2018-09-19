using System;
using System.Collections;
using UnityEngine;

namespace FbSdk.UI
{
    internal static class TweenExtension
    {
        public static Tweener DOFade(this CanvasGroup canvasGroup, MonoBehaviour monoBehaviour, float to,
            float duration)
        {
            return new Tweener(monoBehaviour, () => canvasGroup.alpha, (a) => canvasGroup.alpha = a, to, duration);
        }
    }

    internal class Tweener
    {
        private readonly MonoBehaviour _behaviour;
        private Coroutine _coroutine;
        private Action _complate;
        private Func<float> _get;
        private Action<float> _set;

        public float To;

        public float Duration;

        public Tweener(MonoBehaviour behaviour, Func<float> get, Action<float> set, float to, float duration)
        {
            _behaviour = behaviour;
            _get = get;
            _set = set;
            To = to;
            Duration = duration;
        }

        public void OnComplete(Action callback)
        {
            _complate = callback;
        }

        public bool IsPlaying()
        {
            return _coroutine != null;
        }

        public Coroutine WaitForCompletion()
        {
            return _behaviour.StartCoroutine(FadeCoroutine(To, Duration));
        }

        private IEnumerator FadeCoroutine(float to, float duration)
        {
            var from = _get();
            var time = 0f;
            while (Math.Abs(to - _get()) > float.Epsilon)
            {
                time += Time.deltaTime;
                _set(Mathf.Lerp(from, to, time / duration));
                yield return null;
            }

            _coroutine = null;
            if (_complate != null) _complate();
        }

        public void Kill()
        {
            if (_coroutine == null) return;

            _behaviour.StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }
}