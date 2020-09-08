using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class UARTS
    {
        public SerialPort UartSerialPort = new SerialPort();
        public bool IsReceive;
       

        #region 打开串口
        public bool OpenPort(string com)
        {
            try
            {
                UartSerialPort.DataReceived += new SerialDataReceivedEventHandler(ModbusPort_DataReceived);
                UartSerialPort.PortName = com;
                UartSerialPort.BaudRate = 9600;
                UartSerialPort.Parity = Parity.None;
                UartSerialPort.StopBits = StopBits.One;
                UartSerialPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void ModbusPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(100);//延缓一会，用于防止硬件发送速率跟不上缓存数据导致的缓存数据杂乱
                int len = UartSerialPort.BytesToRead;
                Byte[] readBuffer = new Byte[len];
                UartSerialPort.Read(readBuffer, 0, len); //将数据读入缓存
                if(readBuffer[0] == 49 && readBuffer[1] == 50 && readBuffer[2] == 51)//如果收到了"123"字符，则认为收到应答
                    IsReceive = true;
            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
            }
        }

        #endregion

        #region 关闭串口
        public bool ClosePort()
        {
            try
            {
                UartSerialPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        #endregion

    }
}
