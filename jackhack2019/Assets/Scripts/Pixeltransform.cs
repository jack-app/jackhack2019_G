using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pixeltransform : MonoBehaviour
{
    //canvasのカメラの座標
    static Camera CanvasCamera;

    // Start is called before the first frame update
    void Start()
    {
        CanvasCamera = GetComponent<Canvas>().worldCamera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// pixel座標をワールド座標に変換します
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static Vector3 Getworldpositionfromrectposition(RectTransform rect)
    {
        //UI座標からスクリーン座標に変換
        Vector2 screenpos = RectTransformUtility.WorldToScreenPoint(CanvasCamera,rect.position);

        //代入する用のワールド座標
        Vector3 result = Vector3.zero;

        //スクリーン座標からワールド座標に変換
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rect,screenpos,CanvasCamera,out result);

        return result;
    }

}
