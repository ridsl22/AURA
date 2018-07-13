using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Metrom.AURA.Log;


namespace Metrom.AURA.ViewLog
{


  public interface IViewLogDlgHost
  {
    void ViewLog_UIFeedback(string text);

    void ViewLog_SendGetLogStatus();
    void ViewLog_SendGetLogEntry(AURALog whichLog, uint entryIndex);
  }


}
