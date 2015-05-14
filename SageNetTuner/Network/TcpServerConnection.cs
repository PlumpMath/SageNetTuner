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
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class TcpServerConnection
    {
        private TcpClient _socket;
        private readonly List<byte[]> _messagesToSend;
        private int _attemptCount;
        private Thread _thread = null;
        private DateTime _lastVerifyTime;
        private Encoding _encoding;

        public TcpServerConnection(TcpClient sock, Encoding encoding)
        {
            this._socket = sock;
            this._messagesToSend = new List<byte[]>();
            this._attemptCount = 0;

            this._lastVerifyTime = DateTime.UtcNow;
            this._encoding = encoding;
        }

        public bool IsConnected()
        {
            try
            {
                return this._socket.Connected;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool VerifyConnected()
        {
            //note: `Available` is checked before because it's faster,
            //`Available` is also checked after to prevent a race condition.
            bool connected = this._socket.Client.Available != 0 || 
                !this._socket.Client.Poll(1, SelectMode.SelectRead) || 
                this._socket.Client.Available != 0;
            this._lastVerifyTime = DateTime.UtcNow;
            return connected;
        }

        public bool ProcessOutgoing(int maxSendAttempts)
        {
            lock (this._socket)
            {
                if (!this._socket.Connected)
                {
                    this._messagesToSend.Clear();
                    return false;
                }

                if (this._messagesToSend.Count == 0)
                {
                    return false;
                }

                NetworkStream stream = this._socket.GetStream();
                try
                {
                    stream.Write(this._messagesToSend[0], 0, this._messagesToSend[0].Length);

                    lock (this._messagesToSend)
                    {
                        this._messagesToSend.RemoveAt(0);
                    }
                    this._attemptCount = 0;
                }
                catch (System.IO.IOException)
                {
                    //occurs when there's an error writing to network
                    this._attemptCount++;
                    if (this._attemptCount >= maxSendAttempts)
                    {
                        //TODO log error

                        lock (this._messagesToSend)
                        {
                            this._messagesToSend.RemoveAt(0);
                        }
                        this._attemptCount = 0;
                    }
                }
                catch (ObjectDisposedException)
                {
                    //occurs when stream is closed
                    this._socket.Close();
                    return false;
                }
            }
            return this._messagesToSend.Count != 0;
        }

        public void SendData(string data)
        {
            byte[] array = this._encoding.GetBytes(data);
            lock (this._messagesToSend)
            {
                this._messagesToSend.Add(array);
            }
        }

        public void ForceDisconnect()
        {
            lock (this._socket)
            {
                this._socket.Close();
            }
        }

        public bool HasMoreWork()
        {
            bool rc;
            try
            {
                rc = this._messagesToSend.Count > 0 || (this.Socket.Available > 0 && this.CanStartNewThread());
            }
            catch (ObjectDisposedException)
            {
                rc = false;
            }
            catch (SocketException)
            {
                rc = false;
            }

            //return this._messagesToSend.Count > 0 || (this.Socket.Available > 0 && this.CanStartNewThread());
            return rc;
        }

        private bool CanStartNewThread()
        {
            if (this._thread == null)
            {
                return true;
            }
            return (this._thread.ThreadState & (ThreadState.Aborted | ThreadState.Stopped)) != 0 &&
                   (this._thread.ThreadState & ThreadState.Unstarted) == 0;
        }

        public TcpClient Socket
        {
            get
            {
                return this._socket;
            }
            set
            {
                this._socket = value;
            }
        }

        public IPAddress ClientAddress
        {
            get
            {
                if (VerifyConnected())
                {
                    return ((IPEndPoint)_socket.Client.RemoteEndPoint).Address;
                }
                return IPAddress.Any;
            }
        }

        public Thread CallbackThread
        {
            get
            {
                return this._thread;
            }
            set
            {
                if (!this.CanStartNewThread())
                {
                    throw new Exception("Cannot override TcpServerConnection Callback Thread. The old thread is still running.");
                }
                this._thread = value;
            }
        }

        public DateTime LastVerifyTime
        {
            get
            {
                return this._lastVerifyTime;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return this._encoding;
            }
            set
            {
                this._encoding = value;
            }
        }
    }
}
