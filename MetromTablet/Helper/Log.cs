using System;
using System.IO;

namespace MetromTablet.Helper
{
	class Log
	{
		static Log log; 
 

        private Log() { } 
 

        public static Log GetInstance() 
        { 
            if (log == null) 
                log = new Log(); 
            return log; 
        } 
 

        internal void ProcessError(Exception exception) 
        { 
			var error = DateTime.Now.ToString("MM/dd/yyyy, HH:mm") + Environment.NewLine;
            error += exception.Message; 
 
            while (exception.InnerException != null) 
            {
                exception = exception.InnerException;
            }
			error += " : Inner Exception = " + exception.Message + Environment.NewLine;
			error += exception.Source + Environment.NewLine;
			error += exception.StackTrace + Environment.NewLine;
			error += exception.TargetSite + Environment.NewLine + Environment.NewLine; ;
			if (MetromRailPage.serialPort != null && MetromRailPage.serialPort.IsOpen)
			{
				MetromRailPage.serialPort.Close();
			}
			//save to file. 
            File.AppendAllText(@"C:\METROM\app.log", error + Environment.NewLine);
        } 
    } 
}
