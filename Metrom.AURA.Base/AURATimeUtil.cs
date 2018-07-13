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
  public static class AURATimeUtil
  {
    public const uint kMillisecondsPerDay = 24 * 60 * 60 * 1000;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="refTime"></param>
    /// <param name="utcAdjust"></param>
    /// <param name="hours"></param>
    /// <param name="minutes"></param>
    /// <param name="seconds"></param>
    /// <param name="milliseconds"></param>
    /// 
    public static void DecodeTimestamp(
      uint refTime,
      int utcAdjust,
      out int hours,
      out int minutes,
      out int seconds,
      out int milliseconds)
    {
      uint time = (refTime + (uint)((24 + utcAdjust) * 60 * 60 * 1000)) % kMillisecondsPerDay;

      milliseconds = (int)(time % 1000);
      time /= 1000;
      seconds = (int)(time % 60);
      time /= 60;
      minutes = (int)(time % 60);
      hours = (int)(time / 60);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="refTime"></param>
    /// <returns></returns>
    /// 
    public static DateTime TimestampToDateTime(DateTime utcUseThisDate, uint refTime)
    {
      int milliseconds = (int)(refTime % 1000);
      refTime /= 1000;
      int seconds = (int)(refTime % 60);
      refTime /= 60;
      int minutes = (int)(refTime % 60);
      int hours = (int)(refTime / 60);

      DateTime dt;

      try
      {
        dt = new DateTime(utcUseThisDate.Year, utcUseThisDate.Month, utcUseThisDate.Day, hours, minutes, seconds, milliseconds, DateTimeKind.Utc);
      }
      catch (ArgumentOutOfRangeException)
      {
        dt = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      }

      return dt;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="refTime"></param>
    /// <param name="utcAdjust"></param>
    /// <returns></returns>
    /// 
    public static string TimestampToString(uint refTime, int utcAdjust)
    {
      int hours, minutes, seconds, msec;

      DecodeTimestamp(refTime, utcAdjust, out hours, out minutes, out seconds, out msec);

      return string.Format("{0:d2}:{1:d2}:{2:d2}.{3:d3}", hours, minutes, seconds, msec);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="refTime"></param>
    /// <param name="utcAdjust"></param>
    /// <returns></returns>
    /// 
    public static uint LocalAdjustTimestamp(uint refTime, int utcAdjust)
    {
      return (refTime + (uint)((24 + utcAdjust) * 60 * 60 * 1000)) % kMillisecondsPerDay;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="refTime1"></param>
    /// <param name="refTime2"></param>
    /// <returns></returns>
    /// 
    public static int TimeDeltaMs(uint refTime1, uint refTime2)
    {
      // TODO: THIS IS NOT CORRECT!! But as long as the time isn't near the wrap time, it'll work...
      return (int)((Int64)refTime1 - (Int64)refTime2);
#if HOLD_THIS_ITS_NOT_QUITE_RIGHT
      if (refTime1 >= refTime2)
        return (int)(refTime1 - refTime2);
      else
        return (int)(refTime1 + kMillisecondsPerDay - refTime2);
#endif
    }
  }


}
