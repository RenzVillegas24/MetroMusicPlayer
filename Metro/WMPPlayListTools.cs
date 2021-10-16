//
// WMPPlayListTool - Simple Windows Media Player Playlist Controller.
// Copyright (C) 2009-2013 John Janssen
// http://www.travelisfun.net 
//
// This program is free software; you can redistribute it and/or modify
// it any way you want as long as the above copyright statement is maintained.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using Microsoft.Win32;
using System.Windows;

namespace WPLControlLib
{
  /// <summary>
  /// A media source entry in the Playlist.
  /// Sample entry.
  /// <!-- <media src="..\07-All of us.mp3" cid="{83858B95-435B-4113-8044-958113F83871}" tid="{D8F888AE-DEE8-4258-93EE-641FFD2D12BD}"/>  -->
  /// </summary>
  public class WMPPlayListEntry
  {
    //
    private String mediaSource;
    public String src
    {
      get { return mediaSource; }
      set
      {
        mediaSource = value;
        int index;
        if ((index = mediaSource.LastIndexOf(Path.DirectorySeparatorChar)) > 0)
        {
          // get the track/file name from the source string.
          Filename = mediaSource.Substring(index + 1);
        }
        else
          Filename = String.Empty;
      }
    }
    private String trackTitle = null;
    public String Tracktitle
    {
      get { return GetTrackTitle(); }
    }
    public String Filename { get; private set; }
    public String cid { get; private set; }
    public String tid { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="mediaFile"></param>
    public WMPPlayListEntry(String mediaFile)
    {
      src = mediaFile;
      cid = null;
      tid = null;
    }
    /// <summary>
    /// Basic constructor that reads from the input WPL file.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="reader"></param>
    public WMPPlayListEntry(XmlReader reader)
    {
      src = reader.GetAttribute("src");
      cid = reader.GetAttribute("cid");
      tid = reader.GetAttribute("tid");
    }
    /// <summary>
    /// 
    /// </summary>
    public void ClearTIDSIDAttributes()
    {
      cid = null;
      tid = null;
    }
    /// <summary>
    /// Get the tracktitle from the MP3 data.
    /// </summary>
    /// <returns></returns>
    public String GetTrackTitle()
    {
      if (trackTitle != null)
        return trackTitle;

      String filepath = WMPPlayList.BuildMediaPath(src);
      if (!File.Exists(filepath))
      {
        trackTitle = Path.GetFileName(filepath);
      }
      else
      {
        String ext = Path.GetExtension(filepath);

        if (String.Compare(ext, ".mp3", true) == 0)
        {
           TagLib.File tags = TagLib.File.Create(filepath);
          if (tags != null)
            trackTitle = tags.Tag.Title;
          else
            trackTitle = Path.GetFileName(filepath);
        }
        else
        {
          trackTitle = Path.GetFileName(filepath);
        }
      }
      return trackTitle;
    }
  }
  /// <summary>
  /// Case insensitive string comparer for use in the double remove.
  /// </summary>
  class StringComparer : IEqualityComparer<String>
  {
    public Boolean Equals(String s1, String s2)
    {
      return (String.Compare(s1, s2, true) == 0);
    }
    public int GetHashCode(String s)
    {
      return s.GetHashCode();
    }
  }
  /// <summary>
  /// 
  /// </summary>
  sealed class WMPPlayListBody : List<WMPPlayListEntry>
  {
    public WMPPlayListBody()
    {
    }
    public WMPPlayListBody(XmlReader reader)
    {
      Read(reader);
    }
    /// <summary>
    /// get the media item count.
    /// </summary>
    public int ItemCount
    {
      get { return this.Count; }
    }

