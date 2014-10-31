/****************************************************************
 * This work is original work authored by Craig Baird, released *
 * under the Code Project Open Licence (CPOL) 1.02;             *
 * http://www.codeproject.com/info/cpol10.aspx                  *
 * This work is provided as is, no guarentees are made as to    *
 * suitability of this work for any specific purpose, use it at *
 * your own risk.                                               *
 * If this work is redistributed in code form this header must  *
 * be included and unchanged.                                   *
 * Any modifications made, other than by the original author,   *
 * shall be listed below.                                       *
 * Where applicable any headers added with modifications shall  *
 * also be included.                                            *
 ****************************************************************/

namespace SageNetTuner.Network
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public delegate void tcpServerConnectionChanged(TcpServerConnection connection);
    public delegate void tcpServerError(TcpServer server, Exception e);

    public partial class TcpServer : Component
    {
        private List<TcpServerConnection> connections;
        private TcpListener listener;

        private Thread listenThread;
        private Thread sendThread;

        private bool m_isOpen;

        private int m_port;
        private int m_maxSendAttempts;
        private int m_idleTime;
        private int m_maxCallbackThreads;
        private int m_verifyConnectionInterval;
        private Encoding m_encoding;

        private SemaphoreSlim sem;
        private bool waiting;

        private int activeThreads;
        private object activeThreadsLock = new object();

        public event tcpServerConnectionChanged OnConnect = null;
        public event tcpServerConnectionChanged OnDataAvailable = null;
        public event tcpServerError OnError = null;

        public TcpServer() 
        {
            this.InitializeComponent();

            this.initialise();
        }

        public TcpServer(IContainer container)
        {
            container.Add(this);

            this.InitializeComponent();

            this.initialise();
        }

        private void initialise()
        {
            this.connections = new List<TcpServerConnection>();
            this.listener = null;

            this.listenThread = null;
            this.sendThread = null;

            this.m_port = -1;
            this.m_maxSendAttempts = 3;
            this.m_isOpen = false;
            this.m_idleTime = 50;
            this.m_maxCallbackThreads = 100;
            this.m_verifyConnectionInterval = 100;
            this.m_encoding = Encoding.ASCII;

            this.sem = new SemaphoreSlim(0);
            this.waiting = false;

            this.activeThreads = 0;
        }

        public int Port
        {
            get
            {
                return this.m_port;
            }
            set
            {
                if (value < 0)
                {
                    return;
                }

                if (this.m_port == value)
                {
                    return;
                }

                if (this.m_isOpen)
                {
                    throw new Exception("Invalid attempt to change port while still open.\nPlease close port before changing.");
                }

                this.m_port = value;
                if (this.listener == null)
                {
                    //this should only be called the first time.
                    this.listener = new TcpListener(IPAddress.Any, this.m_port);
                }
                else
                {
                    this.listener.Server.Bind(new IPEndPoint(IPAddress.Any, this.m_port));
                }
            }
        }

        public int MaxSendAttempts
        {
            get
            {
                return this.m_maxSendAttempts;
            }
            set
            {
                this.m_maxSendAttempts = value;
            }
        }

        [Browsable(false)]
        public bool IsOpen
        {
            get
            {
                return this.m_isOpen;
            }
            set
            {
                if (this.m_isOpen == value)
                {
                    return;
                }

                if (value)
                {
                    this.Open();
                }
                else
                {
                    this.Close();
                }
            }
        }

        public List<TcpServerConnection> Connections
        {
            get
            {
                List<TcpServerConnection> rv = new List<TcpServerConnection>();
                rv.AddRange(this.connections);
                return rv;
            }
        }

        public int IdleTime
        {
            get
            {
                return this.m_idleTime;
            }
            set
            {
                this.m_idleTime = value;
            }
        }

        public int MaxCallbackThreads
        {
            get
            {
                return this.m_maxCallbackThreads;
            }
            set
            {
                this.m_maxCallbackThreads = value;
            }
        }

        public int VerifyConnectionInterval
        {
            get
            {
                return this.m_verifyConnectionInterval;
            }
            set
            {
                this.m_verifyConnectionInterval = value;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return this.m_encoding;
            }
            set
            {
                Encoding oldEncoding = this.m_encoding;
                this.m_encoding = value;
                foreach (TcpServerConnection client in this.connections)
                {
                    if (client.Encoding == oldEncoding)
                    {
                        client.Encoding = this.m_encoding;
                    }
                }
            }
        }

        public void setEncoding(Encoding encoding, bool changeAllClients)
        {
            Encoding oldEncoding = this.m_encoding;
            this.m_encoding = encoding;
            if (changeAllClients)
            {
                foreach (TcpServerConnection client in this.connections)
                {
                    client.Encoding = this.m_encoding;
                }
            }
        }

        private void runListener()
        {
            while (this.m_isOpen && this.m_port >= 0)
            {
                try
                {
                    if (this.listener.Pending())
                    {
                        TcpClient socket = this.listener.AcceptTcpClient();
                        TcpServerConnection conn = new TcpServerConnection(socket, this.m_encoding);

                        if (this.OnConnect != null)
                        {
                            lock (this.activeThreadsLock)
                            {
                                this.activeThreads++;
                            }
                            conn.CallbackThread = new Thread(() =>
                            {
                                this.OnConnect(conn);
                            });
                            conn.CallbackThread.Start();
                        }

                        lock (this.connections)
                        {
                            this.connections.Add(conn);
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(this.m_idleTime);
                    }
                }
                catch (ThreadInterruptedException) { } //thread is interrupted when we quit
                catch (Exception e)
                {
                    if (this.m_isOpen && this.OnError != null)
                    {
                        this.OnError(this, e);
                    }
                }
            }
        }

        private void runSender()
        {
            while (this.m_isOpen && this.m_port >= 0)
            {
                try
                {
                    bool moreWork = false;
                    for (int i = 0; i < this.connections.Count; i++)
                    {
                        if (this.connections[i].CallbackThread != null)
                        {
                            try
                            {
                                this.connections[i].CallbackThread = null;
                                lock (this.activeThreadsLock)
                                {
                                    this.activeThreads--;
                                }
                            }
                            catch (Exception)
                            {
                                //an exception is thrown when setting thread and old thread hasn't terminated
                                //we don't need to handle the exception, it just prevents decrementing activeThreads
                            }
                        }

                        if (this.connections[i].CallbackThread != null) { }
                        else if (this.connections[i].IsConnected() && 
                            (this.connections[i].LastVerifyTime.AddMilliseconds(this.m_verifyConnectionInterval) > DateTime.UtcNow || 
                             this.connections[i].VerifyConnected()))
                        {
                            moreWork = moreWork || this.processConnection(this.connections[i]);
                        }
                        else
                        {
                            lock (this.connections)
                            {
                                this.connections.RemoveAt(i);
                                i--;
                            }
                        }
                    }

                    if (!moreWork)
                    {
                        System.Threading.Thread.Yield();
                        lock (this.sem)
                        {
                            foreach (TcpServerConnection conn in this.connections)
                            {
                                if (conn.HasMoreWork())
                                {
                                    moreWork = true;
                                    break;
                                }
                            }
                        }
                        if (!moreWork)
                        {
                            this.waiting = true;
                            this.sem.Wait(this.m_idleTime);
                            this.waiting = false;
                        }
                    }
                }
                catch (ThreadInterruptedException) { } //thread is interrupted when we quit
                catch (Exception e)
                {
                    if (this.m_isOpen && this.OnError != null)
                    {
                        this.OnError(this, e);
                    }
                }
            }
        }

        private bool processConnection(TcpServerConnection conn)
        {
            bool moreWork = false;
            if (conn.ProcessOutgoing(this.m_maxSendAttempts))
            {
                moreWork = true;
            }

            if (this.OnDataAvailable != null && this.activeThreads < this.m_maxCallbackThreads && conn.Socket.Available > 0)
            {
                lock (this.activeThreadsLock)
                {
                    this.activeThreads++;
                }
                conn.CallbackThread = new Thread(() =>
                {
                    this.OnDataAvailable(conn);
                });
                conn.CallbackThread.Start();
                Thread.Yield();
            }
            return moreWork;
        }

        public void Open()
        {
            lock (this)
            {
                if (this.m_isOpen)
                {
                    //already open, no work to do
                    return;
                }
                if (this.m_port < 0)
                {
                    throw new Exception("Invalid port");
                }

                try
                {
                    this.listener.Start(5);
                }
                catch (Exception)
                {
                    this.listener.Stop();
                    this.listener = new TcpListener(IPAddress.Any, this.m_port);
                    this.listener.Start(5);
                }

                this.m_isOpen = true;

                this.listenThread = new Thread(new ThreadStart(this.runListener));
                this.listenThread.Start();

                this.sendThread = new Thread(new ThreadStart(this.runSender));
                this.sendThread.Start();
            }
        }

        public void Close()
        {
            if (!this.m_isOpen)
            {
                return;
            }

            lock (this)
            {
                this.m_isOpen = false;
                foreach (TcpServerConnection conn in this.connections)
                {
                    conn.ForceDisconnect();
                }
                try
                {
                    if (this.listenThread.IsAlive)
                    {
                        this.listenThread.Interrupt();

                        Thread.Yield();
                        if(this.listenThread.IsAlive)
                        {
                            this.listenThread.Abort();
                        }
                    }
                }
                catch (System.Security.SecurityException) { }
                try
                {
                    if (this.sendThread.IsAlive)
                    {
                        this.sendThread.Interrupt();

                        Thread.Yield();
                        if(this.sendThread.IsAlive)
                        {
                            this.sendThread.Abort();
                        }
                    }
                }
                catch (System.Security.SecurityException) { }
            }
            this.listener.Stop();

            lock (this.connections)
            {
                this.connections.Clear();
            }

            this.listenThread = null;
            this.sendThread = null;
            GC.Collect();
        }

        public void Send(string data)
        {
            lock (this.sem)
            {
                foreach (TcpServerConnection conn in this.connections)
                {
                    conn.SendData(data);
                }
                Thread.Yield();
                if (this.waiting)
                {
                    this.sem.Release();
                    this.waiting = false;
                }
            }
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }

        #endregion

    }

}
