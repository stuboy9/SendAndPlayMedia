using AreaParty.info;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

namespace http
{


    using Node = node.Node;
    using NodeContainer = node.NodeContainer;
    



    /// <summary>
    /// A simple, tiny, nicely embeddable HTTP 1.0 server in Java
    /// Modified from NanoHTTPD, you can find it here
    /// http://elonen.iki.fi/code/nanohttpd/
    /// </summary>
    public class HttpServer
	{
        // ==================================================
        // API parts
        // ==================================================
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        HTTPSession myHTTPSession;
        private Thread myHTTPSession_thread;
        private static bool CloseAll = false;
        private static string ip = AreaParty.Program.ip;
        //public bool canstop = true;
        /// <summary>
        /// Override this to customize the server.<para>
        /// 
        /// (By default, this delegates to serveFile() and allows directory listing.)
        /// 
        /// </para>
        /// </summary>
        /// <param name="uri">	Percent-decoded URI without parameters, for example "/index.cgi" </param>
        /// <param name="method">	"GET", "POST" etc. </param>
        /// <param name="parms">	Parsed, percent decoded parameters from URI and, in case of POST, data. </param>
        /// <param name="header">	Header entries, percent decoded </param>
        /// <returns> HTTP response, see class Response for details </returns>
        public virtual Response serve(string uri, string method, Dictionary<string,string> header, Dictionary<string, string> parms, Dictionary<string, string> files)
		{
			Console.WriteLine(method + " '" + uri + "' ");

            if(header.Count > 0)
            {
                foreach(KeyValuePair<string,string> head in header)
                {
                    Console.WriteLine("  HDR: '" + head.Key + "' = '" + head.Value + "'");
                }
            }
            if (parms.Count > 0)
            {
                foreach (KeyValuePair<string, string> parm in parms)
                {
                    Console.WriteLine("  HDR: '" + parm.Key + "' = '" + parm.Value + "'");
                }
            }
            if (files.Count > 0)
            {
                foreach (KeyValuePair<string, string> file in files)
                {
                    Console.WriteLine("  HDR: '" + file.Key + "' = '" + file.Value + "'");
                }
            }
            
            int urix = uri.IndexOf("/");
            string itemId = uri.Remove(urix,1);
			
			Console.WriteLine("----GPF----itemID=" + itemId);
			
			string newUri = null;
			if (NodeContainer.hasNode(itemId))
			{
				Console.WriteLine("---GPF---ContentTree.hasNode");
				Node node = NodeContainer.getNode(itemId);
                newUri = node.path;
                Console.WriteLine("----GPF----nerwUri=" + newUri);
			}

			if (!string.ReferenceEquals(newUri, null))
			{
				uri = newUri;
			}
			Console.WriteLine("---GPF----myRootDir=" + new FileInfo(myRootDir).FullName);
			return ServeFile(uri, header, myRootDir, false);
		}

		/// <summary>
		/// HTTP response.
		/// Return one of these from serve().
		/// </summary>
		public class Response
		{
			private readonly HttpServer outerInstance;

			/// <summary>
			/// Default constructor: response = HTTP_OK, data = mime = 'null'
			/// </summary>
			public Response(HttpServer outerInstance)
			{
				this.outerInstance = outerInstance;
				this.status = HTTP_OK;
			}

			/// <summary>
			/// Basic constructor.
			/// </summary>
			public Response(HttpServer outerInstance, string status, string mimeType, System.IO.Stream data)
			{
				this.outerInstance = outerInstance;
				this.status = status;
				this.mimeType = mimeType;
				this.data = data;
			}

			/// <summary>
			/// Convenience method that makes an InputStream out of
			/// given text.
			/// </summary>
			public Response(HttpServer outerInstance, string status, string mimeType, string txt)
			{
				this.outerInstance = outerInstance;
				this.status = status;
				this.mimeType = mimeType;
				try
				{
					this.data = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(txt));
				}
				catch (UnsupportedEncodingException uee)
				{
					Console.WriteLine(uee.ToString());
					Console.Write(uee.StackTrace);
				}
			}

			/// <summary>
			/// Adds given line to the header.
			/// </summary>
			public virtual void addHeader(string name, string value)
			{
				header.Add(name, value);
			}

			/// <summary>
			/// HTTP status code after processing, e.g. "200 OK", HTTP_OK
			/// </summary>
			public string status;

			/// <summary>
			/// MIME type of content, e.g. "text/html"
			/// </summary>
			public string mimeType;

			/// <summary>
			/// Data of the response, may be null.
			/// </summary>
			public System.IO.Stream data;

			/// <summary>
			/// Headers for the HTTP response. Use addHeader()
			/// to add lines.
			/// </summary>
			public Dictionary<string,string> header = new Dictionary<string,string>();
		}

		/// <summary>
		/// Some HTTP response status codes
		/// </summary>
		public const string HTTP_OK = "200 OK", HTTP_PARTIALCONTENT = "206 Partial Content", HTTP_RANGE_NOT_SATISFIABLE = "416 Requested Range Not Satisfiable", HTTP_REDIRECT = "301 Moved Permanently", HTTP_FORBIDDEN = "403 Forbidden", HTTP_NOTFOUND = "404 Not Found", HTTP_BADREQUEST = "400 Bad Request", HTTP_INTERNALERROR = "500 Internal Server Error", HTTP_NOTIMPLEMENTED = "501 Not Implemented";

