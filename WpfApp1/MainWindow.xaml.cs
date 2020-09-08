using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UARTS myUart = new UARTS();
        private int count;

        public MainWindow()
        {
            InitializeComponent();
            CheckCOM();
        }

        private void BtnClick_OpenPort(object sender, RoutedEventArgs e)
        {
            if ((string)btnOpenPort.Content == "打开串口")
            {
                btnOpenPort.Content = "关闭串口";
                myUart.OpenPort(cbPort.Text);
            }
            else
            {
                btnOpenPort.Content = "打开串口";
                myUart.ClosePort();
            }
        }


        #region 串口发送
        //str 是发送的字符，retry_num 是重发次数timeout 是超时时间
        public bool UartSend(string str,UInt32 retry_num, UInt32 timeout)
        {
            try
            {
                myUart.IsReceive = false;//发送数据前重置接收标志
                for (int i = 0; i < retry_num; i++)
                {
                    myUart.UartSerialPort.Write(str);//串口发送数据
                    while (myUart.IsReceive == false)
                    {
                        Thread.Sleep(10);
                        count++;
                        if (count >= timeout / 10)
                            break;
                    }
                    if (count < timeout / 10) //如果在规定的时间内收到了应答,则直接返回，如果没有应答则继续重发
                        return true;

                    WriteLog(Brushes.Red, $"发送次数 + {i} \r\n");

                    count = 0;
                }

                return false;//如果发送次数超过3次，依然没有收到应答，则认为发送指令失败
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        #endregion

        private void BtnClickSendCmd(object sender, RoutedEventArgs e)
        {
            bool res = false;
            var myTask = Task.Run(() =>
            {
                res = UartSend("456\r\n", 3,1000);
                if (res == false)
                    WriteLog(Brushes.Red, $"发送失败！\r\n");
                else
                    WriteLog(Brushes.Black, $"发送成功。\r\n");
            });

        }


        #region 热插拔中断
        public const int WM_DEVICECHANGE = 0x219;          //Windows消息编号
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public IntPtr DeveiceChanged(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DEVICECHANGE)
            {
                switch (wParam.ToInt32())
                {
                    case DBT_DEVICEARRIVAL://设备插入  
                        CheckCOM();
                        break;
                    case DBT_DEVICEREMOVECOMPLETE: //设备卸载
                        CheckCOM();
                        break;

                    default:
                        break;
                }
            }
            return IntPtr.Zero;
        }
        #endregion

        #region 检测端口
        public void CheckCOM()
        {
            try
            {
                string[] portsName = SerialPort.GetPortNames();
                Array.Sort(portsName);
                cbPort.ItemsSource = portsName;

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        #endregion

        #region 打印日志方法
        private void WriteLog(SolidColorBrush color, string v)
        {
            Action action1 = () =>
            {
                Run run = new Run() { Text = v, Foreground = color };
                myParagraph.Inlines.Add(run);
            };
            tbxLog.Dispatcher.Invoke(action1);
        }
        #endregion
        private void LogTextChanged(object sender, TextChangedEventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            richTextBox.ScrollToEnd();
        }
    }
}
