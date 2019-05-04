using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timerscript : MonoBehaviour
{
    static Text Timertext;//残り焼き時間表示
    public static int time;//残り焼き時間
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Timer");
    }

    private IEnumerator Timer()
    {
        while (time > 0)
        {
            yield return new WaitForSeconds(1f);
            time -= 1;
            int minute = time / 60;//残り時間(分)
            if (minute >= 0)
            {
                Timertext.text = "残り" + minute + "分";
            }
            else
            {
                Timertext.text = "残り" + time + "秒";
            }

        }
        if(time <= 0)
        {
            Debug.Log("上手に焼けました!");
        }
    }
}
