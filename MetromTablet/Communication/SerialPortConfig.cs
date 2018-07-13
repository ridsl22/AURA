using System;
using System.IO.Ports;


namespace MetromTablet.RS485Manager
{
    public class SerialPortConfig
    {
        #region Properties

        public int BaudRate
        { get; set; }

        public int DataBits
        { get; set; }

        public Parity Parity
        { get; set; }

        public StopBits StopBits
        { get; set; }

        public Handshake Handshake
        { get; set; }

        public int ReadTimeout
        { get; set; }

        public int WriteTimeout
        { get; set; }

        #endregion

        #region Lifetime Management

        /// <summary>
        /// Ctor.
        /// </summary>
        /// 
        public SerialPortConfig()
        {
            BaudRate = 115200;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            Handshake = Handshake.None;
            ReadTimeout = 1000;//100
            WriteTimeout = 1000;//100
        }

        /// <summary>
        /// "Copy" ctor.
        /// </summary>
        /// <param name="src"></param>
        /// 
        public SerialPortConfig(SerialPortConfig src)
        {
            BaudRate = src.BaudRate;
            DataBits = src.DataBits;
            Parity = src.Parity;
            StopBits = src.StopBits;
            Handshake = src.Handshake;
            ReadTimeout = src.ReadTimeout;
            WriteTimeout = src.WriteTimeout;
        }

        #endregion

        #region Operations

        /// <summary>
        /// Applies the configuration values to the supplied SerialPort.
        /// </summary>
        /// <param name="port"></param>
        /// 
        public void Apply(SerialPort port)
        {
            // Sanity.

            if (port == null)
                throw new ArgumentNullException("port", "Port may not be null");

            port.BaudRate = BaudRate;
            port.DataBits = DataBits;
            port.Parity = Parity;
            port.StopBits = StopBits;
            port.Handshake = Handshake;
            port.ReadTimeout = ReadTimeout;
            port.WriteTimeout = WriteTimeout;
        }

        #endregion
    }

}
