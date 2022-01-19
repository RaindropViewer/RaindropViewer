using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSizeManager : MonoBehaviour
{
    public SpriteRenderer s;
    private float targetHeight = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        s = this.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var bounds = s.sprite.bounds;
        var factor = targetHeight / bounds.size.y;
        transform.localScale = new Vector3(factor, factor, factor);


    }
}
