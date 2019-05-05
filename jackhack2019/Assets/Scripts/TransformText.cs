using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;

public class TransformText : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public static void Trnstxt(GameObject a,RectTransform rt,OpenCvSharp.Rect r)
    {
        var newVec = new Vector2(r.X,-r.Y) - new Vector2(rt.sizeDelta.x / 2.0f, -rt.sizeDelta.y / 2.0f);
        Debug.LogFormat("r : {0}, sizeDelta : {1}",new Vector2(r.X,r.Y),new Vector2(rt.sizeDelta.x,rt.sizeDelta.y));
        //a.transform.localPosition = (newVec);
         a.transform.localPosition = (newVec + new Vector2((float)r.Width / 2.0f, 0)) * InstantiateText.Canvas.transform.localScale.x;
    }
}
