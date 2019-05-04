using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leftburntime : MonoBehaviour
{
    private Vector3 nikuposition;//焼肉の位置
    public static Text Timertext ;//残り焼き時間表示
    // Start is called before the first frame update
    void Start()
    {
        Timertext.text = "";
    }

    //焼肉に残り時間を表示する
    public static void Yakizikan(Vector3 nikuposition)
    {
        Instantiate(Timertext,nikuposition,Quaternion.identity);
    }


    
}