    /// <summary>
    /// Loads a body section from a WM file into the instance. 
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public Boolean Read(XmlReader reader)
    {
      this.Clear(); // Clear any previous items
      try
      {
        while (reader.Read())
        {
          if (WMPPlayList.AbortRequested)
          {
            this.Clear();
            return false;
          }
          if (reader.NodeType == XmlNodeType.Element && reader.Name == "media")
          {
            this.Add(new WMPPlayListEntry(reader));
            reader.Read(); // read over the end element 
          }
          else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "seq")
          {
            break;
          }
        }
      }
      catch
      {
        return false;
      }
      return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    public Boolean Write(XmlWriter writer)
    {
      writer.WriteStartElement("body"); // <body>
      writer.WriteStartElement("seq"); // <seq>

      foreach (WMPPlayListEntry item in this)
      {
        if (WMPPlayList.AbortRequested)
        {
          // This will leave the WPL file in a unfinished state.
          // To prevent original file being corrupted the writer
          // should make a temp file and only when all data
          // is written to the remp should that temp file be
          // copied over the original.
          return false;
        }

        writer.WriteStartElement("media");
        writer.WriteAttributeString("src", item.src);
        if (!String.IsNullOrEmpty(item.cid))
          writer.WriteAttributeString("cid", item.cid);
        if (!String.IsNullOrEmpty(item.tid))
          writer.WriteAttributeString("tid", item.tid);
        writer.WriteEndElement(); // </media>
      }
      writer.WriteEndElement(); // </seq>
      writer.WriteEndElement(); // </body>
      return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="removeType"></param>
    /// <returns>Returns the number of items in the modified playlist.</returns>
    public int RemoveDoubles(WMPRemoveDoubleType removeType)
    {
      StringComparer stringComp = new StringComparer();
      // create temporary dictionary.
      Dictionary<String, WMPPlayListEntry> list = new Dictionary<String, WMPPlayListEntry>(stringComp);

      foreach (WMPPlayListEntry item in this)
      {
        if (WMPPlayList.AbortRequested)
          return 0;

        if (removeType == WMPRemoveDoubleType.BasedOnLocation)
        {
          if (!list.ContainsKey(item.src))
            list.Add(item.src, item);
        }
        else if (removeType == WMPRemoveDoubleType.BasedOnFilename)
        {
          if (!list.ContainsKey(item.Filename))
            list.Add(item.Filename, item);
        }
        else if (removeType == WMPRemoveDoubleType.BasedOnTrackTitle)
        {
          if (!list.ContainsKey(item.Tracktitle))
            list.Add(item.Tracktitle, item);
        }
      }

      // clear the list.
      this.Clear();
      foreach (KeyValuePair<String, WMPPlayListEntry> item in list)
      {
        this.Add(item.Value);
      }
      return this.Count;
    }
    /// <summary>
    /// Removes the TID and SID attributes from the media entries.
    /// </summary>
    /// <returns>Returns the number of items in the collection.</returns>
    public int RemoveTID()
    {
      foreach (WMPPlayListEntry item in this)
      {
        item.ClearTIDSIDAttributes();
      }
      return this.Count;
    }

    /// <summary>
    /// Removes those entries in the media list that do not exist as files.
    /// </summary>
    /// <returns>Returns the number of items in the collection.</returns>
    public int RemoveNonFiles()
    {
      try
      {
        List<WMPPlayListEntry> removeList = new List<WMPPlayListEntry>();
        foreach (WMPPlayListEntry item in this)
        {
          if (WMPPlayList.AbortRequested)
            return 0;
          
          String filename = WMPPlayList.BuildMediaPath(item.src);
          if (!File.Exists(filename))
          {
            // Do not modify the collection while in a foreach loop!
            // Creating an removal list.
            removeList.Add(item);
          }
        }
        // because this is a very fast process there is no need for AbortRequested check.
        foreach (WMPPlayListEntry item in removeList)
        {
          this.Remove(item);
        }
      }
      catch (Exception exp)
      {
        MessageBox.Show(exp.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      return this.Count;
    }
    /// <summary>
    /// Shuffles the playlist
    /// </summary>
    public void Shuffle()
    {
      Random rand = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);
      List<int> randList = new List<int>();  // contains indexes into the playlist

      while (randList.Count != this.Count)
      {
        int index;
        do
        {
          // Find a next index that is not already assigned.
          index = rand.Next(this.Count);
        } while (randList.Contains(index));
        randList.Add(index);
      }
      // Create a shuffled copy of the current list.
      List<WMPPlayListEntry> list = new List<WMPPlayListEntry>();
      foreach (int index in randList)
      {
        list.Add(this[index]);
      }
      this.Clear();
      this.AddRange(list); // copy the shuffled list back into the current playlist.
    }
  }
  /// <summary>
  /// Small class to hold a meta item from the WPL header.
  /// </summary>
  sealed class WMPMetaItem
  {
    public WMPMetaItem(string name, string content)
    {
      Name = name;
      Content = content;
    }
    public String Name { get; set; }
    public String Content { get; set; }
  }
  /// <summary>
  /// The header in a WPL Playlist file.
  /// This class contains a reference to the WMPPlayList class that holds
  /// this header class, because we need access to the itemcount of the
  /// playlist which is in the body, which is accessible using WMPPlyList.
  /// </summary>
  sealed class WMPPlayListHeader
  {
    private WMPPlayList parent = null;
    private List<WMPMetaItem> metaItems = new List<WMPMetaItem>();
    public String Title { get; set; }
    public String Author { get; set; }

    public WMPPlayListHeader(WMPPlayList parent)
    {
      this.parent = parent;
      Title = String.Empty;
      Author = String.Empty;
    }
    public WMPPlayListHeader(WMPPlayList parent, XmlReader reader)
    {
      this.parent = parent;
      Title = String.Empty;
      Author = String.Empty;
      Read(reader);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns>The found metadata or null.</returns>
    private WMPMetaItem FindMetaItem(String name)
    {
      foreach (WMPMetaItem item in metaItems)
      {
        if (item.Name == name)
          return item;
      }
      return null;
    }
    /// <summary>
    /// To maintain syncronisation between the amount of items in the medialist
    /// and the ItemCount value in the metadata the callback to the parent object 
    /// is created.
    /// </summary>
    public int ItemCount
    {
      get { return parent.ItemCount; }
      set
      {
        WMPMetaItem item = FindMetaItem("ItemCount");
        if (item != null)
        {
          item.Content = value.ToString();
        }
        else
        {
          metaItems.Add(new WMPMetaItem("ItemCount", value.ToString()));
        }
      }
    }
    /// <summary>
    /// Write out a Playlist to a WPL file.
    /// </summary>
    /// <param name="writer"></param>
    public Boolean Write(XmlWriter writer)
    {
      // Update the ItemCount value in the metadata.
      ItemCount = parent.ItemCount;

      // <head>
      writer.WriteStartElement("head");

      // <meta>
      foreach (WMPMetaItem item in metaItems)
      {
        writer.WriteStartElement("meta");
        writer.WriteAttributeString("name", item.Name);
        if (!String.IsNullOrEmpty(item.Content))
        {
          writer.WriteAttributeString("content", item.Content);
        }
        writer.WriteEndElement(); // end of <metadata>
      }
      // </meta>

      // <author>
      if (!String.IsNullOrEmpty(Author))
      {
        writer.WriteStartElement("author");
        writer.WriteValue(Author);
        writer.WriteEndElement();
      }
      // </author>

      // <title>
      writer.WriteStartElement("title");
      if (!String.IsNullOrEmpty(Title))
        writer.WriteValue(Title);
      writer.WriteEndElement();
      // </title>

      writer.WriteEndElement();
      // </head>

      return true; // the header is stored correctly
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    public void Read(XmlReader reader)
    {
      /*
       * Sample Header.
      <head>
        <meta name="Generator" content="Microsoft Windows Media Player -- 12.0.7601.17514"/>
        <meta name="ItemCount" content="151"/>
        <meta name="ContentPartnerListID"/>
        <meta name="ContentPartnerNameType"/>
        <meta name="ContentPartnerName"/>
        <meta name="Subtitle"/>
        <author/>
        <title>Preparty</title>
       </head> 
       */
      metaItems.Clear(); // clear any previous items.
      Author = null;
      Title = null;
      while (reader.Read())
      {
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "meta")
          {
            if (reader.HasAttributes)
            {
              String name = reader.GetAttribute("name");
              String content = reader.GetAttribute("content");
              metaItems.Add(new WMPMetaItem(name, content));
            }
          }
          else if (reader.Name == "author")
          {
            reader.Read();
            if (reader.NodeType == XmlNodeType.Text)
            {
              Author = reader.Value;
            }
          }
          else if (reader.Name == "title")
          {
            reader.Read();
            if (reader.NodeType == XmlNodeType.Text)
            {
              Title = reader.Value;
            }
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "head")
        {
          // we are done with reading the header.
          break;
        }
      }
    }
  }
  /// <summary>
  /// 
  /// </summary>
  public enum WMPRemoveDoubleType
  {
    [Description("Remove doubles based on the track name in the metadata.")]
    BasedOnTrackTitle = 0,
    [Description("Remove doubles based on the location, the filepath is used.")]
    BasedOnLocation,
    [Description("Remove doubles base on the filename of a track, case insensitive.")]
    BasedOnFilename,
  }
  /// <summary>
  /// 
  /// </summary>
  public class WMPPlayList
  {
    private static String WMPSavePlaylistPath = null;
    private int playListIndexer;
    private WMPPlayListHeader header = null;
    private WMPPlayListBody body = new WMPPlayListBody();
    public String Filename { get; private set; }

    private static ManualResetEvent abortOnSetEvent = new ManualResetEvent(false);

    /// <summary>
    /// Call this to terminate all ongoing playlist processes.
    /// </summary>
    public static void RequestAbort()
    {
      abortOnSetEvent.Set();
    }
    public static void CancelAbortRequest()
    {
      abortOnSetEvent.Reset();
    }
    /// <summary>
    /// Because true is the AbortOnSetEvent is set and active.
    /// Otherwise returns false.
    /// </summary>
    public static Boolean AbortRequested
    {
      get { return abortOnSetEvent.WaitOne(0); }
    }
    public WMPPlayList()
    {
      header = new WMPPlayListHeader(this);
    }
    public WMPPlayList(String filename)
    {
      header = new WMPPlayListHeader(this);
      Load(filename);
    }

    public String Title
    {
      set { header.Title = value; }
      get { return header.Title; }
    }

    public String Author
    {
      set { header.Author = value; }
      get { return header.Author; }
    }
    /// <summary>
    /// Get First media entry in the playlist, or null.
    /// Example:
    /// WMPPlayList pl = new WMPPlayList(filename);
    /// WMPPlayListEntry item = pl.GetFirst();
    /// while (item)
    /// {
    ///   // do something with item.
    ///   item = pl.GetNext();
    /// }
    /// </summary>
    /// <returns></returns>
    public WMPPlayListEntry GetFirst()
    {
      playListIndexer = 0;
      if (body.ItemCount > 0)
        return body[0];
      return null;
    }
    /// <summary>
    /// Get the next entry in the playlist, or null.
    /// </summary>
    /// <returns></returns>
    public WMPPlayListEntry GetNext()
    {
      ++playListIndexer;
      if (body.ItemCount > playListIndexer)
        return body[playListIndexer];
      return null;
    }
    /// <summary>
    ///  gets the number of media items in the playlist.
    /// </summary>
    public int ItemCount
    {
      get { return body.ItemCount; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static String GetPlayListSavePath()
    {
      if (WMPSavePlaylistPath == null)
      {
        try
        {
          RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\MediaPlayer\Player\Settings", false);
          if (key != null)
          {
            WMPSavePlaylistPath = (String)key.GetValue("SaveAsDir", "");
          }
        }
        catch { }
        // The registry entry doesn't have to be set, 
        // use the default, that is in the English language version: %My Music%\Playlists
        // In all their wisdom, Microsoft translates system folder names??? 
        // So other languages have to be figured out.
        if (String.IsNullOrEmpty(WMPSavePlaylistPath))
        {
          WMPSavePlaylistPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) +
            Path.DirectorySeparatorChar +
            "Playlists";
        }
      }
      return WMPSavePlaylistPath;
    }
    /// <summary>
    /// Media paths in the playlist are relative accoording to
    /// the location of the playlist save path. (Very weird decisions at Microsoft).
    /// </summary>
    /// <param name="mediafilename"></param>
    /// <returns></returns>
    public static String BuildMediaPath(String mediafilename)
    {
      if (String.IsNullOrEmpty(mediafilename) || mediafilename.Length < 3)
        return String.Empty;

      if (mediafilename[0] == '.' && mediafilename[1] == '.')
      {
        return Path.GetFullPath(GetPlayListSavePath() + Path.DirectorySeparatorChar + mediafilename);
      }
      else
      {
        return mediafilename;
      }
    }
    /// <summary>
    /// Add a mediafile to the playlist.
    /// </summary>
    /// <param name="mediaFile"></param>
    /// <returns></returns>
    public WMPPlayListEntry Add(String mediaFile)
    {
      WMPPlayListEntry item = new WMPPlayListEntry(mediaFile);
      body.Add(item);
      return item;
    }
    /// <summary>
    /// Remove a media entry item from the list.
    /// </summary>
    /// <param name="item"></param>
    public void Remove(WMPPlayListEntry item)
    {
      body.Remove(item);
    }

    /// <summary>
    /// Remove a media entry item index from the list.
    /// </summary>
    /// <param name="item"></param>
    public void RemoveFromIndex(int item)
    {
        body.RemoveAt(item);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public Boolean Load(String filename)
    {
      XmlReader reader = null;
      Filename = filename;
      try
      {
        reader = XmlReader.Create(filename);

        Boolean in_smil = false;
        Boolean in_body = false;
        while (reader.Read())
        {
          if (WMPPlayList.AbortRequested)
            return false;

          if (reader.NodeType == XmlNodeType.Element && reader.Name == "smil")
          {
            in_smil = true;
          }
          else if (in_smil && reader.NodeType == XmlNodeType.Element && reader.Name == "head")
          {
            header.Read(reader);
          }
          else if (in_smil && reader.NodeType == XmlNodeType.Element && reader.Name == "body")
          {
            in_body = true;
          }
          else if (in_smil && in_body && reader.NodeType == XmlNodeType.Element && reader.Name == "seq")
          {
            body.Read(reader);
            break; // just end it here.. no more data to be read.
          }
          else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "body")
          {
            in_body = false;
          }
          else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "smil")
          {
            in_smil = false;
          }
        }
      }
      catch (Exception exp)
      {
        MessageBox.Show(exp.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }
      finally
      {
        if (reader != null)
          reader.Close();
      }
      return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public Boolean Store(String filename, String title)
    {
      XmlWriter writer = null;
      String tempFilename = null;
      try
      {
        if (WMPPlayList.AbortRequested)
          return false;

        if (header != null && body != null)
        {
          header.ItemCount = body.ItemCount;
          if (!String.IsNullOrEmpty(title))
            header.Title = title;

          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Indent = true;
          settings.IndentChars = " ";
          settings.NewLineChars = "\xD\xA";
          settings.ConformanceLevel = ConformanceLevel.Fragment;
          settings.CheckCharacters = false;
          settings.Encoding = Encoding.UTF8;

          // First store the WPL into a temp file, if everything goes alright
          // then copy the temp file over the original and delete the temp.
          tempFilename = Path.GetTempPath() +
            Path.DirectorySeparatorChar +
            Path.GetRandomFileName() + ".wpl";

          writer = XmlWriter.Create(tempFilename, settings);
          writer.WriteProcessingInstruction("wpl", "version=\"1.0\"");
          writer.WriteStartElement("smil");
          if (header.Write(writer) && body.Write(writer))
          {
            writer.WriteEndElement(); // </smil>
            writer.Close();
            File.Copy(tempFilename, filename, true);
          }
        }
      }
      catch (Exception xp)
      {
        MessageBox.Show(xp.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }
      finally
      {
        if (writer != null)
        {
          writer.Close();
          try { File.Delete(tempFilename); }
          catch { }
        }
      }
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="removeType"></param>
    public void RemoveDoubles(WMPRemoveDoubleType removeType)
    {
      header.ItemCount = body.RemoveDoubles(removeType);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="removeType"></param>
    public void RemoveTIDAndCIS()
    {
      body.RemoveTID();
    }
    /// <summary>
    /// Remove entries that have a non-existing file. 
    /// </summary>
    public void RemoveNonFiles()
    {
      body.RemoveNonFiles();
    }
    /// <summary>
    /// 
    /// </summary>
    public void Shuffle()
    {
      body.Shuffle();
    }
  }
}