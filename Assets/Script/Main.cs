using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        ABUpdateManager.Instance.DownLoadABCompareFile();
        ABUpdateManager.Instance.DownLoadABFlie((isOver) =>
        {
            if (isOver)
            {
                Debug.Log("所有资源传输完毕");
            }
            else
            {
                Debug.Log("网络有波动，请检测网络重新下载");
            }
        });
    }

}
