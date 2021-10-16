#region "Copyright"
/*======================================================================
Filename :QianQianLrcer.cs
Description :

修改记录：
Created by 吕亮 at 2009-1-15 10:08:16
算法来源于网络
 
 * 修改记录：
 * by leomon. 2014-03-07.
======================================================================*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Metro.LyricDownloader
{
    /// <summary>
    /// 歌词下载类。
    /// </summary>
    public class MyLyricDownLoader
    {
        /// <summary>
        /// 字段：搜索路径
        /// </summary>
        public static readonly string SearchPath = "http://ttlrcct2.qianqian.com/dll/lyricsvr.dll?sh?Artist={0}&Title={1}&Flags=0";
        /// <summary>
        /// 字段：下载路径
        /// </summary>
        public static readonly string DownloadPath = "http://ttlrcct2.qianqian.com/dll/lyricsvr.dll?dl?Id={0}&Code={1}";
       /// <summary>
       /// 字段：是否需要开启代理下载
       /// </summary>
        private bool needProxy = false;
        /// <summary>
        /// 字段：Http代理
        /// </summary>
        private WebProxy proxy;
        /// <summary>
        /// 字段：歌曲xml
        /// </summary>
        private XmlNode currentSong;
        /// <summary>
        /// 属性：是否需要开启代理下载
        /// </summary>
        public bool NeedProxy
        {
            get
            {
                return needProxy;
            }
            set
            {
                needProxy = value;
            }
        }
        /// <summary>
        /// 属性：Http代理
        /// </summary>
        public WebProxy Proxy       
        {
            get {return proxy;}
            set {proxy = value;}
        }
        /// <summary>
        /// 属性：当前查询歌曲
        /// </summary>
        public XmlNode CurrentSong
        {
            get { return currentSong;}
            set {currentSong = value;}
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="needProxy">是否开启代理下载</param>
        public MyLyricDownLoader(bool needProxy)
        {
            this.NeedProxy = needProxy;
        }
        /// <summary>
        /// 方法：获取歌词的下载链接
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public string RequestALink(string link)
        {
            StringBuilder sb = new StringBuilder();
            WebRequest request = WebRequest.Create(link);
            if (this.NeedProxy)
            {
                if (Proxy != null)
                {
                    //初始化
                    this.OnInitilizeProxy();
                }
                request.Proxy = this.Proxy;
            }
            try
            {
                using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    sb.Append(sr.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                //异常引发
                this.OnWebException(ex);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 方法：下载歌词
        /// </summary>
        /// <param name="singer">歌手</param>
        /// <param name="title">歌名</param>
        /// <param name="downloadNow">是否下载</param>
        /// <returns></returns>
        public string DownloadLyric(XmlDocument xml)
        {
            string retStr = "没有找到该歌词";
            if (xml == null)
                return retStr;

            XmlNodeList list = xml.SelectNodes("/result/lrc");
            if (list.Count > 0)
            {
                if (this.currentSong == null)
                    this.currentSong = list[0];
            }
            else if (list.Count == 1)
            {
                this.currentSong = list[0];
            }
            else
            {
                return retStr;
            }
            if (this.currentSong == null)
                return retStr;

            XmlNode node = this.currentSong;
            
            int lrcId = -1;
            if (node != null && node.Attributes != null && node.Attributes["id"] != null)
            {
                string sLrcId = node.Attributes["id"].Value;
                if (int.TryParse(sLrcId, out lrcId))
                {
                    string xSinger = node.Attributes["artist"].Value;
                    string xTitle = node.Attributes["title"].Value;
                    retStr = RequestALink(string.Format(DownloadPath, lrcId,
                        EncodeHelper.CreateQianQianCode(xSinger, xTitle, lrcId)));
                }
            }
            return retStr;
        }

        public event EventHandler InitializeProxy;
        public event EventHandler SelectSong;
        public event EventHandler WebException;
        /// <summary>
        /// 当初始化代理设置时触发
        /// </summary>
        protected void OnInitilizeProxy()
        {
            if (this.InitializeProxy != null)
            {
                this.InitializeProxy(this, new EventArgs());
            }
        }
        /// <summary>
        /// 当发生网络请求错误时发生
        /// </summary>
        /// <param name="ex"></param>
        protected void OnWebException(WebException ex)
        {
            if(this.WebException != null)
            {
                this.WebException(ex, new EventArgs());
            }
            else
            {
                throw ex;
            }
        }
        /// <summary>
        /// 当选择歌曲节点时触发
        /// </summary>
        /// <param name="list"></param>
        protected void OnSelectSong(XmlNodeList list)
        {
            if (this.SelectSong != null)
            {
                this.SelectSong(list, new EventArgs());
            }

        }

        /// <summary>
        /// 获得有关歌词的xml待下载目录
        /// </summary>
        /// <param name="singer"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public XmlDocument SearchLyric(string singer,string title)
        {
            XmlDocument xml = new XmlDocument();
            singer = singer.ToLower().Replace(" ", "").Replace("'", "");
            title = title.ToLower().Replace(" ", "").Replace("'", "");
            string x = RequestALink(string.Format(SearchPath, EncodeHelper.ToQianQianHexString(singer, Encoding.Unicode),
                EncodeHelper.ToQianQianHexString(title, Encoding.Unicode)));
            try
            {
                xml.LoadXml(x);
                XmlNodeList list = xml.SelectNodes("/result/lrc");
                OnSelectSong(list);
            }
            catch {}
            return xml;
        }
    }
}
