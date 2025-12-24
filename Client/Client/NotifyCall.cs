using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Net;



public class NotifyCall
{


    public static string LoginEMPid;//88780
    public static string ServerIP;//"10.80.231.39"
    public static int ServerPort;//10



    // ManualResetEvent instances signal completion.
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    // The response from the remote device.
    private static String response = String.Empty;

    static public void info(string text)
    {

        NotifyWindow nw = new NotifyWindow();

        string Title = "";

        nw = new NotifyWindow(Title, text);
        nw.TitleClicked += new System.EventHandler(titleClick);

        nw.TextClicked += new System.EventHandler(textClick);
        nw.SetDimensions(130, 110);
        nw.Notify();
    }

    static protected void titleClick(object sender, System.EventArgs e)
    {

    }

    static protected void textClick(object sender, System.EventArgs e)
    {

    }


    public void Run()
    {
        while (true)
        {

            response = "";
            Thread t = new Thread(delegate() { StartClient(); });
            t.Start();
            t.Join();
            Thread.Sleep(30000);

        }
     
    }




    private static void StartClient()
    {

        // Establish the remote endpoint for the socket.
        // The name of the 
        // remote device is "host.contoso.com".
        IPHostEntry ipHostInfo = Dns.Resolve(ServerIP);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, ServerPort);

        // Create a TCP/IP socket.
        Socket client = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        // Connect to a remote device.
        try
        {


            // Connect to the remote endpoint.
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            if (client.Connected)
            {

                // Send test data to the remote device.
                Send(client, LoginEMPid);
                sendDone.WaitOne();
                sendDone.Reset();
                // Receive the response from the remote device.
                Receive(client);
                receiveDone.WaitOne();
                receiveDone.Reset();
                if (response != string.Empty)
                { info(response); }
                connectDone.Reset();

            }
        }
        catch (Exception e)
        {

           //Console.WriteLine(e.ToString());
        }
        finally
        {

            // Release the socket.
          //  client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.
            client.EndConnect(ar);

            //Console.WriteLine("Socket connected to {0}",
            //    client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.
            connectDone.Set();
        }
        catch (Exception e)
        {
          // Console.WriteLine(e.ToString());
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
         //  Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket 
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                // Get the rest of the data.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // All the data has arrived; put it in response.
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                // Signal that all bytes have been received.
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
         // Console.WriteLine(e.ToString());
        }
    }

    private static void Send(Socket client, String data)
    {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.UTF8.GetBytes(data);

        // Begin sending the data to the remote device.
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
            //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            // Signal that all bytes have been sent.
            sendDone.Set();
        }
        catch (Exception e)
        {
         //  Console.WriteLine(e.ToString());
        }
    }


}

