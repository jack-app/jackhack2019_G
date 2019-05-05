using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;

public class InstantiateText : MonoBehaviour
{
    public static GameObject Text;
    public static GameObject Canvas;
    public GameObject Text2;//ここにinstantiateするテキストを入れる
    public GameObject Canvas2;//ここに親となるCanvasを入れる
    // Start is called before the first frame update
    void Start()
    {
        Text = Text2;
        Canvas = Canvas2;
    }

    public static GameObject Instxt(RectTransform rt,OpenCvSharp.Rect r)
    {
        var newVec = new Vector2((float)r.X, -(float)r.Y) - new Vector2(rt.sizeDelta.x / 2.0f, -rt.sizeDelta.y / 2.0f);
        Debug.Log(newVec);
        GameObject a = GameObject.Instantiate(Text, Canvas.transform);
        a.transform.localPosition = (newVec + new Vector2((float)r.Width / 2.0f, 0)) *Canvas.transform.localScale.x;
        return a;
    }
}
