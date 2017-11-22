using AreaParty.fileService;
using AreaPartyTests.fileService;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AreaParty.fileServece
{
    public class SendLittle
    {
        // 数据包类
        UdpClient udpSendClient;
        // 数据包基本信息
        String yourAddress = "192.168.1.4";
        int yourPort = 10000;
        int myPort = 12000;
        /* 用于分组的数据 */
        int[] StartTip = { 12, 0, 4, 8 }; // 开始传输包(12) 标识(4)|次随机数(4)|报尾(4)
        int[] StartAckTip = { 12, 0, 4, 8 }; // 开始确认包(12) 标识(4)|次随机数(4)|报尾(4)
        int[] StopTip = { 12, 0, 4, 8 }; // 结束传输包(12) 标识(4)|次随机数(4)|报尾(4)
        int[] StopAckTip = { 12, 0, 4, 8 }; // 结束确认包(12) 标识(4)|次随机数(4)|报尾(4)
        int[] HeadTip = { 18, 0, 4, 8, 12, 14 }; // 分组头包(18)
                                                 // 标识(4)|次随机数(4)|块随机数(4)|分组数量(2)|报尾(4)
        int[] HeadAckTip = { 16, 0, 4, 8, 12 }; // 分组头确认包(16)
                                                // 标识(4)|次随机数(4)|块随机数(4)|报尾(4)
        int[] DataTip = { 1020, 0, 4, 8, 12, 14, 16, 1016 }; // 分组数据包(520)
                                                             // 标识(4)|次随机数(4)|块随机数(4)|序号(2)|长度(2)|数据(500)|报尾(4)
        int[] FinishTip = { 20, 0, 4, 8, 12, 16 }; // 发送完毕包(20)
                                                   // 标识(40|次随机数(4)|块随机数(4)|小随机数(4)|报尾(4)
        int[] ReqAgainTip = { 522, 0, 4, 8, 12, 16, 18, 518 }; // 请求重发包(522)
                                                               // 标识(4)|次随机数(4)|块随机数(4)|小随机数(4)|长度(2)|数据(500)|报尾(4)
        int[] FilePropertyTip = { 407, 0, 4, 8, 16, 20, 275, 307 }; // 文件属性包(12) 标识(4)|次随机数(4)|文件长度(8)|文件名长度(4)|文件名(255)|MD5(128)|报尾(4)
        int[] FilePropertyAckTip = { 12, 0, 4, 8 }; // 文件属性响应包(12) 标识(4)|次随机数(4)|报尾(4)
        int[] FileBreakPointTip = { 28, 0, 4, 8, 16, 24 };// 文件断点续传包(28) 标识(4)|次随机数(4)|文件长度(8)|已传文件长度(8)|报尾(4)
        int[] FileBreakPointAckTip = { };// 文件属性响应包(12) 标识(4)|次随机数(4)|报尾(4)
                                         /* 标识 */
        String datagramHead = "[]01";
        String datagramData = "[]02";
        String datagramHeadAck = "[]03";
        String datagramReqAgain = "[]04";
        String datagramFinish = "[]05";
        String datagramStop = "[]06";
        String datagramStopAck = "[]07";
        String datagramStart = "[]08";
        String datagramFileProperty = "[]09";
        String datagramStartAck = "[]0A";
        String datagramFilePropertyAck = "[]0B";
        String datagramFileBreakPoint = "[]0C";
        String datagramFileBreakPointAck = "[]0D";
        String datagramTail = "end|";
        /* 数据长度的规定 */
        int dataLength = 1000;// 发送数据长度
        int groupNum = 250;// 分组数量
        int littleLength = 1024 * 1024;
        SendState sendState = null;
        //	long haveSendBytes = 0;
        IPEndPoint host; //服务器端套接字，客户端发送数据到此套接字
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SendLittle(String receiverIp, int senderPort, int receiverPort, SendState ss)
        {
            InitLog4Net();
            udpSendClient = new UdpClient(senderPort);
            IPAddress HostIP = IPAddress.Parse(receiverIp);
            host = new IPEndPoint(HostIP, receiverPort);
            sendState = ss;
            log.Info("建立发送程序");
        }

        public SendLittle(UdpClient udpClient, String receiverIp, int receiverPort, SendState ss)
        {
            InitLog4Net();
            udpSendClient = udpClient;
            IPAddress HostIP = IPAddress.Parse(receiverIp);
            host = new IPEndPoint(HostIP, receiverPort);
            sendState = ss;
            log.Info("建立发送程序");
        }
        public SendLittle(UdpClient udpClient, IPEndPoint receiverEP, SendState ss)
        {
            InitLog4Net();
            udpSendClient = udpClient;
            host = receiverEP;
            sendState = ss;
            log.Info("建立发送程序");
        }
        private static void InitLog4Net()
        {
            var logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/fileService/log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logCfg);
        }
        //public long sendFileProperty(File file)
        //{
        //    if (file.exists() && file.isFile())
        //    {
        //        System.out.println("文件长度为" + file.length());
        //        System.out.println("文件名为" + file.getName());

        //        DatagramPacket sendPacket = null, receivePacket = null;
        //        byte sendBuffer[] = new byte[FilePropertyTip[0]];
        //        byte receiveBuffer[] = new byte[FileBreakPointTip[0]];
        //        int numberRandom = (new Random()).nextInt();
        //        // 填充开始传输包
        //        ByteChange.StringToByte(datagramFileProperty, sendBuffer, FilePropertyTip[1], 4);
        //        ByteChange.intToByte(sendBuffer, FilePropertyTip[2], numberRandom);
        //        ByteChange.longToByte(sendBuffer, FilePropertyTip[3], file.length());
        //        ByteChange.intToByte(sendBuffer, FilePropertyTip[4], file.getName().length());
        //        ByteChange.StringToByte(file.getName(), sendBuffer, FilePropertyTip[5], file.getName().length());
        //        try
        //        {
        //            String MD5Str = MD5.getFileMD5String(file);
        //            ByteChange.StringToByte(MD5Str, sendBuffer, FilePropertyTip[6], 32);
        //        }
        //        catch (Exception e1)
        //        {
        //            // TODO Auto-generated catch block
        //            logger.error("{}", e1);
        //        }
        //        ByteChange.StringToByte(datagramTail, sendBuffer, FilePropertyTip[7], 4);

        //        sendState.setFileLength(file.length());
        //        // 设置超时1ms及初始化数据包
        //        try
        //        {
        //            sendPacket = new DatagramPacket(sendBuffer, sendBuffer.length, InetAddress.getByName(yourAddress),
        //                    yourPort);
        //            receivePacket = new DatagramPacket(receiveBuffer, receiveBuffer.length);
        //        }
        //        catch (Exception e)
        //        {
        //            e.printStackTrace();
        //        }
        //        boolean canSend = true;
        //        while (true)
        //        {
        //            try
        //            {
        //                if (canSend)
        //                {
        //                    socket.setSoTimeout(sendState.getWaitTime());
        //                    socket.send(sendPacket);
        //                    sendState.addHaveSendBytes(sendPacket.getLength());
        //                    System.out.println("发送文件属性包");
        //                }
        //                socket.receive(receivePacket);
        //                // 如果接收到数据包,无论如何,下一次都不能发送
        //                canSend = false;
        //                // 判断标识
        //                String str = new String(receiveBuffer, FilePropertyAckTip[1], 4);
        //                System.out.println("str=" + str + "datagramFilePropertyAck=" + datagramFilePropertyAck);
        //                if (!str.equals(datagramFilePropertyAck))
        //                {
        //                    //接收到断点信息
        //                    if (str.equals(datagramFileBreakPoint))
        //                    {
        //                        // 判断次随机数
        //                        int num = ByteChange.byteToInt(receiveBuffer, FileBreakPointTip[2]);
        //                        System.out.println("num=" + num + "numberRandom=" + numberRandom);
        //                        if (num != numberRandom)
        //                        {
        //                            continue;
        //                        }
        //                        long fileLength = ByteChange.byteToLong(receiveBuffer, FileBreakPointTip[3]);
        //                        if (fileLength != file.length())
        //                        {
        //                            continue;
        //                        }
        //                        long receivedFileLength = ByteChange.byteToLong(receiveBuffer, FileBreakPointTip[4]);
        //                        sendState.setExistFileLength(receivedFileLength);
        //                        if (receivedFileLength == fileLength)
        //                        {
        //                            logger.info("文件已存在，并传输完成");
        //                            return -1;
        //                        }
        //                        int time = sendState.getWaitTime();
        //                        if (time <= 2000 && time >= 4)
        //                        {
        //                            sendState.setWaitTime(time / 2);//延迟时间减半
        //                        }
        //                        //							logger.info("WaitTime={}",sendState.getWaitTime());
        //                        // 否则接收成功,退出循环
        //                        return receivedFileLength;
        //                    }
        //                    continue;
        //                }
        //                // 判断次随机数
        //                int num = ByteChange.byteToInt(receiveBuffer, FilePropertyAckTip[2]);
        //                System.out.println("num=" + num + "numberRandom=" + numberRandom);
        //                if (num != numberRandom)
        //                {
        //                    continue;
        //                }
        //                int time = sendState.getWaitTime();
        //                if (time <= 2000 && time >= 4)
        //                {
        //                    sendState.setWaitTime(time / 2);//延迟时间减半
        //                }
        //                //					logger.info("WaitTime={}",sendState.getWaitTime());
        //                // 否则接收成功,退出循环
        //                return 0;
        //            }
        //            catch (Exception e)
        //            {
        //                if (e instanceof SocketTimeoutException) {
        //                    canSend = true;
        //                    int time = sendState.getWaitTime();
        //                    if (time <= 1800 && time > 0)
        //                    {
        //                        sendState.setWaitTime(time + 10);//延迟时间+10
        //                    }
        //                    //						logger.info("WaitTime={}",sendState.getWaitTime());
        //                }else{
        //                    e.printStackTrace();
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //文件不存在
        //        return -2;
        //    }
        //}

        /* 拆分分组,并发送 */
        public void SendAll(byte[] littleBuffer, int littleLength)
        {
            byte[] buffer = new byte[dataLength * groupNum];// 建立缓冲区
            int numberRandom = (new Random()).Next();
            SendStart(numberRandom);// 发送传输开始数据包
            // 计算块个数
            int bigNumber = littleLength / (dataLength * groupNum) + (littleLength % (dataLength * groupNum) == 0 ? 0 : 1);
            for (int i = 0; i < bigNumber; i++)
            {
                // 填充buffer
                int length;
                if (i == bigNumber - 1)
                {
                    length = littleLength - i * (dataLength * groupNum);
                }
                else
                {
                    length = dataLength * groupNum;
                }
                for (int j = 0; j < length; j++)
                {
                    buffer[j] = littleBuffer[j + i * dataLength * groupNum];
                }
                SendGroup(numberRandom, buffer, length);
                //sendState.addHavaSendfileLength(length);
            }
            SendOver(numberRandom);
        }

        /* 发送开始传输包 */
        public void SendStart(int numberRandom)
        {
            //log.Info("SendStart进入");
            byte[] sendBuffer = new byte[StartTip[0]];
            byte[] receiveBuffer = new byte[StartAckTip[0]];
            // 填充开始传输包
            ByteChange.StringToByte(datagramStart, sendBuffer, StartTip[1], 4);
            ByteChange.IntToByte(sendBuffer, StartTip[2], numberRandom);
            ByteChange.StringToByte(datagramTail, sendBuffer, StartTip[3], 4);
            // 设置超时1ms及初始化数据包
            bool canSend = true;
            while (true)
            {
                try
                {
                    if (canSend)
                    {
                        udpSendClient.Client.ReceiveTimeout = sendState.WaitTime;
                        udpSendClient.Send(sendBuffer, sendBuffer.Length, host);
                        sendState.AddNumHaveSendBytes(sendBuffer.Length);
                    }
                    receiveBuffer = udpSendClient.Receive(ref host);
                    // 如果接收到数据包,无论如何,下一次都不能发送
                    canSend = false;
                    // 判断标识
                    String str = Encoding.Default.GetString(receiveBuffer, StartAckTip[1], 4);
                    if (!datagramStartAck.Equals(str))
                    {
                        continue;
                    }
                    // 判断次随机数
                    int num = ByteChange.ByteToInt(receiveBuffer, StartAckTip[2]);
                    if (num != numberRandom)
                    {
                        continue;
                    }
                    // 否则接收成功,退出循环
                    int time = sendState.WaitTime;
                    if (time <= 2000 && time >= 4)
                    {
                        sendState.WaitTime = time / 2;//延迟时间减半
                    }
                    //				logger.info("WaitTime={}",sendState.getWaitTime());
                    //log.Info("SendStart退出");
                    break;
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10060)
                    {
                        canSend = true;
                        int time = sendState.WaitTime;
                        if (time <= 1800 && time > 0)
                        {
                            sendState.WaitTime = (time + 10);//延迟时间+10
                        }
                    }
                    //MessageBox.Show(e.ToString());
                    //if (e is TimeoutException)
                    //{
                    //    canSend = true;
                    //    //int time = sendState.getWaitTime();
                    //    //if (time <= 1800 && time > 0)
                    //    //{
                    //    //    sendState.setWaitTime(time + 10);//延迟时间+10
                    //    //}
                    //    //					logger.info("WaitTime={}",sendState.getWaitTime());
                    //}
                    //if(e is SocketException)
                    //{
                    //}
                }
            }
            //		System.out.println("开始发送包发送完毕退出");
        }

        /* 发送一组数据包 */
        public void SendGroup(int numberRandom, byte[] buffer, int length)
        {
            //log.Info("SendGroup进入");
            //assert length <= dataLength * groupNum : "Error:数据量大于最大分组容量!";
            if (length > dataLength * groupNum)
            {
                throw new Exception("Error:数据量大于最大分组容量!");
            }
            // 生成块随机数
            int blockRandom = (new Random()).Next();
            // 计算分组数量
            int number = length / dataLength + (length % dataLength == 0 ? 0 : 1);
            // 发送分组头
            SendDatagramHead(numberRandom, blockRandom, number);
            //		System.out.println("发送分组:" + number);
            // 发送分组体
            /*
		     * 发送方式是 1:全部连续发送 2.发送发送完毕包 3:接收请求重发包.如果接收超时,执行2
		     * 4:如果请求重发包中有数据,则重发相应的包,然后执行2.否则执行5 5:发送成功,返回
		     */
            // 先将buffer拆分成number个分组,记录每个分组中buffer中的起始下标和数据长度
            int[] startTip = new int[number];
            int[] lengthTip = new int[number];
            bool[] sendTip = new bool[number];//记录是否发送成功
                                              // 一开始,每组数据都未传输成功
            for (int i = 0; i < number; i++)
            {
                sendTip[i] = false;
            }
            if (number == 1)
            {
                // 如果只有一组的情况,比较特殊.为提高效率,单独考虑
                startTip[0] = 0;
                lengthTip[0] = length;
            }
            else
            {
                // 分组超过一组
                for (int i = 0; i < number; i++)
                {
                    if (i == 0)
                    {
                        startTip[i] = 0;
                    }
                    else
                    {
                        // 很明显,只有最后一组数据长度才可能不为dataLength
                        // 所以,每一组前面的分组长度肯定都是dataLength
                        startTip[i] = dataLength * i;
                    }
                    if (i != number - 1)
                    {
                        lengthTip[i] = dataLength;
                    }
                    else
                    {
                        lengthTip[i] = length - dataLength * i;
                    }
                }
            }
            // 下面开始传送
            // 设定产生小随机数的机器
            // 用于生成小随机数
            MyRandom myRandom = new MyRandom(1);
            // 设置random1,用于防止接收到同一个请求从发包而重复发送
            // 发送端保证,绝不会发值为0的小随机数
            while (true)
            {
                // 按顺序发送尚未成功发送的数据分组
                int count = 0;
                for (int i = 0; i < number; i++)
                {
                    if (sendTip[i] == false)
                    {
                        count++;
                        SendDatagramData(numberRandom, blockRandom, i, buffer, startTip[i], lengthTip[i]);
                        if (count >= sendState.NumSendWindow)
                        {
                            Thread.Sleep(sendState.WaitTime);
                            count = 0;
                        }
                    }
                }
                sendState.NumSendPackets = number;
                // 接收请求重发包(超时5ms),内含发送"发送完毕包"
                // 在getDatagramReq中,已经修改了sendTip的值
                bool flag = GetDatagramReq(numberRandom, blockRandom, myRandom.GetNextRandom(), sendTip);
                float resendRate = (float)sendState.NumResendPackets / sendState.NumSendPackets;
                //logger.warn("number={}, haveSendNumber={}, resendRate={}", number, sendState.getNumResendPackets(), resendRate);
                if (resendRate > 0)
                {
                    int time = sendState.WaitTime;
                    if (time <= 1800 && time > 0)
                    {
                        sendState.WaitTime = (time + 10);//延迟时间翻倍
                    }
                    int sendWindow = sendState.NumSendWindow;
                    if (sendWindow >= 40 && sendWindow <= 250)
                    {
                        sendState.NumSendWindow = (sendWindow - 10);
                    }
                    //logger.info("WaitTime={},windows={}", sendState.getWaitTime(), sendState.getNumSendWindow());
                }
                else
                {
                    int time = sendState.WaitTime;
                    if (time <= 2000 && time >= 11)
                    {
                        sendState.WaitTime = (time - 10);//延迟时间*2
                    }
                    int sendWindow = sendState.NumSendWindow;
                    if (sendWindow >= 30 && sendWindow <= 240)
                    {
                        sendState.NumSendWindow = (sendWindow + 10);
                    }
                    //logger.info("WaitTime={},windows={}", sendState.getWaitTime(), sendState.getNumSendWindow());
                }
                if (flag)
                {
                    break;
                    //log.Info("SendGroup退出");
                }
            }
        }

        /* 发送分组头 */
        public void SendDatagramHead(int numberRandom, int blockRandom, int number)
        {
            //log.Info("SendDatagramHead进入");
            byte[] buffer = new byte[HeadTip[0]];
            // 填充分组头标识
            ByteChange.StringToByte(datagramHead, buffer, HeadTip[1], 4);
            // 填充次随机数
            ByteChange.IntToByte(buffer, HeadTip[2], numberRandom);
            // 填充块随机数
            ByteChange.IntToByte(buffer, HeadTip[3], blockRandom);
            // 写入数组数量
            //assert number <= groupNum : "分组数量超过500";
            if (number > groupNum)
            {
                throw new Exception("分组数量超过500");
            }
            ByteChange.ShortToByte(buffer, HeadTip[4], (short)number);
            // 填充报尾标识
            ByteChange.StringToByte(datagramTail, buffer, HeadTip[5], 4);
            // 发送分组头

            // 发送后接收报头确认报,接收超时或者接收的不匹配则重发分组头
            bool canSendHead = true;
            while (true)
            {
                try
                {
                    if (canSendHead)
                    {
                        udpSendClient.Client.ReceiveTimeout = sendState.WaitTime;
                        udpSendClient.Send(buffer, buffer.Length, host);
                        sendState.AddNumHaveSendBytes(buffer.Length);
                    }
                    byte[] c = new byte[HeadAckTip[0]];
                    ByteChange.CleanByte(c);
                    c = udpSendClient.Receive(ref host);
                    // 只要接收到数据包,不论是不正确,下次循环就不能发送头
                    canSendHead = false;
                    // 进行检查
                    // 1.检查其是否是分组头确认包
                    //String s = new String(c, HeadAckTip[1], 4);
                    String s = Encoding.Default.GetString(c, HeadAckTip[1], 4);
                    //if (!s.equals(datagramHeadAck))
                    if (!datagramHeadAck.Equals(s))
                    {
                        continue;
                    }
                    // 2.检查其次随机数,是否与自己的匹配
                    int r = ByteChange.ByteToInt(c, HeadAckTip[2]);
                    if (r != numberRandom)
                    {
                        continue;
                    }
                    // 3.检查其块随机数
                    r = ByteChange.ByteToInt(c, HeadAckTip[3]);
                    if (r != blockRandom)
                    {
                        continue;
                    }
                    // 所有检查均正确,发送成功,返回
                    int time = sendState.WaitTime;
                    if (time <= 2000 && time >= 4)
                    {
                        sendState.WaitTime = (time / 2);//延迟时间翻倍
                    }
                    //logger.info("WaitTime={}",sendState.getWaitTime());
                    //log.Info("SendDatagramHead退出");
                    break;
                }
                catch (SocketException ex)
                {
                    //MessageBox.Show(ex.ToString());
                    // Logger.getLogger(Send.class.getName()).log(Level.SEVERE,
                    // null, ex);
                    // 如果是接收超时,则一下次循环的时候,要先发送头
                    if (ex.ErrorCode == 10060) {
                        canSendHead = true;
                        int time = sendState.WaitTime;
                        if (time <= 1800 && time > 0)
                        {
                            sendState.WaitTime = (time + 10);//延迟时间+10
                        }
                        //					logger.info("WaitTime={}",sendState.getWaitTime());
                    }
                    //if (ex is TimeoutException)
                    //{
                    //    canSendHead = true;
                    //}
                    //if(ex is SocketException)
                    //{
                    //    canSendHead = true;
                    //}
                    //canSendHead = true;
                }
            }
        }

        /* 发送分组包,无回送 */
        public void SendDatagramData(int numberRandom, int blockRandom, int serial, byte[] buffer, int start, int length)
        {
            //DatagramPacket packet = null;
            byte[] b = new byte[DataTip[0]];
            //		System.out.println("发送分组,序列号:" + serial);
            //assert length <= dataLength : "Error[发送分组包]:数据长度大于dataLength!";
            if (length > dataLength)
            {
                throw new Exception("Error[发送分组包]:数据长度大于dataLength!");
            }
            // 填充分组包标识
            ByteChange.StringToByte(datagramData, b, DataTip[1], 4);
            // 填充次随机数
            ByteChange.IntToByte(b, DataTip[2], numberRandom);
            // 填充块随机数
            ByteChange.IntToByte(b, DataTip[3], blockRandom);
            // 填充分组序号
            ByteChange.ShortToByte(b, DataTip[4], (short)serial);
            // 填充数据长度
            ByteChange.ShortToByte(b, DataTip[5], (short)length);
            // 填充等传输的数据
            for (int i = 0; i < length; i++)
            {
                b[i + DataTip[6]] = buffer[i + start];
            }
            // 填充报尾
            ByteChange.StringToByte(datagramTail, b, DataTip[7], 4);
            // 填充完毕,发送
            try
            {
                //packet = new DatagramPacket(b, b.length, InetAddress.getByName(yourAddress), yourPort);
                //socket.send(packet);
                sendState.AddNumHaveSendBytes(b.Length);
                udpSendClient.Send(b, b.Length, host);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                //logger.error("{}", ex);
            }
        }

        /* 接收请求重发包 */
        /* 返回接收方是否接收完毕 */
        public bool GetDatagramReq(int numberRandom, int blockRandom, int littleRandom, bool[] sendTip)
        {
            //log.Info("GetDatagramReq进入");
            // 首先,设置接收超时5ms
            //DatagramPacket packetReq = null;
            // 由于请求重发包发送的是需要重发的分组
            // 所以,先将成功发送标记全部置true,再根据
            for (int i = 0; i < sendTip.Length; i++)
            {
                sendTip[i] = true;
            }
            // 请求重发包将发送失败的置false
            byte[] buffer = new byte[ReqAgainTip[0]];
            bool canSendFinish = true;
            while (true)
            {
                try
                {
                    if (canSendFinish)
                    {
                        udpSendClient.Client.ReceiveTimeout = sendState.WaitTime;
                        SendFinish(numberRandom, blockRandom, littleRandom);
                    }
                    ByteChange.CleanByte(buffer);
                    buffer = udpSendClient.Receive(ref host);
                    // 如果接收到数据包,不管是否正确,下一轮都不能发未接收请求重发包
                    canSendFinish = false;
                    // 对接收到的请法求重发包进行解析
                    // 检证是不是请求重发包
                    String s = Encoding.Default.GetString(buffer, ReqAgainTip[1], 4);
                    if (!datagramReqAgain.Equals(s))
                    {
                        continue;
                    }
                    // 匹配次随机数
                    int r = ByteChange.ByteToInt(buffer, ReqAgainTip[2]);
                    if (r != numberRandom)
                    {
                        continue;
                    }
                    // 匹配块随机数
                    r = ByteChange.ByteToInt(buffer, ReqAgainTip[3]);
                    if (r != blockRandom)
                    {
                        continue;
                    }
                    // //验证该包是否完整
                    // s=new String(buffer,1014,4);
                    // if(!s.equals(datagramTail)){
                    // continue;
                    // }
                    // 验证此请求重发包的小随机数是否与本次发送的"发送完毕包"相同
                    r = ByteChange.ByteToInt(buffer, ReqAgainTip[4]);
                    if (r != littleRandom)
                    {
                        continue;
                    }
                    // 验证完毕,提取请求重发的分组
                    // 提取数据长度
                    int length = ByteChange.ByteToShort(buffer, ReqAgainTip[5]);
                    // 如果数据长度是0,验证一下数据完整性,如果完整,则返回数据接收完毕
                    if (length == 0)
                    {
                        //s = new String(buffer, ReqAgainTip[5] + 2, 4);
                        s = Encoding.Default.GetString(buffer, ReqAgainTip[5] + 2, 4);
                        //if (s.equals(datagramTail))
                        if (datagramTail.Equals(s))
                        {
                            // 数据完整,返回
                            //log.Info("GetDatagramReq退出true");
                            return true;
                        }
                        else
                        {
                            // 数据不完整,此包无效,继续循环
                            continue;
                        }
                    }
                    else
                    {
                        sendState.NumResendPackets = 0;
                        for (int i = 0; i < length; i++)
                        {
                            int tip = ByteChange.ByteToShort(buffer, i * 2 + ReqAgainTip[6]);
                            sendTip[tip] = false;
                            sendState.AddNumResendPackets();
                        }
                        //log.Info("GetDatagramReq退出");
                        break;
                    }
                }
                catch (SocketException ex)
                {
                    // Logger.getLogger(Send.class.getName()).log(Level.SEVERE,null, ex);
                    // 如果是接收超时,那么下一次循环就应该发送"发送完毕包"
                    if (ex.ErrorCode == 10060) {
                        canSendFinish = true;
                        int time = sendState.WaitTime;
                        if (time <= 1800 && time > 0)
                        {
                            sendState.WaitTime = (time + 10);//延迟时间+10
                        }
                    }
                }
            }
            return false;
        }

        /* 发送发送完毕包 */
        public void SendFinish(int numberRandom, int blockRandom, int littleRandom)
        {
            byte[] buffer = new byte[FinishTip[0]];
            // 填充发送完毕包标识
            ByteChange.StringToByte(datagramFinish, buffer, FinishTip[1], 4);
            // 填充次随机数
            ByteChange.IntToByte(buffer, FinishTip[2], numberRandom);
            // 填充块随机数
            ByteChange.IntToByte(buffer, FinishTip[3], blockRandom);
            // 填充小随机数
            ByteChange.IntToByte(buffer, FinishTip[4], littleRandom);
            // 填充报尾
            ByteChange.StringToByte(datagramTail, buffer, FinishTip[5], 4);
            try
            {
                //DatagramPacket packet = new DatagramPacket(buffer, buffer.length, InetAddress.getByName(yourAddress),
                //        yourPort);
                //socket.send(packet);
                sendState.AddNumHaveSendBytes(buffer.Length);
                udpSendClient.Send(buffer, buffer.Length, host);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                //logger.error("{}", ex);
            }
        }

        /* 发送结束传输包 */
        public void SendOver(int numberRandom)
        {
            // 设定接收超时1ms
            udpSendClient.Client.ReceiveTimeout = sendState.WaitTime;
            // 生成传输结束包
            //DatagramPacket packetOver, packetOverAck;
            byte[] overBuffer = new byte[StopTip[0]];
            byte[] overAckBuffer = new byte[StopAckTip[0]];
            // 填充标识
            ByteChange.StringToByte(datagramStop, overBuffer, StopTip[1], datagramStop.Length);
            // 填充次随机数
            ByteChange.IntToByte(overBuffer, StopTip[2], numberRandom);
            // 填充报尾
            ByteChange.StringToByte(datagramTail, overBuffer, StopTip[3], datagramTail.Length);
            try
            {
                while (true)
                {
                    try
                    {
                        udpSendClient.Send(overBuffer, overBuffer.Length, host);
                        // sendState.addHaveSendBytes(packetOver.getLength());
                        sendState.AddNumHaveSendBytes(overAckBuffer.Length);
                        ByteChange.CleanByte(overAckBuffer);
                        overAckBuffer = udpSendClient.Receive(ref host);
                        // 检查ACK标识
                        String str = Encoding.Default.GetString(overAckBuffer, StopAckTip[1], 4);
                        if (!datagramStopAck.Equals(str))
                        {
                            continue;
                        }
                        // 检查次随机数
                        int num = ByteChange.ByteToInt(overAckBuffer, StopAckTip[2]);
                        if (num != numberRandom)
                        {
                            continue;
                        }
                        break;
                    }
                    catch (SocketException ex)
                    {
                        if(ex.ErrorCode == 10060)
                        {

                            continue;
                        }
                        //MessageBox.Show(ex.ToString());
                        //if (ex is TimeoutException)
                        //{
                        //    continue;
                        //    // 接收超时,则继续循环
                        //}
                        //if(ex is SocketException)
                        //{
                        //    continue;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.error("{}", ex);
                log.Error(ex.ToString());
            }
        }
    }
}