using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Metrom.AURA.Base
{


  /// <summary>
  /// 
  /// </summary>
  /// 
  public static class AURAUtil
  {
    private static string[] CompassPoints8 = new string[]
    { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="heading"></param>
    /// <returns></returns>
    /// 
    public static string HeadingToSymbol(int heading)
    {
      return CompassPoints8[((heading + 22) % 360) / 45];
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sepSource"></param>
    /// <returns></returns>
    /// 
    public static string ShortSepSource(SeparationSource sepSource)
    {
      string res = "?";

      switch (sepSource)
      {
      case SeparationSource.Unknown:
        res = "U";
        break;
      case SeparationSource.RCM:
        res = "R";
        break;
      case SeparationSource.GPS:
        res = "G";
        break;
      }

      return res;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    /// 
    public static uint CalculateFileHash32(string filename)
    {
      if (filename == null)
        throw new ArgumentNullException("filename");

      byte[] data = ASCIIEncoding.ASCII.GetBytes(filename);

      uint hash = 0;

      foreach (byte datum in data)
        hash = hash * 31 + datum;

      return hash;
    }
  }


}
