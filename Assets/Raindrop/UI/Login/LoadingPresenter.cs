using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace Raindrop.Presenters
{
    // loading bar view.
    public class LoadingPresenter : MonoBehaviour
    {
        public TMP_Text text;
        public Button CloseButton;

        public CanvasGroup canvas;
        // public string loginMsg;
        public ReactiveProperty<string> loginMsg { get; set;  }//

        private FadingState state = FadingState.None;
        [SerializeField] public float delayBeforeFade { get; set; } = 2;

        void Awake()
        {
            loginMsg = new ReactiveProperty<string>(""); //"" is a magic value that is required to prevent showing the modal immediately on load.
            loginMsg.AsObservable().Subscribe(
                (message) =>
                {
                    text.text = message;
                }
                );

            CloseButton.onClick.AsObservable().Subscribe(_ => OnCloseBtnClick());
            disableCloseBtn();

            if (text == null)
            {
                OpenMetaverse.Logger.Log("loading bar fail.", Helpers.LogLevel.Error);
            }
        }

        //disable myself on start, as obviously no one wants a loading in screen at start.
        private void Start()
        {
            FadeOut();
        }


        private void OnCloseBtnClick()
        {
            //CloseButton.interactable = false;
            disableCloseBtn();
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
        public void enableCloseBtn()
        {
            CloseButton.gameObject.SetActive(true);
            // CloseButton.interactable = true;
        }
        
        // set the close button to not visible.
        public void disableCloseBtn()
        {
            CloseButton.gameObject.SetActive(false);
            // CloseButton.interactable = true;
        }
    }

    public enum FadingState
    {
        In,
        Out,
        None
    }

}