		/// <summary>
		/// Common mime types for dynamic content
		/// </summary>
		public const string MIME_PLAINTEXT = "text/plain", MIME_HTML = "text/html", MIME_DEFAULT_BINARY = "application/octet-stream", MIME_XML = "text/xml";

        // ==================================================
        // Socket & server code
        // ==================================================

        /// <summary>
        /// Starts a HTTP server to given port.<para>
        /// Throws an IOException if the socket is already in use
        /// </para>
        /// </summary>

        public HttpServer(int port)
		{
			this.myTcpPort = port; 
        }

        public void listen()
        {

            //IPAddress ipAddr = Dns.Resolve(Dns.GetHostName()).AddressList[0];//获得当前IP地址
            //string ip = ipAddr.ToString();

            myRootDir = new FileInfo("/").FullName;

            IPEndPoint hostEP = new IPEndPoint(IPAddress.Parse(ip), myTcpPort);
            myServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            myServerSocket.Bind(hostEP);
            myServerSocket.Listen(1);
            try
            {
                myThread = new Thread(() =>
                {

                    try
                    {
                        while (!CloseAll)
                        {

                            try
                            {
                                Console.WriteLine("accept 等待连接");
                                myHTTPSession = new HTTPSession(this, myServerSocket.Accept());
                                Console.WriteLine("accept 启动");
                                myHTTPSession_thread = new Thread(myHTTPSession.Run)
                                {
                                    IsBackground = true
                                };
                                myHTTPSession_thread.Start();
                            }
                            catch (IOException e)
                            {
                                stop();
                                Console.WriteLine("{0}: The write operation could not " + "be performed because the specified " + "part of the file is locked.", e.GetType().Name);
                                Console.WriteLine("服务器会话启动失败！" + e.Message);
                                log.InfoFormat("服务器会话启动失败！" + e.Message);
                            }
                        }
                        myHTTPSession_thread.Abort();
                        myThread.Abort();
                    }
                    catch (IOException e)
                    {
                        stop();
                        Console.WriteLine("{0}: The write operation could not " + "be performed because the specified " + "part of the file is locked.", e.GetType().Name);
                        Console.WriteLine("服务器启动失败！" + e.Message);
                        log.InfoFormat("服务器启动失败！" + e.Message);
                    }
                })
                {
                    IsBackground = true
                };
                myThread.Start();
            }
            catch
            {
                stop();
                myThread.Abort();
            }
            

        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public virtual void stop()
		{
			try
			{
				myServerSocket.Close();
				myThread.Join();
			}
			catch (IOException)
			{
			}
			catch (ThreadInterruptedException)
			{
			}
		}

		/// <summary>
		/// Handles one session, i.e. parses the HTTP request
		/// and returns the response.
		/// </summary>
        /// 

		private class HTTPSession
        {
			private readonly HttpServer outerInstance;

			public HTTPSession(HttpServer outerInstance, Socket s)
			{
				this.outerInstance = outerInstance;
				mySocket = s;
			}

			public virtual void Run()
			{
                Console.WriteLine("启动一个新线程");
				try
				{
                    int bufsize = 8192 * 8192;
                    byte[] buffer = new byte[bufsize];
                    byte[] buf = new byte[bufsize];
                    List<byte> head_mes = new List<byte>();
                    int rlen = mySocket.Receive(buffer);
                    if (buffer == null)
                    {
                        return;
                    }
                    try
                    {
                        while (rlen > 0)
                        {
                            for (int j = 0; j < rlen; j++)
                            {
                                head_mes.Add(buffer[j]);
                            }
                            if (rlen < buffer.Length)
                            {
                                break;
                            }

                        }
                    }
                    catch
                    {

                    }
                    if (head_mes.Count > 0)
                    {  
                        buf = head_mes.ToArray();
                    }
                   
                    MemoryStream hbis = new MemoryStream(buf, 0, rlen);
                    StreamReader hin = new StreamReader(hbis);
                    
                    Console.WriteLine("streamReader hbis is:{0}", hbis);
					Dictionary<string, string> pre = new Dictionary<string, string>();
                    Dictionary<string, string> parms = new Dictionary<string, string>();
                    Dictionary<string, string> header = new Dictionary<string, string>();
                    Dictionary<string, string> files = new Dictionary<string, string>();

					decodeHeader(hin, pre, parms, header);
                    if (CloseAll)
                    {
                        log.Info(string.Format("接收到CloseAll指令为{0}，退出线程", CloseAll));
                        return;
                    }
                    pre.TryGetValue("method", out string method);
                    pre.TryGetValue("uri", out string uri);
                    if (method == "throw")
                    {
                        Console.WriteLine("捕获一个空连接，退出线程");
                        return;
                    }               
                    hin.Dispose();
                    hbis.Dispose();

                    if (header.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> head in header)
                        {
                            Console.WriteLine("  HDR: '" + head.Key + "' = '" + head.Value + "'");
                        }
                    }

                    long size = 0x7FFFFFFFFFFFFFFFL;
                    header.TryGetValue("content-length", out string contentLength);
                    if (!string.ReferenceEquals(contentLength, null))
					{
						try
						{
							size = int.Parse(contentLength);
						}
						catch (FormatException)
						{
						}
					}

                    // We are looking for the byte separating header from body.
                    // It must be the last byte of the first two sequential new lines.
                    int splitbyte = 0;
					bool sbfound = false;
                    if (header.Count == 0)
                    {
                        splitbyte = rlen;
                        sbfound = true;
                    }
                    else
                    {
                        while (splitbyte < rlen)
                        {
                            if (buf[splitbyte] == (byte)'\r' && buf[++splitbyte] == (byte)'\n' && buf[++splitbyte] == (byte)'\r' && buf[++splitbyte] == (byte)'\n')
                            {
                                sbfound = true;
                                break;
                            }
                            splitbyte++;
                        }
                        splitbyte++;
                    }
					
					

					// Write the part of body already read to ByteArrayOutputStream f
					System.IO.MemoryStream f = new System.IO.MemoryStream();
					if (splitbyte < rlen)
					{
						f.Write(buf, splitbyte, rlen - splitbyte);
					}

					// While Firefox sends on the first read all the data fitting
					// our buffer, Chrome and Opera sends only the headers even if
					// there is data for the body. So we do some magic here to find
					// out whether we have already consumed part of body, if we
					// have reached the end of the data to be sent or we should
					// expect the first byte of the body at the next read.
					if (splitbyte < rlen)
					{
						size -= rlen - splitbyte +1;
					}
					else if (!sbfound || size == 0x7FFFFFFFFFFFFFFFL)
					{
						size = 0;
					}

					// Now read all the body and write it to f
					buf = new byte[512];
					while (rlen >= 0 && size > 0)
					{
                        rlen = mySocket.Receive(buf);
						//rlen = @is.Read(buf, 0, 512);
						size -= rlen;
						if (rlen > 0)
						{
							f.Write(buf, 0, rlen);
						}
					}

					// Get the raw body as a byte []
					byte[] fbuf = f.ToArray();
                    // Create a BufferedReader for easily reading it as string.
                    MemoryStream bin = new MemoryStream(fbuf);
                    StreamReader @in = new StreamReader(bin);

					// If the method is POST, there may be parameters
					// in data section, too, read it:
					if (method.Equals("POST", StringComparison.CurrentCultureIgnoreCase))
					{
						string contentType = "";
                        header.TryGetValue("content-type", out string contentTypeHeader);
                        string[] st = System.Text.RegularExpressions.Regex.Split(contentTypeHeader,";", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if(st.Length != 0)
                        {
                            contentType = st[0];
                        }
                        
						if (contentType.Equals("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
						{
							// Handle multipart/form-data
							if (st.Length<=1)
							{
								sendError(HTTP_BADREQUEST, "BAD REQUEST: Content type is multipart/form-data but boundary missing. Usage: GET /example/file.html");
							}
							string boundaryExp = st[1];
							string[] st_new = System.Text.RegularExpressions.Regex.Split(boundaryExp, "=", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
							if (st_new.Length != 2)
							{
								sendError(HTTP_BADREQUEST, "BAD REQUEST: Content type is multipart/form-data but boundary syntax error. Usage: GET /example/file.html");
							}
                            
							string boundary = st_new[1];

							decodeMultipartData(boundary, fbuf, @in, parms, files);
						}
						else
						{
							// Handle application/x-www-form-urlencoded
							string postLine = "";
							char[] pbuf = new char[512];
							int read = @in.Read(pbuf, 0, pbuf.Length);
							while (read >= 0 && !postLine.EndsWith("\r\n", StringComparison.Ordinal))
							{

                                char[] temp = new char[read];
                                Array.ConstrainedCopy(pbuf, 0, temp, 0, read);
                                string temp_str = new string(temp);
                                postLine += temp_str;
								read = @in.Read(pbuf, 0, pbuf.Length);
                                temp = null;
							}
							postLine = postLine.Trim();
							decodeParms(postLine, parms);
						}
					}

					// Ok, now do the serve()
					Console.WriteLine("----GPF-----");
					Response r = outerInstance.serve(uri, method, header, parms, files);
                    Console.WriteLine("response is :{0} ,{1} ,{2} ,{3}", r.status, r.mimeType, r.header, r.data);
                    if (r == null)
					{
						sendError(HTTP_INTERNALERROR, "SERVER INTERNAL ERROR: Serve() returned a null response.");
					}
					else
					{
						sendResponse(r.status, r.mimeType, r.header, r.data);
                        r.data.Close();
                    }
                    @in.Dispose();
                    bin.Dispose();
				}
                catch (SocketException e)
                {
                    Console.WriteLine("socket error:{0}", e.Message);
                    mySocket.Dispose();
                }
				catch (IOException ioe)
				{
                    Console.WriteLine("IOException :{0}", ioe.Message);
					try
					{
						sendError(HTTP_INTERNALERROR, "SERVER INTERNAL ERROR: IOException: " + ioe.Message);
					}
					catch (Exception)
					{
					}
                    mySocket.Dispose();
                }
				catch (ThreadInterruptedException e)
				{
                    mySocket.Dispose();
                    Console.WriteLine("ThreadInterruptedException :{0}", e.Message);
					// Thrown by sendError, ignore and exit the thread.
				}
			}

			/// <summary>
			/// Decodes the sent headers and loads the data into
			/// java Dictionary<string,string>' key - value pairs
			/// 
			/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void decodeHeader(java.io.BufferedReader in, java.util.Dictionary<string,string> pre, java.util.Dictionary<string,string> parms, java.util.Dictionary<string,string> header) throws InterruptedException
			internal virtual void decodeHeader(System.IO.StreamReader @in, Dictionary<string,string> pre, Dictionary<string, string> parms, Dictionary<string, string> header)
			{
				try
				{
					// Read the request line
					string inLine = @in.ReadLine();
                    if(inLine == null)
                    {
                        pre.Add("method", "throw");
                        return;
                    }
                    if ((inLine.IndexOf("{") == 0))
                    {
                        
                        mySocket.Close();
                        CloseAll = true;
                        return;
                        //Environment.Exit(0);
                    }
                    int index_method = inLine.IndexOf("/");
                    int lastindex_HTTP = inLine.LastIndexOf("HTTP");
                    string method = inLine.Substring(0, index_method - 1).Trim();
                    string uri = inLine.Substring(index_method, lastindex_HTTP - 4).Trim();
                    string version = inLine.Substring(lastindex_HTTP).Trim();
					if (string.ReferenceEquals(inLine, null))
					{
						return;
					}
                    if (method == null)
                    {
                        sendError(HTTP_BADREQUEST, "BAD REQUEST: Syntax error. Usage: GET /example/file.html");
                    }
                    pre.Add("method", method);
                    if (uri == null)
                    {
                        sendError(HTTP_BADREQUEST, "BAD REQUEST: Missing URI. Usage: GET /example/file.html");
                    }
                    // Decode parameters from the URI
                    int qmi = uri.IndexOf('?');
					Console.WriteLine("test by yi " + uri);
					if (qmi >= 0)
					{

						decodeParms(uri.Substring(qmi + 1), parms);
						Console.WriteLine("test by yi " + uri);
                        uri = decodePercent(uri.Substring(0, qmi));
						Console.WriteLine("test by yi " + uri);
					}
					else
					{
                        uri = decodePercent(uri);
					}
					Console.WriteLine("test by yi " + uri);
                    pre.Add("uri", uri);
                    // If there's another token, it's protocol version,
                    // followed by HTTP headers. Ignore version but parse headers.
                    // NOTE: this now forces header names lowercase since they are
                    // case insensitive and vary by client.

                    if (version != null)
                    {
                        string line = @in.ReadLine();
                        int i = 1;
                        while ((line == null)&(i<1000))
                        {


                            if (line == null)
                            {
                                Console.WriteLine("请求头空：{0}", i++);
                            }
                            else
                            {
                                Console.WriteLine("请求头为：{0}", line);
                            }
                            line = @in.ReadLine();
                        }
                        while (!string.ReferenceEquals(line, null) && line.Trim().Length > 0)
                        {
                            int p = line.IndexOf(':');
                            if (p >= 0)
                            {
                                header.Add(line.Substring(0, p).Trim().ToLower(), line.Substring(p + 1).Trim());
                            }
                            line = @in.ReadLine();
                        }
                    }
                }
				catch (IOException ioe)
				{
					sendError(HTTP_INTERNALERROR, "SERVER INTERNAL ERROR: IOException: " + ioe.Message);
                    
				}
			}

			/// <summary>
			/// Decodes the Multipart Body data and put it
			/// into java Dictionary<string,string>' key - value pairs.
			/// 
			/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void decodeMultipartData(String boundary, byte[] fbuf, java.io.BufferedReader in, java.util.Dictionary<string,string> parms, java.util.Dictionary<string,string> files) throws InterruptedException
			internal virtual void decodeMultipartData(string boundary, byte[] fbuf, System.IO.StreamReader @in, Dictionary<string,string> parms, Dictionary<string, string> files)
			{
				try
				{
					int[] bpositions = getBoundaryPositions(fbuf,Encoding.UTF8.GetBytes(boundary));
					int boundarycount = 1;
					string mpline = @in.ReadLine();
					while (!string.ReferenceEquals(mpline, null))
					{
						if (mpline.IndexOf(boundary, StringComparison.Ordinal) == -1)
						{
							sendError(HTTP_BADREQUEST, "BAD REQUEST: Content type is multipart/form-data but next chunk does not start with boundary. Usage: GET /example/file.html");
						}
						boundarycount++;
						Dictionary<string,string> item = new Dictionary<string,string>();
						mpline = @in.ReadLine();
						while (!string.ReferenceEquals(mpline, null) && mpline.Trim().Length > 0)
						{
							int p = mpline.IndexOf(':');
							if (p != -1)
							{
								item.Add(mpline.Substring(0,p).Trim().ToLower(), mpline.Substring(p + 1).Trim());
							}
							mpline = @in.ReadLine();
						}
						if (!string.ReferenceEquals(mpline, null))
						{
                            item.TryGetValue("content-disposition", out string contentDisposition);
                            if (string.ReferenceEquals(contentDisposition, null))
							{
								sendError(HTTP_BADREQUEST, "BAD REQUEST: Content type is multipart/form-data but no content-disposition info found. Usage: GET /example/file.html");
							}
                            string[] st = System.Text.RegularExpressions.Regex.Split(contentDisposition, ";", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
							Dictionary<string,string> disposition = new Dictionary<string,string>();
                            for(int i=0;i<st.Length;i++)
							{
                                string token = st[i];
								int p = token.IndexOf('=');
								if (p != -1)
								{
									disposition.Add(token.Substring(0,p).Trim().ToLower(), token.Substring(p + 1).Trim());
								}
							}
                            disposition.TryGetValue("name", out string pname);
                            pname = pname.Substring(1, (pname.Length - 1) - 1);

							string value = "";
                            item.TryGetValue("content-type", out string content_type);
							if (content_type == null)
							{
								while (!string.ReferenceEquals(mpline, null) && mpline.IndexOf(boundary, StringComparison.Ordinal) == -1)
								{
									mpline = @in.ReadLine();
									if (!string.ReferenceEquals(mpline, null))
									{
										int d = mpline.IndexOf(boundary, StringComparison.Ordinal);
										if (d == -1)
										{
											value += mpline;
										}
										else
										{
											value += mpline.Substring(0,d - 2);
										}
									}
								}
							}
							else
							{
								if (boundarycount > bpositions.Length)
								{
									sendError(HTTP_INTERNALERROR, "Error processing request");
								}
								int offset = stripMultipartHeaders(fbuf, bpositions[boundarycount - 2]);
								string path = saveTmpFile(fbuf, offset, bpositions[boundarycount - 1] - offset - 4);
								files.Add(pname, path);
                                disposition.TryGetValue("filename", out value);
								value = value.Substring(1, (value.Length - 1) - 1);
								do
								{
									mpline = @in.ReadLine();
								} while (!string.ReferenceEquals(mpline, null) && mpline.IndexOf(boundary, StringComparison.Ordinal) == -1);
							}
							parms.Add(pname, value);
						}
					}
				}
				catch (IOException ioe)
				{
					sendError(HTTP_INTERNALERROR, "SERVER INTERNAL ERROR: IOException: " + ioe.Message);
				}
			}

			/// <summary>
			/// Find the byte positions where multipart boundaries start.
			/// 
			/// </summary>
			public virtual int[] getBoundaryPositions(byte[] b, byte[] boundary)
			{
				int matchcount = 0;
				int matchbyte = -1;
				ArrayList matchbytes = new ArrayList();
				for (int i = 0; i < b.Length; i++)
				{
					if (b[i] == boundary[matchcount])
					{
						if (matchcount == 0)
						{
							matchbyte = i;
						}
						matchcount++;
						if (matchcount == boundary.Length)
						{
							matchbytes.Add(new int?(matchbyte));
							matchcount = 0;
							matchbyte = -1;
						}
					}
					else
					{
						i -= matchcount;
						matchcount = 0;
						matchbyte = -1;
					}
				}
				int[] ret = new int[matchbytes.Count];
				for (int i = 0; i < ret.Length; i++)
				{
					ret[i] = ((int?)matchbytes[i]).Value;
				}
				return ret;
			}

			/// <summary>
			/// Retrieves the content of a sent file and saves it
			/// to a temporary file.
			/// The full path to the saved file is returned.
			/// 
			/// </summary>
			internal virtual string saveTmpFile(byte[] b, int offset, int len)
			{
				string path = "";
				if (len > 0)
				{
                    string tmpdir = Path.GetTempPath();
                    try
					{
                        string temp = Path.GetTempFileName();
                        FileStream fstream = new FileStream(temp, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
						fstream.Write(b, offset, len);
						fstream.Close();
                        path = temp;
					}
					catch (Exception e)
					{ // Catch exception if any
						Console.Error.WriteLine("Error: " + e.Message);
					}
				}
				return path;
			}


			/// <summary>
			/// It returns the offset separating multipart file headers
			/// from the file's data.
			/// 
			/// </summary>
			internal virtual int stripMultipartHeaders(byte[] b, int offset)
			{
				int i = 0;
				for (i = offset; i < b.Length; i++)
				{
					if (b[i] == (byte)'\r' && b[++i] == (byte)'\n' && b[++i] == (byte)'\r' && b[++i] == (byte)'\n')
					{
						break;
					}
				}
				return i + 1;
			}

			/// <summary>
			/// Decodes the percent encoding scheme. <br/>
			/// For example: "an+example%20string" -> "an example string" </summary>
			/// <exception cref="UnsupportedEncodingException">  </exception>
			internal virtual string decodePercent(string str)
			{
				/* 此处进行自己的修�? by yi,因为中文完全不能解码
				try
				{
					StringBuffer sb = new StringBuffer();
					for( int i=0; i<str.length(); i++ )
					{
						char c = str.charAt( i );
						switch ( c )
						{
							case '+':
								sb.append( ' ' );
								break;
							case '%':
								sb.append((char)Integer.parseInt( str.substring(i+1,i+3), 16 ));
								i += 2;
								break;
							default:
								sb.append( c );
								break;
						}
					}
					return sb.toString();
				}
				catch( Exception e )
				{
					sendError( HTTP_BADREQUEST, "BAD REQUEST: Bad percent-encoding." );
					return null;
				}*/
				try
				{
					return HttpUtility.UrlDecode(str,Encoding.GetEncoding("UTF-8"));
				}
				catch (UnsupportedEncodingException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				return str;
			}

			/// <summary>
			/// Decodes parameters in percent-encoded URI-format
			/// ( e.g. "name=Jack%20Daniels&pass=Single%20Malt" ) and
			/// adds them to given Dictionary<string,string>. NOTE: this doesn't support multiple
			/// identical keys due to the simplicity of Dictionary<string,string> -- if you need multiples,
			/// you might want to replace the Dictionary<string,string> with a Hashtable of Vectors or such.
			/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void decodeParms(String parms, java.util.Dictionary<string,string> p) throws InterruptedException
			internal virtual void decodeParms(string parms, Dictionary<string,string> p)
			{
				if (string.ReferenceEquals(parms, null))
				{
					return;
				}
                string[] st = System.Text.RegularExpressions.Regex.Split(parms, "&", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                for (int i= 0;i < st.Length; i++)
                {
                    string e = st[i];
                    int sep = e.IndexOf('=');
                    if (sep >= 0)
                    {
                        p.Add(decodePercent(e.Substring(0, sep)).Trim(), decodePercent(e.Substring(sep + 1)));
                    }
                }        
			}

			/// <summary>
			/// Returns an error message as a HTTP response and
			/// throws InterruptedException to stop further request processing.
			/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendError(String status, String msg) throws InterruptedException
			internal virtual void sendError(string status, string msg)
			{
				sendResponse(status, MIME_PLAINTEXT, null, new System.IO.MemoryStream(Encoding.UTF8.GetBytes(msg)));
				throw new ThreadInterruptedException();
			}

			/// <summary>
			/// Sends given response to the socket.
			/// </summary>
			internal virtual void sendResponse(string status, string mime, Dictionary<string,string> header, Stream data)
			{
				try
				{
					if (string.ReferenceEquals(status, null))
					{
						throw new Exception("sendResponse(): Status can't be null.");
					}
                    NetworkStream pw = new NetworkStream(mySocket);

                    string mes = "HTTP/1.0 " + status + " \r\n";
                    byte[] mess = System.Text.Encoding.UTF8.GetBytes(mes);
                    pw.Write(mess, 0, mess.Length);

					if (!string.ReferenceEquals(mime, null))
					{
                        mes = "Content-Type: " + mime + "\r\n";
                        mess = System.Text.Encoding.UTF8.GetBytes(mes);
                        pw.Write(mess, 0, mess.Length);
                    }
                    header.TryGetValue("Data", out string header_data);
                    if (header == null || header_data == null)
					{
                        mes = "Date: " + DateTime.Now.ToString("r") + "\r\n";
                        mess = System.Text.Encoding.UTF8.GetBytes(mes);
                        pw.Write(mess, 0, mess.Length);
                    }

                    if (header.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> head in header)
                        {
                            Console.WriteLine(head.Key + " " + head.Value);
                            mes = head.Key + ": " + head.Value + "\r\n";
                            mess = System.Text.Encoding.UTF8.GetBytes(mes);
                            pw.Write(mess, 0, mess.Length);
                        }
                    }
                    
                    mes = "\r\n";
                    mess = System.Text.Encoding.UTF8.GetBytes(mes);
                    pw.Write(mess, 0, mess.Length);
                    pw.Flush();

                    if (data != null)
					{
						long pending = 0; // This is to support partial sends, see serveFile()

						if (header.ContainsKey("Content-Length"))
						{
                            header.TryGetValue("Content-Length", out string content_length);
                            pending = Convert.ToInt64(content_length);
						}

						Console.WriteLine(pending);
						byte[] buff = new byte[2048];
						while (pending > 0)
						{
							int read = data.Read(buff, 0, ((pending > 2048) ? 2048 : (int)pending));
                            pw.Write(buff,0,buff.Length);
                            pending -= read;
                            pw.Flush();

						}
					}
                    pw.Dispose();
                    if (data != null)
					{
						data.Close();
					}
				}
				catch (IOException)
				{
					// Couldn't write? No can do.
					Console.WriteLine("No can do");
					try
					{
						mySocket.Close();
					}
					catch (Exception)
					{
					}
				}
			}

			internal Socket mySocket;
		}

		/// <summary>
		/// URL-encodes everything between "/"-characters.
		/// Encodes spaces as '%20' instead of '+'.
		/// </summary>
		private string encodeUri(string uri)
		{
			string newUri = "";

            newUri = Uri.EscapeUriString(uri);//此处可能出现编码问题
			return newUri;
		}

		private int myTcpPort;
		private Socket myServerSocket;
		private Thread myThread;
        private string myRootDir;

		// ==================================================
		// File server code
		// ==================================================

		/// <summary>
		/// Serves file from homeDir and its' subdirectories (only).
		/// Uses only URI, ignores all headers and HTTP parameters.
		/// </summary>
		public virtual Response ServeFile(string uri, Dictionary<string,string> header, string homeDir, bool allowDirectoryListing)
		{
			Response res = null;
            string homeDir_str = new FileInfo(homeDir).FullName;
            Console.WriteLine("----GPF---uri=" + uri + ";homeDir=" + homeDir_str);
			// Make sure we won't die of an exception later
			if (!Directory.Exists(homeDir_str))
			{
				res = new Response(this, HTTP_INTERNALERROR, MIME_PLAINTEXT, "INTERNAL ERRROR: serveFile(): given homeDir is not a directory.");
			}

			if (res == null)
			{
				// Remove URL arguments
				uri = uri.Trim().Replace(Path.DirectorySeparatorChar, '/');
				if (uri.IndexOf('?') >= 0)
				{
					uri = uri.Substring(0, uri.IndexOf('?'));
				}

				// Prohibit getting out of current directory
				if (uri.StartsWith("..", StringComparison.Ordinal) || uri.EndsWith("..", StringComparison.Ordinal) || uri.IndexOf("../", StringComparison.Ordinal) >= 0)
				{
					res = new Response(this, HTTP_FORBIDDEN, MIME_PLAINTEXT, "FORBIDDEN: Won't serve ../ for security reasons.");
				}
			}

			if (res == null && !File.Exists(uri))
			{
				res = new Response(this, HTTP_NOTFOUND, MIME_PLAINTEXT, "Error 404, file not found.");
				Console.WriteLine("---GPF--Error 404 file exist:" + File.Exists(uri));
			}


			// List the directory, if necessary
			if (res == null && Directory.Exists(uri))
			{
				// Browsers get confused without '/' after the
				// directory, send a redirect.
				if (!uri.EndsWith("/", StringComparison.Ordinal))
				{
					uri += "/";
					res = new Response(this, HTTP_REDIRECT, MIME_HTML, "<html><body>Redirected: <a href=\"" + uri + "\">" + uri + "</a></body></html>");
					res.addHeader("Location", uri);
				}

				if (res == null)
				{
					// First try index.html and index.htm
                    if(File.Exists(uri + "/index.html"))
					//if ((new File(f, "index.html")).exists())
					{
                        //f = new File(homeDir, uri + "/index.html");
                        uri = uri + "/index.html";
                    }
					else if (File.Exists(uri + "/index.htm"))
					{
                        //f = new File(homeDir, uri + "/index.htm");
                        uri = uri + "/index.htm";
                    }
					// No index file, list the directory if it is readable
					else if (allowDirectoryListing && IsFileInUse(uri))
					{
                        string[] files = Directory.GetFiles(uri);
						//string[] files = f.list();
						string msg = "<html><body><h1>Directory " + uri + "</h1><br/>";

						if (uri.Length > 1)
						{
							string u = uri.Substring(0, uri.Length - 1);
							int slash = u.LastIndexOf('/');
							if (slash >= 0 && slash < u.Length)
							{
								msg += "<b><a href=\"" + uri.Substring(0, slash + 1) + "\">..</a></b><br/>";
							}
						}

						if (files != null)
						{
							for (int i = 0; i < files.Length; ++i)
							{
                                bool dir = Directory.Exists(files[i]);
								if (dir)
								{
									msg += "<b>";
									files[i] += "/";
								}

								msg += "<a href=\"" + encodeUri(uri + files[i]) + "\">" + files[i] + "</a>";

								// Show file size
								if (File.Exists(files[i]))
								{
                                    FileInfo fileInfo = new FileInfo(files[i]);
                                    long len = fileInfo.Length;
                                    //long len = curFile.length();
									msg += " &nbsp;<font size=2>(";
									if (len < 1024)
									{
										msg += len + " bytes";
									}
									else if (len < 1024 * 1024)
									{
										msg += len / 1024 + "." + (len % 1024 / 10 % 100) + " KB";
									}
									else
									{
										msg += len / (1024 * 1024) + "." + len % (1024 * 1024) / 10 % 100 + " MB";
									}

									msg += ")</font>";
								}

								msg += "<br/>";
								if (dir)
								{
									msg += "</b>";
								}
							}
						}
						msg += "</body></html>";
						res = new Response(this, HTTP_OK, MIME_HTML, msg);
					}
					else
					{
						res = new Response(this, HTTP_FORBIDDEN, MIME_PLAINTEXT, "FORBIDDEN: No directory listing.");
					}
				}
			}

			try
			{
				if (res == null)
				{
					// Get MIME type from file name extension, if possible
					string mime = null;
                    int dot = uri.LastIndexOf(".");
					//int dot = f.CanonicalPath.LastIndexOf('.');
					if (dot >= 0)
					{
                        mime = (string)theMimeTypes[uri.Substring(dot + 1).ToLower()];
                        //mime = (string)theMimeTypes[f.CanonicalPath.substring(dot + 1).ToLower()];
					}
					if (string.ReferenceEquals(mime, null))
					{
						mime = MIME_DEFAULT_BINARY;
					}

					long startFrom = 0;
					long endAt = -1;
                    header.TryGetValue("range", out string range);
                    if (!string.ReferenceEquals(range, null))
					{
						if (range.StartsWith("bytes=", StringComparison.Ordinal))
						{
							range = range.Substring("bytes=".Length);
							int minus = range.IndexOf('-');
							try
							{
								if (minus > 0)
								{
									startFrom = long.Parse(range.Substring(0, minus));
									endAt = long.Parse(range.Substring(minus + 1));
								}
							}
							catch (System.FormatException)
							{
							}
						}
					}

					// Change return code and add Content-Range header when skipping is requested
                    FileInfo fileInfo = new FileInfo(uri);
                    long fileLen = fileInfo.Length;
                    FileStream fis = new FileStream(uri, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);


                    if (!string.ReferenceEquals(range, null) && startFrom >= 0)
                    {
                        if (startFrom >= fileLen)
                        {
                            res = new Response(this, HTTP_RANGE_NOT_SATISFIABLE, MIME_PLAINTEXT, "");
                            res.addHeader("Content-Range", "bytes 0-0/" + fileLen);
                        }
                        else
                        {
                            if (endAt < 0)
                            {
                                endAt = fileLen - 1;
                            }
                            long newLen = endAt - startFrom + 1;
                            if (newLen < 0)
                            {
                                newLen = 0;
                            }

                            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                            //ORIGINAL LINE: final long dataLen = newLen;
                            long dataLen = newLen;

                            fis.Seek(startFrom, SeekOrigin.Begin);

                            res = new Response(this, HTTP_PARTIALCONTENT, mime, fis);
                            res.addHeader("Content-Length", "" + dataLen);
                            res.addHeader("Content-Range", "bytes " + startFrom + "-" + endAt + "/" + fileLen);
                        }
                    }
                    else
                    {
                        res = new Response(this, HTTP_OK, mime, fis);
                        res.addHeader("Content-Length", "" + fileLen);
                    }
                }
			}
			catch (IOException)
			{
				res = new Response(this, HTTP_FORBIDDEN, MIME_PLAINTEXT, "FORBIDDEN: Reading file failed.");
			}

			res.addHeader("Accept-Ranges", "bytes"); // Announce that the file server accepts partial content requestes
			return res;
		}

        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;

            FileStream fs = null;
            try
            {

                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,

                FileShare.None);

                inUse = false;
            }
            catch
            {

            }
            finally
            {
                if (fs != null)

                    fs.Close();
            }
            return inUse;//true表示正在使用,false没有使用  
        }


        /// <summary>
        /// Hashtable mapping (String)FILENAME_EXTENSION -> (String)MIME_TYPE
        /// </summary>
        private static Hashtable theMimeTypes = new Hashtable();
		static HttpServer()
		{
            string str =("css text/css " + 
                "js text/javascript " + 
                "htm text/html " + 
                "html text/html " + 
                "txt text/plain " + 
                "asc text/plain " + 
                "gif image/gif " + 
                "jpg image/jpeg " + 
                "jpeg image/jpeg " + 
                "png image/png " + 
                "mp3 audio/mpeg " + 
                "m3u audio/mpeg-url " + 
                "pdf application/pdf " + 
                "doc application/msword " + 
                "ogg application/x-ogg " + 
                "zip application/octet-stream " + 
                "exe application/octet-stream " + 
                "class application/octet-stream ");
            string[] st = str.Split();

            for(int i=0;i < st.Length/2; i++)
            {
                theMimeTypes.Add(st[2*i],st[2*i+1]);
            }
		}

        /// <summary>
        /// GMT date formatter
        /// </summary>
        //private static java.text.SimpleDateFormat gmtFrmt;
        //DateTime gmtFrmt = new DateTime();
        /// <summary>
        /// The distribution licence
        /// </summary>
        private static readonly string LICENCE = "Copyright (C) 2001,2005-2011 by Jarno Elonen <elonen@iki.fi>\n"+
			"and Copyright (C) 2010 by Konstantinos Togias <info@ktogias.gr>\n"+
			"\n"+
			"Redistribution and use in source and binary forms, with or without\n"+
			"modification, are permitted provided that the following conditions\n"+
			"are met:\n"+
			"\n"+
			"Redistributions of source code must retain the above copyright notice,\n"+
			"this list of conditions and the following disclaimer. Redistributions in\n"+
			"binary form must reproduce the above copyright notice, this list of\n"+
			"conditions and the following disclaimer in the documentation and/or other\n"+
			"materials provided with the distribution. The name of the author may not\n"+
			"be used to endorse or promote products derived from this software without\n"+
			"specific prior written permission. \n"+
			" \n"+
			"THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR\n"+
			"IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES\n"+
			"OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.\n"+
			"IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,\n"+
			"INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT\n"+
			"NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,\n"+
			"DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY\n"+
			"THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT\n"+
			"(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE\n"+
			"OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.";

        
    }


}