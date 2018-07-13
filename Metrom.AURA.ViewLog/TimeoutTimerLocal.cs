using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;


namespace Metrom.AURA.ViewLog
{


  internal class TimeoutTimerLocal
  {
    private Timer[] timers_ = new Timer[2];

    private int curTimerNdx_ = 0;

    private Timer curTimer_;

    private bool isRunning_ = false;

    public double Timeout
    { get; set; }

    public event ElapsedEventHandler Elapsed;

    public TimeoutTimerLocal()
    {
      timers_[0] = new Timer() { AutoReset = false };
      timers_[1] = new Timer() { AutoReset = false };

      timers_[0].Elapsed += (s, e) => { System.Diagnostics.Debug.WriteLine("##### [tmo(0) elapsed]"); System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() => { TimerElapsed(s, e); })); };
      timers_[1].Elapsed += (s, e) => { System.Diagnostics.Debug.WriteLine("##### [tmo(1) elapsed]"); System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() => { TimerElapsed(s, e); })); };

      curTimer_ = timers_[0];
    }

    public void Start()
    {
      if (isRunning_)
        Stop();

      curTimer_.AutoReset = true;  // see MSDN doc, Timer.Interval property, Note box under Remarks
      curTimer_.Interval = Timeout;
      curTimer_.AutoReset = false;

      isRunning_ = true;
      curTimer_.Start();
    }

    public void Stop()
    {
      if (isRunning_)
      {
        isRunning_ = false;
        curTimer_.Stop();

        SwitchTimers();
      }
    }

    private void SwitchTimers()
    {
      ++curTimerNdx_;
      if (curTimerNdx_ == 2)
        curTimerNdx_ = 0;

      curTimer_ = timers_[curTimerNdx_];
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
      if (sender != curTimer_)
        return;  // EARLY RETURN! (race condition)

      isRunning_ = false;

      SwitchTimers();

      if (Elapsed != null)
        Elapsed(sender, e);
    }

  }


}
