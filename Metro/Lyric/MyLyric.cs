using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Metro.Lyric
{
    /// <summary>
    /// 该类表示歌词的相关信息。
    /// </summary>
    public class MyLyric
    {
        #region 歌词相关的信息定义 字段 属性
        private string artist = "";
        private string song = "";
        private string album = "";
        private string author = "";

        private double offset = 0;

        private List<string> lyricTextLines = new List<string>();
        private List<string> lyricTimeLines = new List<string>();
        private List<double> lyricTimeLinesDValue = new List<double>();

        public string Artist
        {
            get { return artist; }
        }

        public string SongName
        {
            get { return song; }
        }

        public string Album
        {
            get { return album; }
        }

        public string Author
        {
            get { return author; }
        }

        public double Offset
        {
            get
            {
                return offset;
            }
        }

        public List<string> LyricTextLines
        {
            get { return lyricTextLines; }
        }

        public List<string> LyricTimeLines
        {
            get { return lyricTimeLines; }
        }

        public List<double> LyricTimeLinesDValue
        {
            get
            {
                return lyricTimeLinesDValue;
            }
        }
        #endregion

        #region Functions
        public MyLyric() { }

        public MyLyric(string lyricPath, Encoding encoding)
        {
            string fileString = OpenLyricFile(lyricPath, encoding);
            AnalyzeLyricFile(fileString);
        }

        public MyLyric(string lrcStr)
        {
            AnalyzeLyricFile(lrcStr);
        }

        public void AnalyzeLyricFile(string fileString)
        {
            StartAnalyzeLyric(fileString);
            for (int i = 0; i < LyricTimeLines.Count; i++)
            {
                LyricTimeLines[i] += '\n' + LyricTextLines[i];
            }
            LyricTimeLines.Sort();
            for (int i = 0; i < LyricTimeLines.Count; i++)
            {
                string[] splitStr =  LyricTimeLines[i].Split('\n');
                LyricTimeLines[i] = splitStr[0];
                ConvertTimeToDoubleValue(splitStr[0]);
                LyricTextLines[i] = splitStr[1];
            }
        }

        #endregion

        #region Lyric
        private string OpenLyricFile(string lyricPath, Encoding encoding)
        {
            string tempStr = "";
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }
            using (StreamReader sr = new StreamReader(lyricPath, encoding))
            {
                try
                {
                    tempStr = sr.ReadToEnd();
                }
                catch (DirectoryNotFoundException e)
                {
                    tempStr = "";
                    throw new DirectoryNotFoundException(e.Message);
                }
                catch (FileNotFoundException e)
                {
                    tempStr = "";
                    throw new FileNotFoundException(e.Message);
                }
                catch (Exception e)
                {
                    tempStr = "";
                    throw new Exception(e.Message);
                }
            }
            return tempStr;
        }

        private void StartAnalyzeLyric(string fileString)
        {
            string[] stringLine;
            stringLine = fileString.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < stringLine.Length; i++)
            {
                if (stringLine[i].Contains('['))    
                {
                    string tempStr = stringLine[i].Trim();
                    //Console.WriteLine(tempStr);
                    #region Credits
                    if (tempStr.Contains("[ar:"))
                    {
                        this.artist = GetSongInformation(tempStr);
                    }
                    else if (tempStr.Contains("[ti:"))
                    {
                        this.song = GetSongInformation(tempStr);
                    }
                    else if (tempStr.Contains("[al:"))
                    {
                        this.album = GetSongInformation(tempStr);
                    }
                    else if (tempStr.Contains("[by:"))
                    {
                        this.author = GetSongInformation(tempStr);
                    }
                    #endregion

                    else
                    {
                        if (tempStr.Contains("offset"))
                        {
                            if (!double.TryParse(GetSongInformation(tempStr), out this.offset))
                            {
                                this.offset = 0;
                            }
                        }
                        else
                        {
                            GetLyricContentAndTime(tempStr);
                        }
                    }
                }
            }
        }


        private void GetLyricContentAndTime(string tempStr)
        {
            string[] lineStr = tempStr.Split(']');
            int indexM = lineStr.Length;
            for (int i = 0; i < indexM - 1; i++)
            {
                string time = lineStr[i].Substring(1);
                if (time.Contains('.'))
                    time = time.Replace('.', ':');
                if (time[1] >= '0' && time[1] <= '9')
                {
                    LyricTimeLines.Add(time);
                    if (lineStr[indexM - 1].Trim() == "")
                    {
                        LyricTextLines.Add("");
                    }
                    else
                    {
                        LyricTextLines.Add(lineStr[indexM - 1]);
                    }
                }
            }
        }

        /// <summary>
        /// 返回ms
        /// </summary>
        /// <param name="time"></param>
        private void ConvertTimeToDoubleValue(string time)
        {
            string[] t = time.Split(':');
            double dTime = 0;
            if (t.Length == 2)
            {
                //分
                int min = 0,
                    sec = 0;    //秒
                if (int.TryParse(t[0], out min) && int.TryParse(t[1], out sec))
                {
                    //保证全部得以转换方可以保存
                    dTime = (min * 60 + sec * 1) * 1000.0;    //单位是秒，没有精确到ms
                }
            }
            else
            {
                int min = 0,    //分
                    sec = 0,    //秒
                    mill = 0;   //毫秒
                if (int.TryParse(t[0], out min) &&
                    int.TryParse(t[1], out sec) &&
                    int.TryParse(t[2], out mill))
                {
                    dTime = (min * 60 + sec * 1) * 1000.0 + mill;
                }
            }
            lyricTimeLinesDValue.Add(dTime);
        }

        private string GetSongInformation(string tempStr)
        {
            int index = tempStr.IndexOf(":") + 1;
            string str = tempStr.Substring(index, tempStr.Length - index - 1);
            return str;
        }
        /// <summary>
        /// 获取文件的编码方式
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        private Encoding GetEncoding(string fileName)
        {
            Encoding en = Encoding.Default;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                en = GetEncoding(fs);
                fs.Close();
            }
            catch
            {
                en = Encoding.Default;
            }
            return en;
        }
        /// <summary>
        /// 获取文件流的编码方式
        /// </summary>
        /// <param name="fs">文件流</param>
        /// <returns></returns>
        private Encoding GetEncoding(FileStream fs)
        {
            Encoding en = Encoding.Default;
            try
            {
                BinaryReader br = new BinaryReader(fs, Encoding.Default);
                byte[] buffer = br.ReadBytes(2);
                br.Close();
                //开始判断编码类型
                if (buffer[0] >= 0xEF)
                {
                    if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                    {
                        en = Encoding.UTF8;
                    }
                    else if (buffer[0] == 0xEF && buffer[1] == 0xFF)
                    {
                        en = Encoding.BigEndianUnicode;
                    }
                    else
                    {
                        en = Encoding.Default;
                    }
                }
                else
                {
                    en = Encoding.Default;
                }
            }
            catch
            {
                en = Encoding.Default;
            }
            return en;
        }
        #endregion
    }
}
