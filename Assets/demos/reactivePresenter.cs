using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
// Presenter for scene(canvas) root.
public class ReactivePresenter : MonoBehaviour
{
    // Presenter is aware of its View (binded in the inspector)
    public Button MyButton;
    public Toggle MyToggle;
    public Text MyText;

    // State-Change-Events from Model by ReactiveProperty
    Enemy enemy = new Enemy(1000);


    void Start()
    {
        // Rx supplies user events from Views and Models in a reactive manner 
        MyButton.OnClickAsObservable().Subscribe(_ => enemy.CurrentHp.Value -= 99); // subscribe to the onclick() event abd fire this callback method when the event is raised
        MyToggle.OnValueChangedAsObservable().SubscribeToInteractable(MyButton); //subscribed to the onvaluechanged() event of this toggle. set interactable of 'Mybutton' to same as the value in the event. 

        // Models notify Presenters via Rx, and Presenters update their views
        enemy.CurrentHp.SubscribeToText(MyText); //subscribe to the current hp paramter in model. when it change, update mytext paramteter to match.
        enemy.IsDead.Where(isDead => isDead == true) //subscribe to the isdead parameter in model. if isDead is true in the event, run the lambda callback method 'interactable of my button to false.' 
            .Subscribe(_ =>
            {
                MyToggle.interactable = MyButton.interactable = false;
            });
    }
}