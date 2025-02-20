﻿using Arthur.Core;
using Arthur.App.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arthur.App.Comm
{
    public class SerialComm : IComm
    {
        public Result Connect(Commor commor)
        {
            commor.Connector = null;
            var serialCommor = (SerialCommor)commor.Communicator;
            var serialPort = new SerialPort(serialCommor.PortName, serialCommor.BaudRate, serialCommor.Parity, serialCommor.DataBits);
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }
            }
            catch (Exception ex)
            {
                return new Result(string.Format("连接{0}失败", commor.Communicator.Name), ex);
            }
            commor.Connector = serialPort;
            return Result.Success;
        }

        public Result EndConnect(Commor commor)
        {
            var serialPort = (SerialPort)commor.Connector;
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                serialPort.Dispose();
            }
            catch (Exception ex)
            {
                return new Result(ex.Message);
            }
            return Result.Success;
        }

        public Result Comm(Commor commor, string input, int timeout)
        {
            var serialCommor = (SerialCommor)commor.Communicator;
            var recvData = string.Empty;
            var serialPort = (SerialPort)commor.Connector;
            try
            {
                if (!input.Contains(" "))
                {
                    serialPort.Write(input);
                }
                else
                {
                    var bytes = Arthur.Core.Transfer._Convert.StrToToHexBytes(input);
                    serialPort.Write(bytes, 0, bytes.Length);
                }
                Thread.Sleep(timeout);
                Byte[] InputBuf = new Byte[128];
                serialPort.Read(InputBuf, 0, serialPort.BytesToRead);
                recvData = Encoding.ASCII.GetString(InputBuf).Trim(new char[] { '\0', '\r', '\n' });

                if (string.IsNullOrEmpty(recvData))
                {
                    return new Result("规定时间内串口未返回数据:" + serialPort.PortName);
                }
            }
            catch (Exception ex)
            {
                return new Result(ex.Message);
            }
            return Result.SuccessHasData(recvData);
        }
    }
}
