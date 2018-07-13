using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Metrom.Base;


namespace Metrom.AURA.Base
{


  /// <summary>
  /// 
  /// </summary>
  /// 
  public static class GlobalData
  {
    private static object lockObj_ = new object();

    private static TwoPartVersion configVersion_;

    public static TwoPartVersion ConfigVersion
    {
      get
      {
        lock (lockObj_)
          return configVersion_;
      }
      set
      {
        lock (lockObj_)
        {
          configVersion_ = (value == null) ? new TwoPartVersion() : value;
        }
      }
    }
  }


}
