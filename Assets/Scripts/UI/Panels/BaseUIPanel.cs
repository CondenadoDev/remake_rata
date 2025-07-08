using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UISystem.Core;
using UISystem.Binding;

namespace UISystem.Panels
{
    public abstract class BaseUIPanel : MonoBehaviour, IUIElement
    {
        [Header("Panel Settings")] [SerializeField]
        public string panelId;

        [SerializeField] public bool startHidden = true;
        [SerializeField] protected float animationDuration = 0.3f;

        [Header("Animation")] [SerializeField] public PanelAnimation showAnimation = PanelAnimation.FadeScale;
        [SerializeField] public PanelAnimation hideAnimation = PanelAnimation.FadeScale;
        [SerializeField] protected Ease easeType = Ease.OutBack;

        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected bool isVisible;
        protected List<DataBinding> bindings = new List<DataBinding>();

        public string PanelId => panelId;
        public bool IsVisible => isVisible;
        public GameObject GameObject => gameObject;

        public event Action<BaseUIPanel> OnPanelShown;
        public event Action<BaseUIPanel> OnPanelHidden;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Collect all bindings
            bindings.AddRange(GetComponentsInChildren<DataBinding>());

            if (startHidden)
            {
                gameObject.SetActive(false);
                isVisible = false;
            }
        }

        public virtual void Initialize()
        {
            OnInitialize();
        }

        protected abstract void OnInitialize();

        public virtual void Show(float duration = -1)
        {
            if (duration < 0) duration = animationDuration;

            gameObject.SetActive(true);
            StopAllCoroutines();

            switch (showAnimation)
            {
                case PanelAnimation.None:
                    canvasGroup.alpha = 1;
                    transform.localScale = Vector3.one;
                    break;

                case PanelAnimation.Fade:
                    canvasGroup.alpha = 0;
                    canvasGroup.DOFade(1, duration).SetEase(easeType);
                    break;

                case PanelAnimation.Scale:
                    transform.localScale = Vector3.zero;
                    transform.DOScale(1, duration).SetEase(easeType);
                    break;

                case PanelAnimation.FadeScale:
                    canvasGroup.alpha = 0;
                    transform.localScale = Vector3.one * 0.8f;

                    DOTween.Sequence()
                        .Append(canvasGroup.DOFade(1, duration))
                        .Join(transform.DOScale(1, duration))
                        .SetEase(easeType);
                    break;

                case PanelAnimation.SlideLeft:
                    rectTransform.anchoredPosition = new Vector2(-Screen.width, 0);
                    rectTransform.DOAnchorPos(Vector2.zero, duration).SetEase(easeType);
                    break;

                case PanelAnimation.SlideRight:
                    rectTransform.anchoredPosition = new Vector2(Screen.width, 0);
                    rectTransform.DOAnchorPos(Vector2.zero, duration).SetEase(easeType);
                    break;
            }

            isVisible = true;
            OnShow();
            OnPanelShown?.Invoke(this);
        }

        public virtual void Hide(float duration = -1)
        {
            if (duration < 0) duration = animationDuration;

            StopAllCoroutines();

            Action onComplete = () =>
            {
                gameObject.SetActive(false);
                isVisible = false;
                OnHide();
                OnPanelHidden?.Invoke(this);
            };

            switch (hideAnimation)
            {
                case PanelAnimation.None:
                    onComplete();
                    break;

                case PanelAnimation.Fade:
                    canvasGroup.DOFade(0, duration).SetEase(easeType).OnComplete(() => onComplete());
                    break;

                case PanelAnimation.Scale:
                    transform.DOScale(0, duration).SetEase(easeType).OnComplete(() => onComplete());
                    break;

                case PanelAnimation.FadeScale:
                    DOTween.Sequence()
                        .Append(canvasGroup.DOFade(0, duration))
                        .Join(transform.DOScale(0.8f, duration))
                        .SetEase(easeType)
                        .OnComplete(() => onComplete());
                    break;

                case PanelAnimation.SlideLeft:
                    rectTransform.DOAnchorPos(new Vector2(-Screen.width, 0), duration)
                        .SetEase(easeType)
                        .OnComplete(() => onComplete());
                    break;

                case PanelAnimation.SlideRight:
                    rectTransform.DOAnchorPos(new Vector2(Screen.width, 0), duration)
                        .SetEase(easeType)
                        .OnComplete(() => onComplete());
                    break;
            }
        }

        public virtual void SetInteractable(bool interactable)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected void BindToData(object dataContext)
        {
            foreach (var binding in bindings)
            {
                if (!string.IsNullOrEmpty(binding.propertyPath))
                {
                    binding.Bind(dataContext, binding.propertyPath);
                }
            }
        }
    }
}