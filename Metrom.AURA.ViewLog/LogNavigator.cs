using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Metrom.AURA.ViewLog
{


  /// <summary>
  /// 
  /// </summary>
  /// 
  public class LogNavigator
  {
    #region Constants

    private uint kInvalidIndex = 0xffffffff;

    #endregion

    #region Instance Fields

    private uint capacity_;

    private uint headNdx_;

    private uint tailNdx_;

    private uint totalEntries_;

    #endregion

    #region Properties

    /// <summary>
    /// 
    /// </summary>
    /// 
    public uint Capacity
    { get { return capacity_; } }

    /// <summary>
    /// 
    /// </summary>
    /// 
    public uint TotalEntries
    { get { return totalEntries_; } }

    #endregion

    #region Lifetime Management

    /// <summary>
    /// 
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="head"></param>
    /// <param name="tail"></param>
    /// 
    public LogNavigator(uint capacity, uint head, uint tail)
    {
      capacity_ = capacity;
      headNdx_ = head;
      tailNdx_ = tail;

      if (head != kInvalidIndex)
        totalEntries_ = (head >= tail) ? head - tail + 1 : (capacity_ - tail) + head + 1;
    }

    #endregion

    #region Operations

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logicalNdx"></param>
    /// <returns></returns>
    /// 
    public uint GetPhysicalIndex(uint logicalNdx)
    {
      if (logicalNdx >= totalEntries_)
        throw new InvalidOperationException(string.Format("Supplied logical index ({0}) is outside log extent ({1} entries).", logicalNdx, totalEntries_));

      uint physicalNdx = tailNdx_ + logicalNdx;
      if (physicalNdx >= capacity_)
        physicalNdx -= capacity_;

      return physicalNdx;
    }

    #endregion
  }


}
