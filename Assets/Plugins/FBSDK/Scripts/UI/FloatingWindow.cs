using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FbSdk.UI
{
    public class FloatingWindow : MonoBehaviour, IDragHandler, IPointerClickHandler
    {
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private Camera _worldCamera;

        public event Action Click;

        private void Awake()
        {
            _rectTransform = (RectTransform) transform;
            _canvas = _rectTransform.parent.GetComponent<Canvas>();
            _worldCamera = _canvas.worldCamera;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 pos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform,
                eventData.position, _worldCamera, out pos))
            {
                _rectTransform.position = pos;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            const float offset = 2;
            const float sqrOffset = offset * offset;
            if ((eventData.position - eventData.pressPosition).sqrMagnitude < sqrOffset)
            {
                if (Click != null)
                {
                    Click();
                }
            }
        }
    }
}