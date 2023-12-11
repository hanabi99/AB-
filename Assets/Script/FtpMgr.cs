using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class FtpMgr
{
    private static FtpMgr instance = new FtpMgr();
    public static FtpMgr Instance => instance;

    //远端FTP服务器的地址
    private string FTP_PATH = "ftp://10.0.18.39/AB/PC/";
    //用户名和密码
    private string USER_NAME = "tianhaochen";
    private string PASSWORD = "tian20010808";

    /// <summary>
    /// 上传文件到Ftp服务器（异步）
    /// </summary>
    /// <param name="fileName">FTP上的文件名</param>
    /// <param name="localPath">本地文件路径</param>
    /// <param name="action">上传完毕后想要做什么的委托函数</param>
    public async void UpLoadFile(string fileName, string localPath, UnityAction action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                //通过一个线程执行这里面的逻辑 那么就不会影响主线程了
                //1.创建一个Ftp连接
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                //2.进行一些设置
                //凭证
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                //是否操作结束后 关闭 控制连接
                req.KeepAlive = false;
                //传输类型
                req.UseBinary = true;
                //操作类型
                req.Method = WebRequestMethods.Ftp.UploadFile;
                //代理设置为空
                req.Proxy = null;
                //3.上传
                Stream upLoadStream = req.GetRequestStream();
                //开始上传
                using (FileStream fileStream = File.OpenRead(localPath))
                {
                    byte[] bytes = new byte[2048];
                    //返回值 为具体读取了多少个字节
                    int contentLength = fileStream.Read(bytes, 0, bytes.Length);
                    //有数据就上传
                    while (contentLength != 0)
                    {
                        //读了多少就写(上传)多少
                        upLoadStream.Write(bytes, 0, contentLength);
                        //继续从本地文件中读取数据
                        contentLength = fileStream.Read(bytes, 0, bytes.Length);
                    }
                    //上传结束
                    fileStream.Close();
                    upLoadStream.Close();

                }
                Debug.Log(fileName + "上传成功");
            }
            catch (Exception e)
            {
                Debug.Log("上传文件出错" + e.Message);
            }
        });
        //上传结束后 你想在外部做的事情
        action?.Invoke();
    }

    /// <summary>
    /// 下载文件从Ftp服务器当中（异步）
    /// </summary>
    /// <param name="fileName">FTP上想要下载的文件名</param>
    /// <param name="localPath">存储的本地文件路径</param>
    /// <param name="action">下载完毕后想要做什么的委托函数</param>
    public async void DownLoadFile(string fileName, string localPath, UnityAction action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                //1.创建一个Ftp连接
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                //2.进行一些设置
                //凭证
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                //是否操作结束后 关闭 控制连接
                req.KeepAlive = false;
                //传输类型
                req.UseBinary = true;
                //操作类型
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                //代理设置为空
                req.Proxy = null;
                //3.下载
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                Stream downLoadStream = res.GetResponseStream();
                //写入到本地文件中
                using (FileStream fileStream = File.Create(localPath))
                {
                    byte[] bytes = new byte[1024];
                    //读取数据
                    int contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                    //一点一点的写入
                    while (contentLength != 0)
                    {
                        //读多少 写多少
                        fileStream.Write(bytes, 0, contentLength);
                        //继续读
                        contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                    }
                    fileStream.Close();
                    downLoadStream.Close();
                }
                res.Close();
            }
            catch (Exception e)
            {
                Debug.Log("下载失败" + e.Message);
            }
        });

        //如果下载结束有想做的事情 在这里调用外部传入的委托函数
        action?.Invoke();
    }
    /// <summary>
    /// 下载文件从Ftp服务器当中（同步）
    /// </summary>
    /// <param name="fileName">FTP上想要下载的文件名</param>
    /// <param name="localPath">存储的本地文件路径</param>
    /// <param name="action">下载完毕后想要做什么的委托函数</param>
    public void DownLoadFile(string fileName, string localPath, out bool isOver, UnityAction action = null)
    {
        try
        {
            //1.创建一个Ftp连接
            FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
            //2.进行一些设置
            //凭证
            req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
            //是否操作结束后 关闭 控制连接
            req.KeepAlive = false;
            //传输类型
            req.UseBinary = true;
            //操作类型
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            //代理设置为空
            req.Proxy = null;
            //3.下载
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            Stream downLoadStream = res.GetResponseStream();
            //写入到本地文件中
            using (FileStream fileStream = File.Create(localPath))
            {
                byte[] bytes = new byte[1024];
                //读取数据
                int contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                //一点一点的写入
                while (contentLength != 0)
                {
                    //读多少 写多少
                    fileStream.Write(bytes, 0, contentLength);
                    //继续读
                    contentLength = downLoadStream.Read(bytes, 0, bytes.Length);
                }
                fileStream.Close();
                downLoadStream.Close();
            }
            res.Close();
            isOver = true;
        }
        catch (Exception e)
        {
            Debug.Log("下载失败" + e.Message);
            isOver = false;
        }

        //如果下载结束有想做的事情 在这里调用外部传入的委托函数
        action?.Invoke();
    }

    /// <summary>
    /// 移除指定的文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="action">移除过后想做什么的委托函数</param>
    public async void DeleteFile(string fileName, UnityAction<bool> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                //通过一个线程执行这里面的逻辑 那么就不会影响主线程了
                //1.创建一个Ftp连接
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                //2.进行一些设置
                //凭证
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                //是否操作结束后 关闭 控制连接
                req.KeepAlive = false;
                //传输类型
                req.UseBinary = true;
                //操作类型
                req.Method = WebRequestMethods.Ftp.DeleteFile;
                //代理设置为空
                req.Proxy = null;
                //3.真正的删除
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                res.Close();

                action?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.Log("移除失败" + e.Message);
                action?.Invoke(false);
            }
        });
    }


    /// <summary>
    /// 获取FTP服务器上某个文件的大小 （单位 是 字节）
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="action">获取成功后传递给外部 具体的大小</param>
    public async void GetFileSize(string fileName, UnityAction<long> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                //通过一个线程执行这里面的逻辑 那么就不会影响主线程了
                //1.创建一个Ftp连接
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
                //2.进行一些设置
                //凭证
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                //是否操作结束后 关闭 控制连接
                req.KeepAlive = false;
                //传输类型
                req.UseBinary = true;
                //操作类型
                req.Method = WebRequestMethods.Ftp.GetFileSize;
                //代理设置为空
                req.Proxy = null;
                //3.真正的获取
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                //把大小传递给外部
                action?.Invoke(res.ContentLength);

                res.Close();
            }
            catch (Exception e)
            {
                Debug.Log("获取大小失败" + e.Message);
                action?.Invoke(0);
            }
        });
    }


    /// <summary>
    /// 创建一个文件夹 在FTP服务器上
    /// </summary>
    /// <param name="directoryName">文件夹名字</param>
    /// <param name="action">创建完成后的回调</param>
    public async void CreateDirectory(string directoryName, UnityAction<bool> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                //通过一个线程执行这里面的逻辑 那么就不会影响主线程了
                //1.创建一个Ftp连接
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + directoryName)) as FtpWebRequest;
                //2.进行一些设置
                //凭证
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                //是否操作结束后 关闭 控制连接
                req.KeepAlive = false;
                //传输类型
                req.UseBinary = true;
                //操作类型
                req.Method = WebRequestMethods.Ftp.MakeDirectory;
                //代理设置为空
                req.Proxy = null;
                //3.真正的创建
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                res.Close();

                action?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.Log("创建文件夹失败" + e.Message);
                action?.Invoke(false);
            }
        });
    }

    /// <summary>
    /// 获取所有文件名
    /// </summary>
    /// <param name="directoryName">文件夹路径</param>
    /// <param name="action">返回给外部使用的 文件名列表</param>
    public async void GetFileList(UnityAction<List<string>> action = null)
    {
        await Task.Run(() =>
        {
            try
            {
                //通过一个线程执行这里面的逻辑 那么就不会影响主线程了
                //1.创建一个Ftp连接
                FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH)) as FtpWebRequest;
                //2.进行一些设置
                //凭证
                req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
                //是否操作结束后 关闭 控制连接
                req.KeepAlive = false;
                //传输类型
                req.UseBinary = true;
                //操作类型
                req.Method = WebRequestMethods.Ftp.ListDirectory;
                //代理设置为空
                req.Proxy = null;
                //3.真正的创建
                FtpWebResponse res = req.GetResponse() as FtpWebResponse;
                //把下载的信息流 转换成StreamReader对象 方便我们一行一行的读取信息
                StreamReader streamReader = new StreamReader(res.GetResponseStream());

                //用于存储文件名的列表
                List<string> nameStrs = new List<string>();
                //一行行的读取
                string line = streamReader.ReadLine();
                while (line != null)
                {
                    nameStrs.Add(line);
                    line = streamReader.ReadLine();
                }
                res.Close();

                action?.Invoke(nameStrs);
            }
            catch (Exception e)
            {
                Debug.Log("获取文件列表失败" + e.Message);
                action?.Invoke(null);
            }
        });
    }
    /// <summary>
    /// 获取所有文件名(同步)
    /// </summary>
    /// <param name="directoryName">文件夹路径</param>
    /// <param name="action">返回给外部使用的 文件名列表</param>
    public List<string> GetFileList()
    {
        try
        {
            //通过一个线程执行这里面的逻辑 那么就不会影响主线程了
            //1.创建一个Ftp连接
            FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH)) as FtpWebRequest;
            //2.进行一些设置
            //凭证
            req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
            //是否操作结束后 关闭 控制连接
            req.KeepAlive = false;
            //传输类型
            req.UseBinary = true;
            //操作类型
            req.Method = WebRequestMethods.Ftp.ListDirectory;
            //代理设置为空
            req.Proxy = null;
            //3.真正的创建
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            //把下载的信息流 转换成StreamReader对象 方便我们一行一行的读取信息
            StreamReader streamReader = new StreamReader(res.GetResponseStream());

            //用于存储文件名的列表
            List<string> nameStrs = new List<string>();
            //一行行的读取
            string line = streamReader.ReadLine();
            while (line != null)
            {
                nameStrs.Add(line);
                line = streamReader.ReadLine();
            }         
            res.Close();
            return nameStrs;
        }
        catch (Exception e)
        {
            Debug.Log("获取文件列表失败" + e.Message);
        }

        return null;
    }


    /// <summary>
    /// 移除指定的文件（同步）
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="action">移除过后想做什么的委托函数</param>
    public void DeleteFile(string fileName)
    {

        try
        {
            //通过一个线程执行这里面的逻辑 那么就不会影响主线程了
            //1.创建一个Ftp连接
            FtpWebRequest req = FtpWebRequest.Create(new Uri(FTP_PATH + fileName)) as FtpWebRequest;
            //2.进行一些设置
            //凭证
            req.Credentials = new NetworkCredential(USER_NAME, PASSWORD);
            //是否操作结束后 关闭 控制连接
            req.KeepAlive = false;
            //传输类型
            req.UseBinary = true;
            //操作类型
            req.Method = WebRequestMethods.Ftp.DeleteFile;
            //代理设置为空
            req.Proxy = null;
            //3.真正的删除
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            res.Close();
        }
        catch (Exception e)
        {
            Debug.Log("移除失败" + e.Message);
        }
    }
    
}

