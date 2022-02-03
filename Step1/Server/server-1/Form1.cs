using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace server_1
{

    public partial class Form1 : Form
    {

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Dictionary<string, Tuple<Socket, string>> clients = new Dictionary<string, Tuple<Socket, string>>(); // keeps clients and their information
        string server_pub;
        string server_pub_prv;
        bool terminating = false; // server connection
        bool listening = false; // we are not listening anyone at first
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }



        private void bListen_Click(object sender, EventArgs e)
        {


            server_pub = System.IO.File.ReadAllText("server_pub.txt");
            server_pub_prv = System.IO.File.ReadAllText("server_pub_prv.txt");

            int serverPort;
            try
            {
                //If the port value taken from the GUI is successfully parsed:
                if (Int32.TryParse(tPort.Text, out serverPort))
                {
                    //Connection to the default IP address and given port for listening.
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                    serverSocket.Bind(endPoint);
                    serverSocket.Listen(3);


                    listening = true; //Server has started to listening the port.
                    bListen.Enabled = false;
                    tPort.Enabled = false;
                    bListen.Enabled = false;

                    Thread acceptThread = new Thread(Accept);
                    acceptThread.Start(); //Thread which accepts the clients' connection requests has started.
                    Control.CheckForIllegalCrossThreadCalls = false;
                    logs.AppendText("Started listening on port: " + serverPort + "\n");

                }
                else
                {
                    //if the connection to the port is not successful, it prints out the message.
                    logs.AppendText("Please check port number \n");
                }
            }
            catch
            {
                //If the port is being used by another service, it prints out the message.
                logs.AppendText("Entered port number is being used! Try another\n");
            }

        }

        private void Accept()
        {
            //While the server is listening the port.
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept(); // checks if there is a client wants to connect, if none it is blocked here
                    bool rejected = true;
                    bool sameName = false;
                    string clientName = "";
                    try
                    {
                        Byte[] buffer = new Byte[64]; // Stream type gets it as byte
                        newClient.Receive(buffer);
                        //Server receives the client name in a buffer.
                        clientName = Encoding.Default.GetString(buffer);
                        // can be a null character, to handle it do this below
                        // to match the 64 bits null char may be padded, so remove it
                        clientName = clientName.Substring(0, clientName.IndexOf("\0"));
                        string message = "";

                        Byte[] buffer_send = new Byte[64];
                        Byte[] rndBufferSend = new Byte[16];
                        Byte[] rndBufferSend2 = new Byte[32];


                        //If the client name is unique, adds it to the clients list, 
                        //else, rejects it and terminates the connection.
                        if (uniqueName(clientName))
                        {
                            //Server sends a welcome message to the client.
                            message = "Welcome " + clientName + " \n";
                            logs.AppendText(clientName + " has connected.\n");

                            //It puts the client name and its socket in a tuple.
                            Tuple<Socket, string> temp = new Tuple<Socket, string>(newClient, "");
                            //Then it adds it to the list of clients.
                            clients.Add(clientName, temp);

                            //buffer_send = Encoding.Default.GetBytes(message);
                            Random rnd = new Random();
                            rnd.NextBytes(rndBufferSend);
                            newClient.Send(rndBufferSend);
                            logs.AppendText("Random number has been created for "+ clientName+".\n");
                            Byte[] bSignature = new Byte[512];
                            newClient.Receive(bSignature);
                            logs.AppendText("Signature has been received from " + clientName + ".\n");
                            string clientSign = Encoding.Default.GetString(bSignature);
                            clientSign = clientSign.Substring(0, clientSign.IndexOf("\0"));

                            //dosya loading örneği:
                            // Load keys and plaintexts from Debug/bin folder

                            // RSA 2048 key pair
                      
                            string clientPubKey = System.IO.File.ReadAllText(clientName + "_pub.txt");

                            string rndString = Encoding.Default.GetString(rndBufferSend);
                            try
                            {

                                bool result = verifyWithRSA(rndString, 4096, clientPubKey, bSignature);
                                if (result)
                                {
                                    logs.AppendText(clientName+ " is verified! \n");

                                    Random rnd2 = new Random();
                                    rnd2.NextBytes(rndBufferSend2);
                                    string strRnd256 = Encoding.Default.GetString(rndBufferSend2);
                                    try
                                    {
                                        byte[] encHmacKey = encryptWithRSA(strRnd256, 4096, clientPubKey);
                                        string strEnchHmacKey = Encoding.Default.GetString(encHmacKey);
                                        string posAck = "<verified>";

                                        byte[] toSend = signWithRSA(strEnchHmacKey + posAck, 4096, server_pub_prv);
                                        string strtoSend = Encoding.Default.GetString(toSend);
                                        string strToSendFinal = strEnchHmacKey + strtoSend + posAck;

                                        byte[] toSendFinal = new Byte[1040];
                                        toSendFinal = Encoding.Default.GetBytes(strToSendFinal);

                                        newClient.Send(toSendFinal);
                                        logs.AppendText("Encrypted HMAC sent to client.\n");
                               
                                        
                                        rejected = false; //????????yerin de emin ol
                                        Thread waitThread = new Thread(()=>Wait(clients[clientName], clientName));
                                        waitThread.Start();

                                    }
                                    catch
                                    {

                                        message = "Could not encrpyt with RSA! \n";
                                        logs.AppendText(message);
                                        newClient.Close();
                                        updateList(clientName);
                                    }


                                    // newClient.Send(rndBufferSend2);




                                }
                                else
                                {
                                    message = "Could not verify signature of "+clientName+"! \n";
                                    logs.AppendText(message);
                                    buffer_send = Encoding.Default.GetBytes("<notVerified>");

                                    byte[] toSend = signWithRSA("<notVerified>", 4096, server_pub_prv);
                                    byte[] empty = new Byte[1040];
                                    string strToSend = Encoding.Default.GetString(toSend) + "<notVerified>";

                                    byte[] not_verif = Encoding.Default.GetBytes("<notVerified>");                                    

                                    Array.Copy(toSend, 0, empty, 512, 512);
                                    Array.Copy(not_verif, 0, empty, 1024, 16);

                                    toSend = Encoding.Default.GetBytes(strToSend);
                                    newClient.Send(toSend);
                                    newClient.Close();
                                    updateList(clientName);
                                }
                            }
                            catch
                            {

                                message = "Could not authenticate "+clientName+"! \n";
                                logs.AppendText(message);
                                buffer_send = Encoding.Default.GetBytes("<notVerified>");


                                byte[] toSend = signWithRSA("<notVerified>", 4096, server_pub_prv);
                                string strToSend = Encoding.Default.GetString(toSend) + "<notVerified>";
                                toSend = Encoding.Default.GetBytes(strToSend);
                                newClient.Send(toSend);
                                newClient.Close();
                                updateList(clientName);
                            }
                        }
                        else
                        {
                            //Server sends rejection message to client and closes the socket.                     
                            message = clientName + " already exists! \n";
                            sameName = true;
                            logs.AppendText(message);
                            buffer_send = Encoding.Default.GetBytes("<alreadyExists>");
                            newClient.Send(buffer_send);
                            newClient.Close();
                            //updateList(clientName);
                        }
                    }
                    catch
                    {
                        // client may disconnect or it may be terminating
                        if (!terminating)
                        {
                            logs.AppendText("Client could not connect \n");
                        }
                        newClient.Close(); // close the socket

                        //if an error occured after new client is added to the list, it finds and removes that client tuple.
                        
                        updateList(clientName);
                        
                        

                    }

                    //If the client is not rejected, it starts the receive thread.
                    if (rejected)
                    {
                        //Thread receiveThread = new Thread(() => Receive(getClient(newClient)));
                        //receiveThread.Start();
                        string message = clientName+" is rejected! \n";
                        logs.AppendText(message);
                        if (!sameName)
                        {
                            updateList(clientName);
                        }
                    }
                }
                catch
                {
                    // Server may disconnect or it may be terminating
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        logs.AppendText("The server socket stopped working. \n");
                    }
                }
            }
        }

        private void Wait(Tuple <Socket, string> client, string name)
        {
            while (SocketConnected(client.Item1))
            { }

            logs.AppendText(name+" disconnected!\n");
            client.Item1.Close();
            updateList(name);
        }

        //ne yapılmalı?
        //1: she/he enters password from the client GUI. 
        //The encrypted 4096-bit RSA private key, which is given in the project pack, is decrypted using the hash of the entered password;
        /*
        private void Receive(Tuple<Socket, string> thisClient)
        {

        }
        */


        //If the client name is a unique name, returns true, else false.
        private bool uniqueName(string name)
        {
            return !(clients.ContainsKey(name)); 
        }

        //if the client is in the list, it finds and removes that client tuple.
        private void updateList(string name)
        {
            clients.Remove(name); 
        }

        private bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        // verifying with RSA
        static bool verifyWithRSA(string input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            result = rsaObject.VerifyData(byteInput, "SHA512", signature);

            return result;
        }
        // RSA encryption with varying bit length
        static byte[] encryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            //true flag is set to perform direct RSA encryption using OAEP padding
            result = rsaObject.Encrypt(byteInput, true);

            return result;
        }

        // signing with RSA
        static byte[] signWithRSA(string input, int algoLength, string xmlString)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            result = rsaObject.SignData(byteInput, "SHA512");

            return result;
        }

        //handles the application closing
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //update the states
            listening = false;
            terminating = true;

            //close the app
            Environment.Exit(0);
        }


    }

}

