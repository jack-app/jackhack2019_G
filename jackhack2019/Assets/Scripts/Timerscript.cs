using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timerscript : MonoBehaviour
{
    //static Text Timertext;//残り焼き時間表示
    public int time;//残り焼き時間
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Timer");
    }

    private IEnumerator Timer()
    {
        while (time > 0)
        {
            time -= 1;
            int minute = time / 60;//残り時間(分)
            if (minute > 0)
            {
                if(GetComponent<Text>().text != null)
                {
                    int tmptime = time - minute * 60;
                    GetComponent<Text>().text = "残り" + minute + "分" + tmptime + "秒";
                }
                
            }
            else
            {
                if (GetComponent<Text>().text != null)
                {
                    GetComponent<Text>().text = "残り" + time + "秒";
                }
                
            }
            yield return new WaitForSeconds(1f);

        }
        if(time <= 0)
        {
            Debug.Log("上手に焼けました!");
        }


    }
}
