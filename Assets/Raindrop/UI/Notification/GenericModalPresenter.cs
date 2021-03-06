using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

//attach this to a generic modal.
//a monobehaviour that provides logic to the modal.
public class GenericModalPresenter : MonoBehaviour
{
    /*
    +--------------------+
    +    Title text      +
    +--------------------+
    +    content text    +
    +    content text    +
    +    content text    +
    +--------------------+
    + [at1] [at2] [at3]  +
    +--------------------+
    
    where [button name] 
     
    */
    public TMP_Text titletext;
    public TMP_Text contenttext;
    public TMP_Text actionText;
    
    public Button CloseButton;
    public Button BackgroundButton;

    public CanvasRenderer renderer;

    private void Awake()
    {
        LinkUIComponents();

        void LinkUIComponents()
        {
            if (CloseButton != null)
            {
                CloseButton.onClick.AsObservable().Subscribe(_ => CloseModal()); //when clicked, runs this method.
            }
            else
            {
                Debug.LogError("CloseButton failed");
            }

            if (BackgroundButton != null)
            {
                BackgroundButton.onClick.AsObservable().Subscribe(_ => CloseModal()); //when clicked, runs this method.
            }

            if (titletext.GetComponent<TMP_Text>() == null)
            {
                Debug.LogError("titletext.GetComponent<TMP_Text>() failed");
            }

            if (contenttext.GetComponent<TMP_Text>() == null)
            {
                Debug.LogError("contenttext.GetComponent<TMP_Text>() failed");
            }
        }
    }

    //sets the textual contents of the ui
    public void SetModal(string title, string content, string actionText)
    {
        titletext.GetComponent<TMP_Text>().text = title;
        contenttext.GetComponent<TMP_Text>().text = content;
        if (actionText == null)
        {
            this.actionText.gameObject.SetActive(false);
        }
        else
        {
            this.actionText.gameObject.SetActive(true);
            this.actionText.GetComponent<TMP_Text>().text = actionText;
        }
    }
    
    //special form of setModal
    public void SetModalNoActions(string title, string content)
    {
        SetModal(title, content, null);
    }

    public void CloseModal()
    {
        gameObject.SetActive(false);
        Destroy(this.gameObject);
    }
}
