using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        ABUpdateManager.Instance.CheckUpdate((isOver) =>
        {
            if (isOver)
            {
                Debug.Log("Update Finish");
            }
            else
            {
                Debug.Log("Update Faile");
            }


        }, (str) =>
         {
             print(str);
         });
        Debug.Log(Application.persistentDataPath);
        // ABUpdateManager.Instance.GetLocalABCompareFileInfo();
        //ABUpdateManager.Instance.DownLoadABCompareFile((isover) => {

        //    if (isover)
        //    {

        //    } 

        //});
        //ABUpdateManager.Instance.DownLoadABFlie((isOver) =>
        //{
        //    if (isOver)
        //    {
        //        Debug.Log("所有资源传输完毕");
        //    }
        //    else
        //    {
        //        Debug.Log("网络有波动，请检测网络重新下载");
        //    }
        //});
    }

}
