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
    public class LoadingCanvasPresenter : MonoBehaviour
    {
        public TMP_Text text;
        public Button CloseButton;

        public CanvasGroup canvas;
        // public string loginMsg;
        public ReactiveProperty<string> loginMsg { get; set;  }//

        // public LoadingController Controller;

        private FadingState state = FadingState.None;
        

        void Awake()
        {
            this.gameObject.SetActive(true);
            
            loginMsg = new ReactiveProperty<string>(""); //"" is a magic value that is required to prevent showing the modal immediately on load.
            loginMsg.AsObservable().Subscribe(
                (message) =>
                {
                    text.text = message;
                }
                );

            CloseButton.onClick.AsObservable().Subscribe(_ => OnCloseBtnClick());

            if (text == null)
            {
                OpenMetaverse.Logger.Log("loading bar fail.", Helpers.LogLevel.Error);
            }

        }

        //completely wtf initialisation
        private void Start()
        {
            FadeOut();
        }


        private void OnCloseBtnClick()
        {
            //CloseButton.interactable = false;
            CloseButton.gameObject.SetActive(false);
            FadeOut();
            ResetLoadingText();
        }

        private void ResetLoadingText()
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
            StopAllCoroutines();
            state = FadingState.In;
            
            this.gameObject.SetActive(true);
            StartCoroutine(FadeInCoroutine());
            state = FadingState.None;
        }
        
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
        }
        public void FadeOut()
        {
            if (state == FadingState.Out)
            {
                return;
            }
            StopAllCoroutines();
            state = FadingState.Out;
            
            
            StartCoroutine(FadeoutCoroutine());

            this.gameObject.SetActive(false);
            state = FadingState.None;
        }
         
        IEnumerator FadeoutCoroutine()
        {
            float target = 0;
            float initial = canvas.alpha;
            // loop over 1 second
            for (float i = initial; i >= target; i -= Time.deltaTime)
            {
                // set color with i as alpha
                canvas.alpha = i;
                yield return null;
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
