using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AreaParty.fileServece
{
    public class ReceiveLittle
    {
        //数据包类
        //DatagramSocket socket;
        UdpClient udpReceiveClient;
        //DatagramPacket packet;
        //数据包基本信息
        String yourAddress = "192.168.1.100";  //119.112.6.203 //180.109.158.89
        int yourPort = 12000;
        int myPort = 10000;
        /*用于分组的数据*/
        int[] StartTip = { 12, 0, 4, 8 };                   //开始传输包(12)    标识(4)|次随机数(4)|报尾(4)
        int[] StartAckTip = { 12, 0, 4, 8 };                //开始确认包(12)    标识(4)|次随机数(4)|报尾(4)
        int[] StopTip = { 12, 0, 4, 8 };                    //结束传输包(12)    标识(4)|次随机数(4)|报尾(4)
        int[] StopAckTip = { 12, 0, 4, 8 };                 //结束确认包(12)    标识(4)|次随机数(4)|报尾(4)
        int[] HeadTip = { 18, 0, 4, 8, 12, 14 };              //分组头包(18)      标识(4)|次随机数(4)|块随机数(4)|分组数量(2)|报尾(4)
        int[] HeadAckTip = { 16, 0, 4, 8, 12 };              //分组头确认包(16)  标识(4)|次随机数(4)|块随机数(4)|报尾(4)
        int[] DataTip = { 1020, 0, 4, 8, 12, 14, 16, 1016 };      //分组数据包(520)   标识(4)|次随机数(4)|块随机数(4)|序号(2)|长度(2)|数据(500)|报尾(4)
        int[] FinishTip = { 20, 0, 4, 8, 12, 16 };            //发送完毕包(20)    标识(40|次随机数(4)|块随机数(4)|小随机数(4)|报尾(4)
        int[] ReqAgainTip = { 522, 0, 4, 8, 12, 16, 18, 518 };   //请求重发包(522)   标识(4)|次随机数(4)|块随机数(4)|小随机数(4)|长度(2)|数据(500)|报尾(4)
        int[] FilePropertyTip = { 407, 0, 4, 8, 16, 20, 275, 307 }; // 文件属性包(12) 标识(4)|次随机数(4)|文件长度(8)|文件名长度(4)|文件名(255)|MD5(128)|报尾(4)
        int[] FilePropertyAckTip = { 12, 0, 4, 8 }; // 文件属性响应包(12) 标识(4)|次随机数(4)|报尾(4)	

        int[] CTCPunchDirectTip = { 28, 0, 4, 8, 16, 24 }; // 向客户端直接打洞请求包(28) 标识(4)|次随机数(4)|发起方用户ID(8)|接收方用户ID(8)|报尾(4)
        int[] CTCPunchDirectAckTip = { 28, 0, 4, 8, 16, 24 }; // 向客户端直接打洞确认包(28) 标识(4)|次随机数(4)|发起方用户ID(8)|接受方用户ID(8)|报尾(4)
        int[] FileBreakPointTip = { 28, 0, 4, 8, 16, 24 };// 文件断点续传包(28) 标识(4)|次随机数(4)|文件长度(8)|已传文件长度(8)|报尾(4)
        int[] FileBreakPointAckTip = { };// 文件属性响应包(12) 标识(4)|次随机数(4)|报尾(4)
        /*标识*/
        String datagramHead = "[]01";
        String datagramData = "[]02";
        String datagramHeadAck = "[]03";
        String datagramReqAgain = "[]04";
        String datagramFinish = "[]05";
        String datagramStart = "[]08";
        String datagramStartAck = "[]0A";
        String datagramStop = "[]06";
        String datagramStopAck = "[]07";
        String datagramFileProperty = "[]09";
        String datagramFilePropertyAck = "[]0B";
        String datagramFileBreakPoint = "[]0C";
        String datagramFileBreakPointAck = "[]0D";
        String datagramTail = "end|";

        String datagramCTCPunchDirect = "[]14";
        String datagramCTCPunchDirectAck = "[]15";

        /*数据长度的规定*/
        int dataLength = 1000;
        int groupNum = 250;
        int littleLength = 1024 * 1024;
        //用于连续存储收到的大,小随机数
        CircleStack randomStack = null, littleRandomStack = null;
        IPEndPoint host; //服务器端套接字，客户端发送数据到此套接字
        //用于存放上一次发送的请求重发包
        //DatagramPacket lastReqAgain = null;
        List<byte> lastReqAgainBuffer = new List<byte>();
        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        ReceiveState receiveState = null;
        //    long haveRecvBytes = 0;
        //    boolean isRecv = true;

        //public ReceiveLittle(String addr, int mp, int yp, ReceiveState rs)
        //{
        //    try
        //    {
        //        Date begin;
        //        Date end;
        //        yourAddress = new String(addr);
        //        yourPort = yp;
        //        myPort = mp;
        //        socket = new DatagramSocket(myPort);
        //        socket.setReceiveBufferSize(1024 * 1024 * 5);
        //        receiveState = rs;
        //        //            timerCalRecvSpeed();
        //    }
        //    catch (SocketException ex)
        //    {
        //        //            Logger.getLogger(SendLittle.class.getName()).log(Level.SEVERE, null, ex);
        //        logger.error("{}", ex);
        //    }
        //    catch (IOException ioe)
        //    {
        //        ioe.printStackTrace();
        //    }
        //}
        //public ReceiveLittle(DatagramSocket clientSock, String address, int port, ReceiveState rs)
        //{
        //    // TODO Auto-generated constructor stub
        //    try
        //    {
        //        yourAddress = address;
        //        yourPort = port;
        //        socket = clientSock;
        //        socket.setReceiveBufferSize(1024 * 1024 * 5);
        //        receiveState = rs;
        //    }
        //    catch (SocketException e)
        //    {
        //        // TODO Auto-generated catch block
        //        e.printStackTrace();
        //    }
        //}
        public ReceiveLittle(UdpClient udpClient, IPEndPoint senderEP, ReceiveState rs)
        {
            InitLog4Net();
            udpReceiveClient = udpClient;
            host = senderEP;
            receiveState = rs;
            log.Info("建立接收程序");
        }
        public ReceiveLittle(String addr, int senderPort, int receiverPort, ReceiveState rs)
        {
            InitLog4Net();
            udpReceiveClient = new UdpClient(receiverPort);
            IPAddress HostIP = IPAddress.Parse(addr);
            host = new IPEndPoint(HostIP, senderPort);
            receiveState = rs;
            log.Info("建立接收程序");
        }

        public ReceiveLittle(UdpClient udpClient, String addr, int senderPort, ReceiveState rs)
        {
            InitLog4Net();
            udpReceiveClient = udpClient;
            IPAddress HostIP = IPAddress.Parse(addr);
            host = new IPEndPoint(HostIP, senderPort);
            receiveState = rs;
            log.Info("建立接收程序");
        }
        private static void InitLog4Net()
        {
            var logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/fileService/log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logCfg);
        }
        /**
         * 接收文件属性包
         * @return
         */
        //public FileProperty receiveFileProperty()
        //{
        //    //DatagramPacket receivePacket = null;
        //    //用于接收开始传输包
        //    byte[] receiveBuffer = new byte[FilePropertyTip[0]];
        //    try
        //    {
        //        //socket.setSoTimeout(0);
        //        //receivePacket = new DatagramPacket(receiveBuffer, receiveBuffer.length);
        //        udpReceiveClient.Client.ReceiveTimeout = 0;
        //    }
        //    catch (Exception e) {

        //    }
        //    while (true)
        //    {
        //        try
        //        {
        //            //socket.receive(receivePacket);
        //            //receiveState.addHaveReceiveBytes(receivePacket.getLength());
        //            receiveBuffer = udpReceiveClient.Receive(ref host);
        //            //检查标识
        //            //String str = new String(receiveBuffer, FilePropertyTip[1], 4);
        //            String str = Encoding.Default.GetString(receiveBuffer, FilePropertyTip[1], 4);
        //            //if (!str.equals(datagramFileProperty))
        //            if(!datagramFileProperty.Equals(str))
        //            {
        //                continue;
        //            }
        //            FileProperty fp = new FileProperty();
        //            //获取次随机数
        //            int numberRandom = ByteChange.byteToInt(receiveBuffer, FilePropertyTip[2]);
        //            fp.FileLength = ByteChange.byteToLong(receiveBuffer, FilePropertyTip[3]);
        //            int fileNameLength = ByteChange.byteToInt(receiveBuffer, FilePropertyTip[4]);
        //            fp.FileName = ByteChange.ByteToString(receiveBuffer, FilePropertyTip[5], fileNameLength);
        //            fp.MD5Str = ByteChange.ByteToString(receiveBuffer, FilePropertyTip[6], 32);
        //            receiveState.setFileLength(fp.FileLength);
        //            File f = new File(fp.FileName);
        //            if (f.exists())
        //            {
        //                logger.info("send md5:{}+receive md5:{}", fp.MD5Str, MD5.getFileMD5String(f));
        //                receiveState.setExistFileLength(f.length());
        //                if (fp.FileLength > f.length())
        //                {
        //                    fp.isOver = false;
        //                    fp.isExists = true;
        //                    logger.info("文件已存在,开始断点续传");
        //                    sendBreakPoint(numberRandom, f, fp);
        //                }
        //                else if (fp.FileLength == f.length())
        //                {
        //                    if (fp.MD5Str.equals(MD5.getFileMD5String(f)))
        //                    {//MD5一样
        //                        fp.isOver = true;
        //                        fp.isExists = true;
        //                        logger.info("文件已传输完成");
        //                        sendBreakPoint(numberRandom, f, fp);
        //                    }
        //                    else
        //                    {
        //                        fp.isOver = false;
        //                        fp.isExists = false;
        //                        logger.info("文件出错，重新接受");
        //                        sendFilePropertyAck(numberRandom);
        //                    }
        //                }
        //                else
        //                {
        //                    fp.isOver = false;
        //                    fp.isExists = false;
        //                    logger.info("文件出错，重新接受");
        //                    sendFilePropertyAck(numberRandom);
        //                }
        //            }
        //            else
        //            {
        //                fp.isOver = false;
        //                fp.isExists = false;
        //                sendFilePropertyAck(numberRandom);
        //                System.out.println("文件不存在，接收到文件属性包");
        //            }
        //            return fp;
        //        }
        //        catch (Exception ex)
        //        {
        //            //                Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
        //            logger.error("{}", ex);
        //        }
        //    }
        //}

        //private void SendBreakPoint(int numberRandom, File f, FileProperty fp)
        //{
        //    DatagramPacket sendPacket = null;
        //    byte sendBuffer[] = new byte[FileBreakPointTip[0]];
        //    //填充标识
        //    ByteChange.StringToByte(datagramFileBreakPoint, sendBuffer, FileBreakPointTip[1], 4);
        //    //填充次随机数
        //    ByteChange.intToByte(sendBuffer, FileBreakPointTip[2], numberRandom);
        //    //填充文件长度
        //    ByteChange.longToByte(sendBuffer, FileBreakPointTip[3], fp.FileLength);
        //    //填充已传文件长度
        //    ByteChange.longToByte(sendBuffer, FileBreakPointTip[4], f.length());
        //    //填充报尾
        //    ByteChange.StringToByte(datagramTail, sendBuffer, FileBreakPointTip[5], 4);
        //    try
        //    {
        //        sendPacket = new DatagramPacket(sendBuffer, sendBuffer.length, InetAddress.getByName(yourAddress), yourPort);
        //        socket.send(sendPacket);
        //    }
        //    catch (Exception ex)
        //    {
        //        //            Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
        //        logger.error("{}", ex);
        //    }
        //}
        //    
        //	private int punchDirectAckToClient(DatagramPacket dp, byte[] receiveBuffer) {
        //		// TODO Auto-generated method stub
        //		int randNum = ByteChange.byteToInt(receiveBuffer, CTCPunchDirectTip[2]);
        //		long sID = ByteChange.byteToLong(receiveBuffer, CTCPunchDirectTip[3]);
        //		long rID = ByteChange.byteToLong(receiveBuffer, CTCPunchDirectTip[4]);
        //		String strTail=new String(receiveBuffer,CTCPunchDirectTip[5],4);
        //		if(!strTail.equals(datagramTail)){
        //			return -1;
        //		}
        //		DatagramPacket sendPacket = null;
        //		byte sendBuffer[] = new byte[CTCPunchDirectAckTip[0]];
        //		// 填充开始传输包
        //		ByteChange.StringToByte(datagramCTCPunchDirectAck, sendBuffer, CTCPunchDirectAckTip[1], 4);
        //		ByteChange.intToByte(sendBuffer, CTCPunchDirectAckTip[2], randNum);
        //		ByteChange.longToByte(sendBuffer, CTCPunchDirectAckTip[3], sID);
        //		ByteChange.longToByte(sendBuffer, CTCPunchDirectAckTip[4], rID);
        //		ByteChange.StringToByte(datagramTail, sendBuffer, CTCPunchDirectAckTip[5], 4);
        //		try {
        //			System.out.println("sendAck="+datagramCTCPunchDirectAck+randNum+sID+rID+datagramTail+dp.getAddress()+dp.getPort());
        //			sendPacket = new DatagramPacket(sendBuffer, sendBuffer.length, dp.getAddress(),dp.getPort());
        //			socket.send(sendPacket);
        //		} catch (Exception e) {
        //			// TODO Auto-generated catch block
        //			e.printStackTrace();
        //		}
        //
        //		return 1;
        //	}
        /*发送文件属性确认包*/
        //public void sendFilePropertyAck(int numberRandom)
        //{
        //    DatagramPacket sendPacket = null;
        //    byte sendBuffer[] = new byte[FilePropertyAckTip[0]];
        //    //填充标识
        //    ByteChange.StringToByte(datagramFilePropertyAck, sendBuffer, FilePropertyAckTip[1], 4);
        //    //填充次随机数
        //    ByteChange.intToByte(sendBuffer, FilePropertyAckTip[2], numberRandom);
        //    //填充报尾
        //    ByteChange.StringToByte(datagramTail, sendBuffer, FilePropertyAckTip[3], 4);
        //    try
        //    {
        //        sendPacket = new DatagramPacket(sendBuffer, sendBuffer.length, InetAddress.getByName(yourAddress), yourPort);
        //        socket.send(sendPacket);
        //    }
        //    catch (Exception ex)
        //    {
        //        //            Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
        //        logger.error("{}", ex);
        //    }
        //}
        //文件属性内部类
        //public class FileProperty
        //{
        //    public boolean isOver;
        //    public long FileLength;
        //    public String FileName;
        //    public String MD5Str;
        //    public boolean isExists;

        //    FileProperty()
        //    {
        //        isOver = false;
        //        FileLength = 0;
        //        isExists = false;
        //    }
        //}

        /*分组接收*/
        public int ReceiveAll(byte[] littleBuffer)
        {
            byte[] buffer = new byte[dataLength * groupNum];
            randomStack = new CircleStack(100);
            int numberRandom = ReceiveStart();//接收开始传输包
            int allLength = 0;

            while (true)
            {
                int length = ReceiveGroup(numberRandom, buffer);
                if (length != -1)
                {
                    for (int i = 0; i < length; i++)
                    {
                        littleBuffer[i + allLength] = buffer[i];
                    }
                    allLength += length;
                }
                else
                {
                    SendOverAck(numberRandom);
                    //                if(allLength==3){
                    //                    String end=new String(littleBuffer,0,3);
                    //                    System.out.println("end-->"+end);
                    //                }
                    //receiveState.addHavaReceivefileLength(allLength);
                    return allLength;
                }
            }
        }

        /*接收开始传输包*/
        public int ReceiveStart()
        {
            //DatagramPacket sendPacket = null, receivePacket = null;
            //用于发送开始确认包
            byte[] sendBuffer = new byte[StartAckTip[0]];
            //用于接收开始传输包
            byte[] receiveBuffer = new byte[FilePropertyTip[0]];
            try
            {
                udpReceiveClient.Client.ReceiveTimeout = 0;
            }
            catch (Exception e) {
                log.Error("ReceiveStart",e);
            }
            while (true)
            {
                try
                {
                    //socket.receive(receivePacket);
                    receiveBuffer = udpReceiveClient.Receive(ref host);
                    receiveState.AddHaveReceiveBytes(receiveBuffer.Length);
                    //检查标识
                    String str = Encoding.Default.GetString(receiveBuffer, StartTip[1], 4);
                    if (!datagramStart.Equals(str))
                    {
                        continue;
                    }
                    //获取次随机数
                    int numberRandom = ByteChange.ByteToInt(receiveBuffer, StartTip[2]);
                    SendStartAck(numberRandom);
                    //                System.out.println("接收到开始传输包");
                    return numberRandom;
                }
                catch (Exception ex)
                {
                    //                Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
                    //logger.error("{}", ex);
                    //MessageBox.Show(ex.ToString());
                    log.Error(ex.ToString());
                }
            }
        }

        /*发送开始确认包*/
        public void SendStartAck(int numberRandom)
        {
            //DatagramPacket sendPacket = null;
            byte[] sendBuffer = new byte[StartAckTip[0]];
            //填充标识
            ByteChange.StringToByte(datagramStartAck, sendBuffer, StartAckTip[1], 4);
            //填充次随机数
            ByteChange.IntToByte(sendBuffer, StartAckTip[2], numberRandom);
            //填充报尾
            ByteChange.StringToByte(datagramTail, sendBuffer, StartAckTip[3], 4);
            try
            {
                udpReceiveClient.Send(sendBuffer, sendBuffer.Length, host);
            }
            catch (Exception ex)
            {
                //            Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
                //logger.error("{}", ex);
                log.Error(ex.ToString());
            }
        }
        /*接收一次分组*/
        //返回值为该数组中数据的长度.并且,此函数进行传送结束的检查.发现结束,返回-1
        public int ReceiveGroup(int numberRandom, byte[] buffer)
        {
            //接收分组头
            Rdh_return rdh = ReceiveDatagramHead(numberRandom);
            //       System.out.println("收到分组头,随机数:"+rdh.random+",分组数量:"+rdh.number);
            //记下随机数及分组数量
            //int random = rdh.random;
            //int number = rdh.number;
            int random = rdh.Random;
            int number = rdh.Number;
            if (number == 0)
            {
                return -1;
            }
            randomStack.Add(random);
            littleRandomStack = new CircleStack(100);
            //设定数据总长度
            int lengthAll = 0;
            //由于只有最后一组的数据长度不是dataLength,所以,先计算前number-1个分组的总长度
            lengthAll = dataLength * (number - 1);
            //用于记录成功接收了哪些分组
            bool[] receiveTip = new bool[number];
            for (int i = 0; i < number; i++)
            {
                receiveTip[i] = false;
            }
            //预先计算各组应存入buffer的起始下标
            //由于除了最后一组,其它组的数据长度一定是dataLength.所以计算起始下标非常简单
            int[] startTip = new int[number];
            for (int i = 0; i < number; i++)
            {
                startTip[i] = i * dataLength;
            }
            //进入大循环,即连续去接收分组,直到所有分组完毕
            while (true)
            {
                int result = GetDatagramData(numberRandom, random, buffer, receiveTip, startTip);
                //            System.out.println("收到分组包"+result);
                //如果接收到的是一个分组.要记录其长度.由于,除了最后一组,其他的长度一定是dataLength
                //所以,如果这个length不是dataLength,说明是最后一组的长度
                if (result > 0)
                {
                    if (result != dataLength)
                    {
                        lengthAll += result;
                    }
                    continue;
                }
                //如果接收到是分组头
                else if (result == -2)
                {
                    //发送分组头确认包,然后循环
                    SendDatagramHeadAck(numberRandom, random);
                    continue;
                }
                //如果是新的发送完毕包
                else if (result == -3)
                {
                    //发送请求重发包
                    //在发送请求重发包的时候,是要检查是否全部成功接收的,所以,这里不再重新检查,而是使用它检查的结果
                    bool flag = SendDatagramReqAgain(numberRandom, random, littleRandomStack.GetLast(), receiveTip);
                    //System.out.println("发送请求重发包");
                    if (flag)
                    {
                        break;
                    }
                    continue;
                }
                //如果是上次的发送完毕包
                else if (result == -4)
                {
                    //如果上次发送的不是0请求重发包,则重发
                    //非0请求重发包的长度,一定是1018
                    //System.out.println("接收到上次的发送完毕包:");
                    try
                    {
                        if (lastReqAgainBuffer.Count == ReqAgainTip[0])
                        {
                        
                            //System.out.println("重发非0请求重发包");
                            //socket.send(lastReqAgain);

                            udpReceiveClient.Send(lastReqAgainBuffer.ToArray(), lastReqAgainBuffer.Count, host);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.ToString());
                        //MessageBox.Show(ex.ToString());
                        //                        Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
                        //logger.error("{}", ex);
                    }
                    continue;
                }
            }
            //这里有个很蛋疼的问题.由于不确定最后一组是否完整,只有在接收到最后一个分组时才知道其数据长度
            //而接收分组是在单独的函数中完成,函数返回的是接收的数据长度.在循环中,如果发现数据长度不是dataLength
            //就认为它是最后一组,并将其长度加入lengthAll.但是,有一个严重的问题,因为,每个块,只有最后一个块的最后一个
            //分组的数据长度可能不是dataLength.所以,前面的块,在进行计算的时候,有会忽略它最后一组的长度.导致以后的块
            //连续错误
            if (lengthAll == (number - 1) * dataLength)
            {
                lengthAll += dataLength;
            }
            return lengthAll;
        }
        /*接收分组头*/
        //1.不接收超时. 2.接收成功,会调用发送分组头确认包
        public Rdh_return ReceiveDatagramHead(int numberRandom)
        {
            byte[] buffer = new byte[FilePropertyTip[0]];
            //DatagramPacket packet = null;
            Rdh_return rdh = new Rdh_return();
            //设置不接收超时

            udpReceiveClient.Client.ReceiveTimeout = 0;
            while (true)
            {
                try
                {
                    //packet = new DatagramPacket(buffer, buffer.length);//,InetAddress.getByName(yourAddress),yourPort
                    ByteChange.CleanByte(buffer);
                    //socket.receive(packet);
                    //receiveState.addHaveReceiveBytes(packet.getLength());
                    buffer = udpReceiveClient.Receive(ref host);
                    //检查分组头标识
                    //String str = new String(buffer, HeadTip[1], 4);
                    String str = Encoding.Default.GetString(buffer, HeadTip[1], 4);
                    //if (!str.equals(datagramHead))
                    if (!datagramHead.Equals(str))
                    {
                        /*既然不是分组头,重新接收是肯定的.只不过,有些情况,需要处理一下再重新接收*/
                        //如果是开始传输包
                        //if (str.equals(datagramStart))
                        if (datagramStart.Equals(str))
                        {
                            int num = ByteChange.ByteToInt(buffer, StartTip[2]);
                            if (num == numberRandom)
                            {
                                SendStartAck(numberRandom);
                                //                            System.out.println("在分组头中接收到开始传输包");
                                continue;
                            }
                        }
                        //如果是发送完毕包
                        //if (str.equals(datagramFinish))
                        if (datagramFinish.Equals(str))
                        {
                            //先检其次随机数
                            if (ByteChange.ByteToInt(buffer, FinishTip[2]) == numberRandom)
                            {
                                //检验是不是上一次的发送完毕包
                                //如果是的,说明上次发的0请求重发包没发成功
                                //先检查其块随机数是不是上一次的
                                if (randomStack.GetLast() != -1 && ByteChange.ByteToInt(buffer, FinishTip[3]) == randomStack.GetLast())
                                {
                                    //再检查其小随机数,是不是上次的(即最后一次发过来的)
                                    if (littleRandomStack != null)
                                    {
                                        //如果littelRandomStack是null,说明这是第一次接收分组头,当然不可能要重发
                                        if (littleRandomStack.GetLast() == ByteChange.ByteToInt(buffer, FinishTip[4]))
                                        {
                                            //如果小随机数也匹配,则发送上次的0请求重发包
                                            if (lastReqAgainBuffer != null)
                                            {
                                                //socket.send(lastReqAgain);
                                                udpReceiveClient.Send(lastReqAgainBuffer.ToArray(), lastReqAgainBuffer.Count, host);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //如果是传输结束包,则将0分组数量填充入rdh,然后返回
                        //if (str.equals(datagramStop))
                        if (datagramStop.Equals(str))
                        {
                            rdh.Number = 0;
                            return rdh;
                        }
                        continue;
                    }
                    //检查该包是否完整
                    //str = new String(buffer, HeadTip[5], 4);
                    str = Encoding.Default.GetString(buffer, HeadTip[5], 4);
                    //if (!str.equals(datagramTail))
                    if (!datagramTail.Equals(str))
                    {
                        continue;
                    }
                    //检查完毕,获取随机数
                    rdh.Random = ByteChange.ByteToInt(buffer, HeadTip[3]);
                    //获取分组数量
                    rdh.Number = ByteChange.ByteToShort(buffer, HeadTip[4]);
                    SendDatagramHeadAck(numberRandom, rdh.Random);
                    break;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    //                Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
                    //logger.error("{}", ex);
                }
            }
            return rdh;
        }
        /*发送分组头确认包*/
        public void SendDatagramHeadAck(int numberRandom, int blockRandom)
        {
            //        System.out.println("发送分组头确认包");
            byte[] buffer = new byte[HeadAckTip[0]];
            //DatagramPacket packet = null;
            //填充分组头确认包标识
            ByteChange.StringToByte(datagramHeadAck, buffer, HeadAckTip[1], 4);
            //填充次随机数
            ByteChange.IntToByte(buffer, HeadAckTip[2], numberRandom);
            //填充块随机数
            ByteChange.IntToByte(buffer, HeadAckTip[3], blockRandom);
            //填充报尾
            ByteChange.StringToByte(datagramTail, buffer, HeadAckTip[4], 4);
            //填充完成,发送报文
            try
            {
                //packet = new DatagramPacket(buffer, buffer.length, InetAddress.getByName(yourAddress), yourPort);
                //socket.send(packet);
                udpReceiveClient.Send(buffer, buffer.Length, host);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                //Logger.getLogger(Receive.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
        /*发送请求重发包*/
        public bool SendDatagramReqAgain(int numberRandom, int blockRandom, int littleRandom, bool[] receiveTip)
        {
            //先判断是否全部接收成功
            bool success = true;
            for (int i = 0; i < receiveTip.Length; i++)
            {
                if (receiveTip[i] == false)
                {
                    success = false;
                    break;
                }
            }
            byte[] buffer = new byte[ReqAgainTip[0]];
            //DatagramPacket packet;
            //填充请求重发包标识
            ByteChange.StringToByte(datagramReqAgain, buffer, ReqAgainTip[1], 4);
            //填充次随机数
            ByteChange.IntToByte(buffer, ReqAgainTip[2], numberRandom);
            //填充块随机数
            ByteChange.IntToByte(buffer, ReqAgainTip[3], blockRandom);
            //填充小随机数
            ByteChange.IntToByte(buffer, ReqAgainTip[4], littleRandom);
            //填充数据长度
            if (success)
            {
                //全部成功接收,所以应该发送的是0请求重发包
                ByteChange.ShortToByte(buffer, ReqAgainTip[5], (short)0);
                //由于长度为0,所以没有数据,直接填充报尾
                ByteChange.StringToByte(datagramTail, buffer, ReqAgainTip[6], 4);
                //          System.out.println("未重传数据包");
            }
            else
            {
                //解析receiveTip,遇到false就将下标存入buffer
                int length = 0;
                //            int reqAgainCount = 0;
                //receiveState.setNumReceivePackets(receiveTip.length);
                //receiveState.setNumResendPackets(0);
                for (int i = 0; i < receiveTip.Length; i++)
                {
                    if (receiveTip[i] == false)
                    {
                        ByteChange.ShortToByte(buffer, ReqAgainTip[6] + length * 2, (short)i);
                        length++;
                        //receiveState.addNumResendPackets();
                    }
                }

                //            System.out.println("重传数据包数："+reqAgainCount+"丢包率："+(float)reqAgainCount/receiveTip.length);
                //填充数据长度
                ByteChange.ShortToByte(buffer, ReqAgainTip[5], (short)length);
                //填充报尾
                ByteChange.StringToByte(datagramTail, buffer, ReqAgainTip[7], 4);
            }
            try
            {
                //填充完毕,发送
                if (success)
                {
                    //packet = new DatagramPacket(buffer, ReqAgainTip[6] + 4, InetAddress.getByName(yourAddress), yourPort);
                    udpReceiveClient.Send(buffer, ReqAgainTip[6] + 4, host);
                    //buffer.CopyTo(lastReqAgainBuffer, ReqAgainTip[6] + 4);
                    lastReqAgainBuffer.Clear();
                    lastReqAgainBuffer.AddRange(buffer);
                }
                else
                {
                    //packet = new DatagramPacket(buffer, ReqAgainTip[0], InetAddress.getByName(yourAddress), yourPort);
                    udpReceiveClient.Send(buffer, ReqAgainTip[0], host);
                    lastReqAgainBuffer.Clear();
                    lastReqAgainBuffer.AddRange(buffer);
                    //buffer.CopyTo(lastReqAgainBuffer, ReqAgainTip[0] + 4);
                }
                //lastReqAgainBuffer = buffer;
                //lastReqAgain = packet;
                ////System.out.println("发送请求重发包,长度"+lastReqAgain.getLength());
                //socket.send(packet);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                //            Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
                //logger.error("{}", ex);
            }
            return success;
        }
        /*接收一个分组*/
        /*返回值说明:
         * 1.如果正常接收,返回读取数据的长度
         * 2.如果接收超时,返回-1  //此句已经无效
         * 3.如果接收到本分组的分组头,返回-2
         * 4.如果接收到新的发送完毕包,返回-3
         * 5.如果接收到上次的发送完毕包,返回-4
         * 6.其他情况,返回-5
         */
        //此函数只用于接收,不发送任何信息.遇到错误,只是将错误标识返回.
        //如果接收到的是分组包,将其存入buffer中相应位置,返回数据的长度
        public int GetDatagramData(int numberRandom, int random, byte[] buffer, bool[] receiveTip, int[] startTip)
        {
            //设置不接收超时
            //try
            //{
            //    socket.setSoTimeout(0);
            //}
            //catch (SocketException ex)
            //{
            //    //            Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
            //    logger.error("{}", ex);
            //}
            udpReceiveClient.Client.ReceiveTimeout = 0;
            byte[] receiveBuffer = new byte[DataTip[0]];
            //DatagramPacket packet = new DatagramPacket(receiveBuffer, receiveBuffer.length);
            try
            {
                ByteChange.CleanByte(receiveBuffer);
                //socket.receive(packet);
                receiveBuffer = udpReceiveClient.Receive(ref host);
                //receiveState.addHaveReceiveBytes(packet.getLength());
                //就算收到未接收请求重发包,有意义的随机数也应该是与本分组随机数相同的
                //次随机数
                if (ByteChange.ByteToInt(receiveBuffer, DataTip[2]) != numberRandom)
                {
                    return -5;
                }
                //所以,先判断块随机数
                int r = ByteChange.ByteToInt(receiveBuffer, DataTip[3]);
                if (r != random)
                {
                    return -5;
                }
                //判断分组标识
                //String s = new String(receiveBuffer, DataTip[1], 4);
                String s = Encoding.Default.GetString(receiveBuffer, DataTip[1], 4);
                //if (!s.equals(datagramData))
                if (!datagramData.Equals(s))
                {
                    //如果此包不是分组包
                    //判断其是否是本分组的分组头.由于随机数已经匹配,所以,只需要判断标识
                    //if (s.equals(datagramHead))
                    if (datagramHead.Equals(s))
                    {
                        return -2;
                    }
                    //再判断其是否是发送完毕包
                    //if (s.equals(datagramFinish))
                    if (datagramFinish.Equals(s))
                    {
                        //先判断,是否是新的发送完毕包
                        r = ByteChange.ByteToInt(receiveBuffer, FinishTip[4]);
                        if (!littleRandomStack.InIt(r))
                        {
                            littleRandomStack.Add(r);
                            return -3;
                        }
                        //如果是上次的发送完毕包
                        int r2 = littleRandomStack.GetLast();
                        if (r2 != -1 && r2 == r)
                        {
                            return -4;
                        }
                    }
                    //否则,返回-5
                    return -5;
                }
                //至此已经是分组包了,先判断一下包的完整性
                //因为,如果包是不完整的,它的数据就是有问题的,是不能接收的
                //s = new String(receiveBuffer, DataTip[7], 4);
                s = Encoding.Default.GetString(receiveBuffer, DataTip[7], 4);
                //if (!s.equals(datagramTail))
                if (!datagramTail.Equals(s))
                {
                    return -5;
                }
                //包是完整的,获取包的序号
                int serial = ByteChange.ByteToShort(receiveBuffer, DataTip[4]);
                int number = ByteChange.ByteToShort(receiveBuffer, DataTip[5]);
                //如果该序列的包未被正确接收
                if (!receiveTip[serial])
                {
                    receiveTip[serial] = true;
                    for (int i = 0; i < number; i++)
                    {
                        buffer[i + startTip[serial]] = receiveBuffer[i + DataTip[6]];
                    }
                    return number;
                }
                //如果已经被正确接收了,则丢弃该包
                else
                {
                    return -5;
                }
            }
            catch (Exception ex)
            {
                //Logger.getLogger(Receive.class.getName()).log(Level.SEVERE, null, ex);
                if (ex is TimeoutException)
                {
                    return -1;
                }
                if (ex is SocketException)
                {
                    return -1;
                }
                return -5;
            }
        }
        /*发送传输结束确认包*/
        public void SendOverAck(int numberRandom)
        {
            //DatagramPacket packetOverAck = null, packetOver = null;
            byte[] sendBuffer = new byte[StopAckTip[0]];
            byte[] receiveBuffer = new byte[StopTip[0]];
            //设置接收超时1000ms
            try
            {
                //socket.setSoTimeout(1000);
                udpReceiveClient.Client.ReceiveTimeout = 1000;
                /*填充结束确认包*/
                //填充确认包标识
                ByteChange.StringToByte(datagramStopAck, sendBuffer, StopAckTip[1], 4);
                //填充次随机数
                ByteChange.IntToByte(sendBuffer, StopAckTip[2], numberRandom);
                //填充报尾
                ByteChange.StringToByte(datagramTail, sendBuffer, StopAckTip[3], 4);
                /*填充完毕*/
                //packetOverAck = new DatagramPacket(sendBuffer, sendBuffer.length, InetAddress.getByName(yourAddress), yourPort);
                //packetOver = new DatagramPacket(receiveBuffer, receiveBuffer.length);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                //            Logger.getLogger(ReceiveLittle.class.getName()).log(Level.SEVERE, null, ex);
                //logger.error("{}", ex);
            }
            bool canSendAck = true;
            while (true)
            {
                try
                {
                    if (canSendAck)
                    {
                        //socket.send(packetOverAck);
                        udpReceiveClient.Send(sendBuffer, sendBuffer.Length, host);
                    }
                    ByteChange.CleanByte(receiveBuffer);
                    //socket.receive(packetOver);
                    receiveBuffer = udpReceiveClient.Receive(ref host);
                    //receiveState.addHaveReceiveBytes(packetOver.getLength());
                    //                System.out.println("Over:收到包,IP"+packetOver.getAddress());
                    //如果接收到传输结束包,就继续循环
                    //String str = new String(receiveBuffer, StopTip[1], 4);
                    //                System.out.println("Over:收到包,str"+str);
                    String str = Encoding.Default.GetString(receiveBuffer, StopTip[1], 4);
                    //if (str.equals(datagramStop))
                    if (datagramStop.Equals(str))
                    {
                        //检查次随机数
                        int num = ByteChange.ByteToInt(receiveBuffer, StopTip[2]);
                        if (num == numberRandom)
                        {
                            canSendAck = true;
                            continue;
                        }
                    }

                    //如果收到开始传输包,并且其次随机数与这次的不一样,则退出
                    //if (str.equals(datagramStart))
                    if (datagramStart.Equals(str))
                    {
                        //检查次随机数
                        if (ByteChange.ByteToInt(receiveBuffer, StartTip[2]) != numberRandom)
                        {
                            break;
                        }
                    }
                    //如果是其他包,也继续循环,但是一下轮不发送确认包
                    canSendAck = false;
                    continue;
                }
                catch (Exception e)
                {
                    if (e is TimeoutException)
                    {
                        //如果接收超时,则退出循环
                        //                    System.out.println("接收超时");
                        break;
                    }
                    if(e is SocketException)
                    {
                        break;
                    }
                }
            }
        }
    }
    //在此,由于需要返回两个信息: 1.分组的随机数 2.分组的数量 所以需要定义一个内部类
    public class Rdh_return
    {
        int random;
        int number;

        public int Random { get => random; set => random = value; }
        public int Number { get => number; set => number = value; }
    }
}
