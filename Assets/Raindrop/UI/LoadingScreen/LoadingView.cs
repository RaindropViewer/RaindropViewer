using System.Collections;
using OpenMetaverse;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Raindrop.UI.LoadingScreen
{
    // loading bar view.
    public class LoadingView : MonoBehaviour
    {
        public TMP_Text text;
        public Button CloseButton;

        public CanvasGroup canvas;
        public ReactiveProperty<string> loginMsg { get; set;  }

        private FadingState state = FadingState.None;
        [SerializeField] public float delayBeforeFade = 2;

        
        public void Init()
        {
            loginMsg = new ReactiveProperty<string>(""); //"" is a magic value that is required to prevent showing the modal immediately on load.
            loginMsg.AsObservable().Subscribe(
                (message) =>
                {
                    text.text = message;
                }
                );

            CloseButton.onClick.AsObservable().Subscribe(_ => OnCloseBtnClick());
            disappearCloseBtn();

            if (text == null)
            {
                OpenMetaverse.Logger.Log("loading bar fail.", Helpers.LogLevel.Error);
            }
        }

        //disable loading screen at start.
        private void Start()
        {
            FadeOut();
        }
        
        private void OnCloseBtnClick()
        {
            disappearCloseBtn();
            FadeOut();
        }

        public void ResetLoadingText()
        {
            loginMsg.Value = "";
        }

        #region Fading UI logic
        
        public void FadeIn()
        {
            if (state == FadingState.In)
            {
                return;
            }
            state = FadingState.In;
            StopAllCoroutines();
            
            canvas.interactable = false;
            canvas.blocksRaycasts = true;
            StartCoroutine(FadeInCoroutine());
            
            // fade from transparent to opaque
            IEnumerator FadeInCoroutine()
            {
                float target = 1;
                float initial = canvas.alpha;
                // loop over 1 second
                for (float i = initial; i <= target; i += Time.deltaTime)
                {
                    // set color with i as alpha
                    canvas.alpha = i;
                    yield return null;
                }
                state = FadingState.None;
                canvas.interactable = true;
                canvas.blocksRaycasts = true;
                yield break;
            }
        }

        public void FadeOut()
        {
            if (state == FadingState.Out)
            {
                return;
            }
            state = FadingState.Out;
            StopAllCoroutines();
            canvas.blocksRaycasts = true;
            canvas.interactable = false;
            
            StartCoroutine(FadeoutCoroutine(delayBeforeFade));
            
            IEnumerator FadeoutCoroutine(float delay)
            {
                yield return new WaitForSeconds(delay);
                
                float target = 0;
                float initial = canvas.alpha;
                // loop over 1 second
                for (float i = initial; i >= target; i -= Time.deltaTime)
                {
                    // set color with i as alpha
                    canvas.alpha = i;
                    yield return null;
                }
                
                canvas.blocksRaycasts = false;
                canvas.interactable = false;
                state = FadingState.None;
                
                ResetLoadingText();
                yield break;
            }
        }


        #endregion

        // set the close button to visible.
        public void appearCloseBtn()
        {
            CloseButton.gameObject.SetActive(true);
        }
        
        // set the close button to not visible.
        public void disappearCloseBtn()
        {
            CloseButton.gameObject.SetActive(false);
        }

    }

    public enum FadingState
    {
        In,
        Out,
        None
    }

}
