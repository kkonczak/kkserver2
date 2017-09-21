using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace KKServer2
{
    class Server
    {
        //zwraca, jak serwer ma odpowiedzieć - czy ciągle, zakresami, czy nie znając długości
        public enum TypeOfResponse
        {
            Normal,
            Range
        }
        public enum MethodType
        {
            GetPost,
            Head,
            Options
        }
        public struct Mimetype
        {
            public string EndsWith;
            public string Type;
        }
        public struct CgiItemInfo
        {
            public string CGIpath;
            public string EndsWith;
        }
        public delegate void DelegateModulesConnectionManage(System.Net.Sockets.Socket socket);
        public delegate void DelegateModulesRequestManage(System.Net.Sockets.Socket socket, ref string request);
        public delegate void DelegateModulesDirectoryDisplay(Socket socket, string address);



        private System.Net.Sockets.TcpListener listener;
        public int HeaderTimeout = 3000;
        public int BodyOfRequestTimeout = 10000;
        public string RootAddress = "";
        public string ErrorsPagesAddress = "";
        public bool ListDirectories = false;
        public List<string> NameOfRootDocuments = new List<string>();
        public List<Mimetype> Mimetypes = new List<Mimetype>();
        public Version Version;
        public bool IsCGIhandle = false;
        public List<CgiItemInfo> CgiList = new List<CgiItemInfo>();
        public bool CreateLogs = false;
        public string LogsPath = "";
        public int MaxNumberOfCGI = 2;
        public bool BrowseDirectories = false;

        //uchwyty dla dodatków
        public DelegateModulesConnectionManage ModulesConnectionManage=null;
        public DelegateModulesRequestManage ModulesRequestManage=null;
        public DelegateModulesDirectoryDisplay ModulesDirectoryDisplay = DirectoryShow;
        

        private static string serverName = "KKServer";
        private Dictionary<int, string> errorDescriptions = new Dictionary<int, string>();
        private int numOfCGIProcesses = 0;

        private System.IO.StreamWriter logFile;


        public Server(System.Net.IPAddress ipaddress, int port)
        {
            listener = new System.Net.Sockets.TcpListener(new System.Net.IPEndPoint(ipaddress, port));
            errorDescriptions.Add(100, "Continue");
            errorDescriptions.Add(101, "Switching Protocols");
            errorDescriptions.Add(110, "Connection Timed Out");
            errorDescriptions.Add(111, "Connection refused");
            errorDescriptions.Add(200, "OK");
            errorDescriptions.Add(206, "Partial Content");
            errorDescriptions.Add(300, "Multiple Choices");
            errorDescriptions.Add(301, "Moved Permanently");
            errorDescriptions.Add(302, "Found");
            errorDescriptions.Add(303, "See Other");
            errorDescriptions.Add(304, "Not Modified");
            errorDescriptions.Add(305, "Use Proxy");
            errorDescriptions.Add(306, "Switch Proxy");
            errorDescriptions.Add(307, "Temporary Redirect");
            errorDescriptions.Add(310, "Too Many Redirects");
            errorDescriptions.Add(400, "Bad Request");
            errorDescriptions.Add(401, "Unauthorized");
            errorDescriptions.Add(402, "Payment Required");
            errorDescriptions.Add(403, "Forbidden");
            errorDescriptions.Add(404, "Not Found");
            errorDescriptions.Add(405, "Method Not Allowed");
            errorDescriptions.Add(406, "Not Acceptable");
            errorDescriptions.Add(407, "Proxy Authentication Required");
            errorDescriptions.Add(408, "Request Timeout");
            errorDescriptions.Add(409, "Conflict");
            errorDescriptions.Add(410, "Gone");
            errorDescriptions.Add(411, "Length Required");
            errorDescriptions.Add(412, "Precondition Failed");
            errorDescriptions.Add(413, "Request Entity Too Large");
            errorDescriptions.Add(414, "Request-URI Too Long");
            errorDescriptions.Add(415, "Unsupported Media Type");
            errorDescriptions.Add(416, "Requested Range Not Satisfiable");
            errorDescriptions.Add(417, "Expectation Failed");
            errorDescriptions.Add(418, "I'm a teapot");
            errorDescriptions.Add(451, "Unavailable For Legal Reasons");
            errorDescriptions.Add(500, "Internal Server Error");
            errorDescriptions.Add(501, "Not Implemented");
            errorDescriptions.Add(502, "Bad Gateway");
            errorDescriptions.Add(503, "Service Unavailable");
            errorDescriptions.Add(504, "Gateway Timeout");
            errorDescriptions.Add(505, "HTTP Version Not Supported");
            errorDescriptions.Add(506, "Variant Also Negotiates");
            errorDescriptions.Add(509, "Bandwidth Limit Exceeded");
            errorDescriptions.Add(510, "Not Extended");
            errorDescriptions.Add(511, "Network Authentication Required");

        }
        public void Start()
        {
            if (CreateLogs)
            {
                logFile = new System.IO.StreamWriter(LogsPath, true);
                logFile.AutoFlush = true;
            }
            if (CreateLogs) { logFile.WriteLine("S\t" + DateTime.Now.ToString()); logFile.Flush(); }
            listener.Start( int.MaxValue );
            if (RootAddress == null || RootAddress == "")
            {
                throw new ArgumentNullException("rootAddress", "Null value");
            }

            if (!(new System.IO.DirectoryInfo(RootAddress)).Exists)
            {
                throw new ArgumentNullException("rootAddress", "Invalid value");
            }

            listener.BeginAcceptSocket(ConnectionHandler, listener);
        }

        public void Stop()
        {
            listener.Stop();
        }

        private void ConnectionHandler(IAsyncResult result)
        {
            //  System.Windows.Forms.MessageBox.Show("OK");
            System.Net.Sockets.Socket socket;
            try
            {
 socket = ((System.Net.Sockets.TcpListener)(result.AsyncState)).EndAcceptSocket(result);
           
            }
            catch { socket = null; }
            //tworzenie nowego gniazda
            listener.BeginAcceptSocket(ConnectionHandler, listener);
            try
            {
                ModulesConnectionManage?.Invoke(socket); //jeśli ustawiony dodatek przechwytujcy gniazdo

                //obsługiwanie kodu;
                int _bufferSize = 65000;
                int _bufPointer = 0;
                byte[] buffer = new byte[_bufferSize];
                DateTime gettingTime = DateTime.Now;
                bool isPacketCompleted = false;
                while (true)
                {
                    int _tmp = Math.Min(socket.Available, buffer.Length);
                    if (_tmp != 0)
                    {
                        socket.Receive(buffer, _bufPointer, _bufferSize, SocketFlags.None);
                        _bufPointer += _tmp;
                        if (_bufPointer > 3)
                        {
                            for (int i = 4; i <= _bufPointer; i++)
                            {
                                if (buffer[i - 1] == '\n' && buffer[i - 2] == '\r' && buffer[i - 3] == '\n' && buffer[i - 4] == '\r')
                                {
                                    isPacketCompleted = true;
                                    goto packetEnd;

                                }
                            }

                        }
                    }


                    if ((DateTime.Now - gettingTime).TotalMilliseconds > HeaderTimeout)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(2);

                }
                packetEnd:
                if (isPacketCompleted)
                {
                    string header = System.Text.ASCIIEncoding.ASCII.GetString(buffer, 0, _bufPointer);
                    ModulesRequestManage?.Invoke(socket, ref header);
                    string[] @params = header.Split(new char[] { '\r' })[0].Split(new char[] { ' ' }, 4);


                    if (CreateLogs)
                    {
                        try
                        {
                            logFile.WriteLine("GetFile\t" + DateTime.Now.ToString() + "\t" + socket.RemoteEndPoint.ToString() + "\t" + socket.LocalEndPoint.ToString() + "\t" + @params[0] + "\t" + @params[1]);
                        }
                        catch { }
                    }

                    if (@params[2].ToUpper() == "HTTP/1.1")
                    {
                        if (@params[0].ToUpper() == "GET")
                        {
                            MethodGET(socket, header,MethodType.GetPost);
                        }
                        else if (@params[0].ToUpper() == "OPTIONS")
                        {
                            MethodGET(socket, header, MethodType.Options);
                        }
                        else if (@params[0].ToUpper() == "HEAD")
                        {
                            MethodGET(socket, header, MethodType.Head);
                        }
                        else if (@params[0].ToUpper() == "POST")
                        {
                            MethodPOST(socket, header, ref buffer, _bufPointer);
                        }
                        else
                        {
                            ErrorWrite(501, socket);
                            //TODO: unsupported method
                        }
                        //tutaj możliwe pobieranie treści
                        //System.Windows.Forms.MessageBox.Show(header);
                    }
                    else
                    {
                        //version of protocol not supported
                        ErrorWrite(505, socket);
                    }

                }
                else
                {
                    //header is not completed
                    ErrorWrite(408, socket);
                   // Console.WriteLine((DateTime.Now - gettingTime).TotalMilliseconds);
                }

            }
            catch 
            {

            }

        }
        private bool FileExists(ref string address, ref string[] methodparam, Socket socket)
        {
            address = PrepareAddress(methodparam[1].Split('?')[0].Split(new char[] { '/' }), methodparam);
            bool czyznaleziono = false;
            if ((new System.IO.FileInfo(RootAddress + address)).Exists)
            {
                czyznaleziono = true;
            }
            else
            {
                foreach (var item in NameOfRootDocuments)
                {
                    if ((new System.IO.FileInfo(RootAddress + address + item)).Exists)
                    {
                        czyznaleziono = true;
                        address += item;
                        break;
                    }
                }
                if (!czyznaleziono)
                {
                    if(BrowseDirectories && (new System.IO.DirectoryInfo(RootAddress + address)).Exists)
                    {
                        ModulesDirectoryDisplay?.Invoke(socket, RootAddress + address);
                        socket.Close();
                    }
                }
                if (!czyznaleziono)
                {
                    ErrorWrite(404, socket);
                    return false;
                }

            }
            return czyznaleziono;
        }

        private bool IsParsedByCGI(string address, ref int parseCGIi)
        {
            for (int i = 0; i < CgiList.Count; i++)
            {
                if (address.EndsWith(CgiList[i].EndsWith))
                {
                    parseCGIi = i;
                    return true;
                }
            }
            return false;
        }
        private void MethodPOST(System.Net.Sockets.Socket socket, string header, ref byte[] bufferWithStartMessage, int bufferStartSize)
        {
            string[] headers = header.Split(new char[] { '\n' });
            string[] methodparam = headers[0].Split(new char[] { ' ' });
            //parsowanie adresu
            string address = "";
            try
            {
                bool czyznaleziono = FileExists(ref address, ref methodparam, socket);
                if (czyznaleziono)
                {
                    //plik znaleziony. adres : rootAddress+address

                    //czy plik parsowany przez CGI?
                    int parseCGIi = 0;
                    bool parseCGI = IsParsedByCGI(address, ref parseCGIi);

                    if (!parseCGI)
                    {
                        //czy normalnie czy przedziałami?
                        TypeOfResponse type = TypeOfResponse.Normal;
                        string rangeHeader = "";
                        for (int i = 1; i < headers.Length; i++)
                        {
                            if (headers[i].StartsWith("Range: "))
                            {
                                type = TypeOfResponse.Range;
                                rangeHeader = headers[i];
                            }
                        }
                        if (type == TypeOfResponse.Normal)
                        {
                            ResponseNormal(socket, RootAddress + address,MethodType.GetPost );
                        }
                        else
                        {
                            ResponseRanges(socket, RootAddress + address, rangeHeader);
                        }
                    }
                    else
                    {
                        //trzeba parsowac cgi...
                        if (numOfCGIProcesses <= MaxNumberOfCGI)
                        {
numOfCGIProcesses++;
                       // Console.WriteLine(numOfCGIProcesses);
                        try
                        {
                            ResponseCGI(socket, RootAddress + address, header, CgiList[parseCGIi], ref bufferWithStartMessage, bufferStartSize);
                        }
                        catch { }
                        numOfCGIProcesses--;
                        }
                        else
                        {
                            ErrorWrite(503, socket);
                        } 
                    }
                }

                //System.Windows.Forms.MessageBox.Show(rootAddress+ address);
            }
            catch (ArgumentException)
            {
                //nieprawidłowa ścieżka
                ErrorWrite(404, socket);
            }
            catch (Exception)
            {
                //błąd połączenia
            }
        }

        private void MethodGET(System.Net.Sockets.Socket socket, string header, MethodType methodType)
        {
            string[] headers = header.Split(new char[] { '\n' });
            string[] methodparam = headers[0].Split(new char[] { ' ' });
            //parsowanie adresu
            string address = "";
            try
            {
                bool czyznaleziono = FileExists(ref address, ref methodparam, socket);
                if (czyznaleziono)
                {
                    //plik znaleziony. adres : rootAddress+address

                    //czy plik parsowany przez CGI?
                    int parseCGIi = 0;
                    bool parseCGI = IsParsedByCGI(address, ref parseCGIi);

                    if (!parseCGI)
                    {
                        //czy normalnie czy przedziałami?
                        TypeOfResponse type = TypeOfResponse.Normal;
                        string rangeHeader = "";
                        for (int i = 1; i < headers.Length; i++)
                        {
                            if (headers[i].StartsWith("Range: "))
                            {
                                type = TypeOfResponse.Range;
                                rangeHeader = headers[i];
                            }
                        }
                        if (type == TypeOfResponse.Normal)
                        {
                            ResponseNormal(socket, RootAddress + address,methodType );
                        }
                        else
                        {
                            ResponseRanges(socket, RootAddress + address, rangeHeader);
                        }
                    }
                    else
                    {
                        //trzeba parsowac cgi...
                        byte[] tmp = new byte[0];
                        if (numOfCGIProcesses <= MaxNumberOfCGI)
                        {
                            numOfCGIProcesses++;
                           // Console.WriteLine(numOfCGIProcesses);
                            try
                            {
                                ResponseCGI(socket, RootAddress + address, header, CgiList[parseCGIi], ref tmp);
                            }
                            catch { }
                            numOfCGIProcesses--;
                        }else
                        {
                            ErrorWrite(503, socket);
                        }

                    }

                }

                //System.Windows.Forms.MessageBox.Show(rootAddress+ address);
            }
            catch (ArgumentException)
            {
                //nieprawidłowa ścieżka
                ErrorWrite(404, socket);
            }
            catch (Exception)
            {
                //błąd połączenia
            }
        }

        private string PrepareAddress(string[] adrParts, string[] methodparam)
        {
            string address = "";
            for (int i = 0; i < adrParts.Length; i++)
            {
                if (adrParts[i] == "")
                {
                    //nic
                }
                else if (adrParts[i] == "..")
                {
                    //katalog w górę
                    int offset = address.LastIndexOf('/');
                    if (offset == -1)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        address = address.Substring(0, offset);
                    }
                }
                else
                {
                    address += "/" + adrParts[i];
                }
            }
            if (methodparam[1].Split('?')[0].EndsWith("/"))
            {
                address += "/";
            }
            return address;
        }

        // przez CGI
        private void ResponseCGI(Socket socket, string address, string header, CgiItemInfo CGIinfo, ref byte[] sentData, int sentDataSize = 0)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = CGIinfo.CGIpath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.EnvironmentVariables.Add("REMOTE_ADDR", socket.RemoteEndPoint.ToString());
            proc.StartInfo.EnvironmentVariables.Add("SCRIPT_FILENAME", address);
            uint sentDataLength = 0;
            string[] par = header.Split('\n');
            string[] requestpar = header.Split(new char[] { ' ' }, 4);
            for (int i = 1; i < par.Length - 1; i++)
            {
                string[] h = par[i].Trim().Split(new char[] { ':' }, 2);
                if (h.Length == 1) { break; }
                if (h[0] == "User-Agent")
                {
                    proc.StartInfo.EnvironmentVariables.Add("USER_AGENT", h[1].Trim());
                }
                else if (h[0] == "Referer")
                {
                    proc.StartInfo.EnvironmentVariables.Add("REFERER", h[1].Trim());
                }
                else if (h[0] == "Content-Length")
                {
                    proc.StartInfo.EnvironmentVariables.Add("CONTENT_LENGTH", h[1].Trim());
                    sentDataLength = uint.Parse(h[1].Trim());
                }
                else if (h[0] == "Content-Type")
                {
                    proc.StartInfo.EnvironmentVariables.Add("CONTENT_TYPE", h[1].Trim());
                }
                else if (h[0] == "Cookie")
                {
                    proc.StartInfo.EnvironmentVariables.Add("HTTP_COOKIE", h[1].Trim());
                }

            }
            proc.StartInfo.EnvironmentVariables.Add("REQUEST_METHOD", requestpar[0]);
            //
            proc.StartInfo.EnvironmentVariables.Add("SERVER_PROTOCOL", "HTTP/1.1");
            if (requestpar[1].IndexOf('?') >= 0)
            {
                proc.StartInfo.EnvironmentVariables.Add("QUERY_STRING", requestpar[1].Substring(requestpar[1].IndexOf('?') + 1));
            }
            else
            {
                proc.StartInfo.EnvironmentVariables.Add("QUERY_STRING", "");
            }
            proc.StartInfo.EnvironmentVariables.Add("SERVER_SOFTWARE", serverName + "/" + Version.ToString());
            proc.StartInfo.EnvironmentVariables.Add("GATEWAY_INTERFACE", "CGI/1.1");
            proc.StartInfo.EnvironmentVariables.Add("REDIRECT_STATUS", "1");


            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            System.IO.StreamReader reader = proc.StandardOutput;
            System.IO.StreamWriter writer = proc.StandardInput;
            //wysyłanie danych z post (jeśli są)
            if (sentData.Length > 3)
            {
                //szukanie treści
                int endPos = 3;
                for (; endPos <= sentDataSize; endPos++)
                {
                    if (sentData[endPos - 1] == '\n' && sentData[endPos - 2] == '\r' && sentData[endPos - 3] == '\n' && sentData[endPos - 4] == '\r')
                    {
                        break;
                    }
                }
                writer.BaseStream.Write(sentData, endPos, sentDataSize - endPos);
                writer.BaseStream.Flush();

                //reszta danych z post
                uint howManyBytesGet = (uint)sentDataSize - (uint)endPos;
                if (sentDataLength > howManyBytesGet)
                {
                    byte[] buffer2 = new byte[60000];
                    int buffersize = 0;
                    DateTime gettingTime = DateTime.Now;
                    while (sentDataLength > howManyBytesGet)
                    {
                        if (socket.Available > 0)
                        {
                            buffersize = Math.Min(socket.Available, buffer2.Length);
                            socket.Receive(buffer2, buffersize, SocketFlags.None);
                            howManyBytesGet += (uint)buffersize;
                            writer.BaseStream.Write(buffer2, 0, buffersize);
                        }
                        if ((DateTime.Now - gettingTime).TotalMilliseconds > BodyOfRequestTimeout)
                        {
                            proc.Kill();
                            ErrorWrite(408, socket);
                            return;
                        }
                    }
                    writer.BaseStream.Flush();
                }
            }


            writer.BaseStream.Close();

            //pobieranie odpowiedzi cgi
            //bufor naglowka żądania
            byte[] bufferreq = new byte[60000];
            int bufferpos = 0;
            bool isHeaderMake = false;
            bool isHeaderSent = false;
            while (true)
            {
                while (true && !isHeaderMake)
                {
                    bufferreq[bufferpos] = (byte)reader.BaseStream.ReadByte();
                    bufferpos++;
                    if (bufferpos >= 4)
                    {
                        if (bufferreq[bufferpos - 1] == '\n' && bufferreq[bufferpos - 2] == '\r' && bufferreq[bufferpos - 3] == '\n' && bufferreq[bufferpos - 4] == '\r')
                        {
                            isHeaderMake = true;
                            break;
                        }
                    }
                }
                if (isHeaderMake && !isHeaderSent)
                {
                    string newHeader = System.Text.ASCIIEncoding.ASCII.GetString(bufferreq, 0, bufferpos);
                    newHeader = "Server: " + serverName + "/" + Version.ToString() + "\r\n" + newHeader;
                    newHeader = "Transfer-Encoding: chunked\r\n" + newHeader;
                    newHeader = "Connection: close\r\n" + newHeader;
                    string[] getHeaders = newHeader.Split('\n');
                    string status = "";
                    for (int q = 0; q < getHeaders.Length; q++)
                    {
                        if (getHeaders[q].StartsWith("Status:"))
                        {
                            status = getHeaders[q].Split(new char[] { ':' }, 2)[1].Trim();
                        }
                    }
                    if (status == "")
                    {
                        newHeader = "HTTP/1.1 200 OK\r\n" + newHeader; //status

                    }
                    else
                    {
                        newHeader = "HTTP/1.1 " + status + "\r\n" + newHeader; //status

                    }

                    // System.Windows.Forms.MessageBox.Show(newHeader);
                    socket.Send(ASCIIEncoding.ASCII.GetBytes(newHeader));
                    isHeaderSent = true;
                }
                if (isHeaderMake && isHeaderSent)
                {
                    bufferpos = 0;
                    while (bufferpos < 1024)
                    {
                        int val = reader.BaseStream.ReadByte();
                        if (val == -1)
                        {
                            break;
                        }
                        bufferreq[bufferpos] = (byte)val;
                        bufferpos++;
                    }

                    socket.Send(ASCIIEncoding.ASCII.GetBytes(Convert.ToString(bufferpos, 16) + "\r\n"));
                    socket.Send(bufferreq, 0, bufferpos, SocketFlags.None);
                    socket.Send(ASCIIEncoding.ASCII.GetBytes("\r\n"));
                    if (bufferpos != 1024)
                    {
                        break;
                    }
                    // System.Windows.Forms.MessageBox.Show("ok");

                }
                //System.Windows.Forms.MessageBox.Show(reader.ReadToEnd());
                System.Threading.Thread.Sleep(1);
            }
            socket.Send(ASCIIEncoding.ASCII.GetBytes("0\r\n\r\n"));

        }

        //normalny transfer
        private void ResponseNormal(System.Net.Sockets.Socket socket, string address, MethodType methodType)
        {
            if(methodType== MethodType.Options)
            {
                string response = CreateHeader(200,0, false);
                response += "Allow: GET,HEAD,POST,OPTIONS\r\n";
                response += "\r\n";
                socket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(response));
                socket.Close();
            }
            else
            {
            System.IO.FileInfo file = new System.IO.FileInfo(address);
            string response = CreateHeader(200, (int)file.Length, false);
           // response += "Content-Length: " + file.Length + "\r\n";
            response += "Accept-Ranges: bytes\r\n";
            for (int i = 0; i < Mimetypes.Count; i++)
            {
                if (address.EndsWith(Mimetypes[i].EndsWith))
                {
                    response += "Content-Type: " + Mimetypes[i].Type + "\r\n";
                    break;
                }
            }
            response += "\r\n";

            System.IO.StreamReader reader = new System.IO.StreamReader(address);
            socket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(response));
            byte[] buffer = new byte[1024];
            int bufferPos = 0;
            int bufferSize = 1024;
            while (bufferSize == 1024)
            {
                bufferSize = Math.Min((int)reader.BaseStream.Length - bufferPos, 1024);
                reader.BaseStream.Read(buffer, 0, bufferSize);
                socket.Send(buffer, bufferSize, SocketFlags.Partial);
                bufferPos += 1024;
            }
            reader.Close();
            }
            
        }

        private void ResponseRanges(System.Net.Sockets.Socket socket, string address, string rangeHeader)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(address);
            UInt64 startRange = 0;
            UInt64 endRange = 0;
            if (rangeHeader.Replace("Range: ", "").StartsWith("bytes="))
            {
                string[] parts = rangeHeader.Replace("Range: ", "").Replace("\r", "").Substring(6).Split('-');

                if (parts[0] != "")
                {
                    startRange = UInt64.Parse(parts[0]);
                }
                if (parts[1] != "")
                {
                    endRange = UInt64.Parse(parts[1]);
                }
            }
            int rangeSize = (int)((endRange == 0 ? (ulong)file.Length - 1 : endRange) - startRange);

            string response = CreateHeader(206, (rangeSize), false);
            
            response += "Accept-Ranges: bytes\r\n";
            response += "Content-Range: bytes " + startRange.ToString() + "-" + ((endRange == 0 ? (ulong)file.Length - 1 : endRange)).ToString() + "/" + file.Length.ToString() + "\r\n";
            for (int i = 0; i < Mimetypes.Count; i++)
            {
                if (address.EndsWith(Mimetypes[i].EndsWith))
                {
                    response += "Content-Type: " + Mimetypes[i].Type + "\r\n";
                    break;
                }
            }
            response += "\r\n";

            System.IO.StreamReader reader = new System.IO.StreamReader(address);
            reader.BaseStream.Seek((long)startRange, System.IO.SeekOrigin.Begin);
            socket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(response));
            byte[] buffer = new byte[1024];

            int bufferPos = 0;
            int bufferSize = 1024;

            while (bufferSize == 1024)
            {
                bufferSize = Math.Min(Math.Min((int)(reader.BaseStream.Length - (long)startRange) - bufferPos, rangeSize - bufferPos), 1024);
                reader.BaseStream.Read(buffer, 0, bufferSize);
                socket.Send(buffer, bufferSize, SocketFlags.Partial);
                bufferPos += 1024;
                rangeSize -= 1024;
            }
            reader.Close();
        }

        private void ErrorWrite(int number, System.Net.Sockets.Socket socket)
        {
            try
            {
                byte[] errorpage;
                //czy instnieje strona błędu
                string[] pages = System.IO.Directory.GetFiles(ErrorsPagesAddress, number + ".*");
                if (pages.Length > 0)
                {
                    System.IO.StreamReader read = new System.IO.StreamReader(pages[0]);
                    errorpage = new byte[read.BaseStream.Length];
                    read.BaseStream.Read(errorpage, 0, errorpage.Length);
                    read.Close();
                }
                else
                {
                    errorpage = System.Text.ASCIIEncoding.ASCII.GetBytes("<h1>" + number.ToString() + "</h1><h2>" + errorDescriptions[number] + "</h2>");
                }
                string response = CreateHeader(number, errorpage.Length,true);

                socket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(response));
                socket.Send(errorpage);
                socket.Close();
            }
            catch
            {

            }


        }
        public string CreateHeader(int responseCode, int contentLength, bool isHeaderEnded)
        {
            string res = "";
            res = "HTTP/1.1 " + responseCode.ToString() + " " + errorDescriptions[responseCode] + "\r\n";
            DateTime date = DateTime.Now.ToUniversalTime();
            res += "Date: " + date.ToString("R") + "\r\n";
            res += "Server: " + serverName + "/" + Version.ToString() + "\r\n";
            res += "Connection: close" + "\r\n";
            res += "Content-Length: " + contentLength.ToString()+ "\r\n" ;
            if (isHeaderEnded) {
 res += "\r\n";
            }
           
            return res;
        }

       public static void DirectoryShow(Socket socket, string address)
        {
            string result = "";
            result += "<!DOCTYPE html><html><head><title>Browse directory</title></head><body>";
            result += "<table>";
            result += "<tr><td><a href=\".\">.</a></td></tr>";
            result += "<tr><td><a href=\"..\">..</a></td></tr>";
            var directory = (new System.IO.DirectoryInfo(address));
            var directories = directory.EnumerateDirectories();
            foreach( var elem in directories)
            {
                result += "<tr><td><a href=\"./"+elem.Name+"/\">"+elem.Name+"</a></td></tr>";
            }
            var files = directory.EnumerateFiles();
            foreach (var elem in files)
            {
                result += "<tr><td><a href=\"./" + elem.Name + "\">" + elem.Name + "</a></td></tr>";
            }

            result += "</table></body></html>";

            int size = result.Length;
            result = "\r\n" + result;
            result = "Content-Type: text/html\r\n"+result;
            result = "Content-Length: " + size.ToString() +"\r\n"+ result;
            result = "Connection: close\r\n" + result;
            result = "HTTP/1.1 200 OK\r\n" + result;

            socket.Send(ASCIIEncoding.ASCII.GetBytes(result));

            socket.Close();
        }

       


    }
}
