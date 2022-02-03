using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;


namespace client
{
    public partial class Form1 : Form
    {
        //state variables we keep during the execution
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        string username;
        string server_pub;
        string s_RSA_decrypted;

        //constructor
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        //connect and disconnect button operation
        private void connect_button_Click(object sender, EventArgs e)
        {
            //if the user is not already connected (i.e. button text is connect)
            if (!connected)
            {
                //create the socket
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //read the IP from the GUI
                string IP = ip_box.Text;

                int portNum;
                //check for correctness of the port number and whether IP address has been entered
                if (Int32.TryParse(port_box.Text, out portNum) && ip_box.Text.Length != 0)
                {
                    try
                    {
                        //read the username from GUI
                        username = username_box.Text;

                        //check for correctness of the entered name (not empty, shorter than 64 characters and is alphanumeric)
                        if (username != "" && username.Length <= 64 && Regex.IsMatch(username, "^[a-zA-Z0-9]*$"))
                        {
                            byte[] sha384 = hashWithSHA384(password_box.Text);

                            byte[] AES_enc_key = new Byte[32];
                            byte[] AES_IV = new Byte[16];
                            Array.Copy(sha384, 0, AES_enc_key, 0, 32);  // source, source_start_index, copy_dest, copy_dest_start_index, copy_dest_end_index
                            Array.Copy(sha384, 32, AES_IV, 0, 16);

                            string RSAxmlKey4096;

                            try
                            {
                                string RSAxmlKey4096_hex = System.IO.File.ReadAllText("enc_" + username + "_pub_prv.txt");
                                byte[] RSAxmlKey4096_byte = hexStringToByteArray(RSAxmlKey4096_hex);
                                RSAxmlKey4096 = Encoding.Default.GetString(RSAxmlKey4096_byte);



                                try
                                {
                                    //RSAxmlKey4096 = Encoding.Default.GetString(b_RSAxmlKey4096);
                                    byte[] RSA_decrypted = decryptWithAES256(RSAxmlKey4096, AES_enc_key, AES_IV);
                                    s_RSA_decrypted = Encoding.Default.GetString(RSA_decrypted);
                                }
                                catch
                                {
                                    logs.AppendText("Password is incorrect.\n");
                                    return;
                                }

                            }
                            catch
                            {
                                logs.AppendText("Username is incorrect.\n");
                                return;
                            }


                            //connect the socket
                            clientSocket.Connect(IP, portNum);

                            Byte[] username_buffer = new Byte[64];
                            username_buffer = Encoding.Default.GetBytes(username);
                            clientSocket.Send(username_buffer);

                            //change the button text to disconnect
                            connect_button.Text = "Disconnect";
                            logs.AppendText("Connected to the server!\n");
                            //update the state
                            connected = true;

                            Thread verifyThread = new Thread(Verification);
                            verifyThread.Start();

                            
                        }

                        else
                        {
                            //update the state
                            connected = false;


                            //display the appropriate message
                            logs.AppendText("Username cannot be empty or longer than 64 characters, and must consist of alphanumeric characters!\n");
                        }
                    }
                    //we go into this catch if there was a problem with connecting to the server
                    catch
                    {
                        //if we are not terminating and we were connected while this happened
                        if (!terminating & connected)
                        {
                            //display the appropriate message
                            logs.AppendText("The server has disconnected\n");

                            //change the button text to connect
                            connect_button.Text = "Connect";

                            //disable the sending related buttons

                        }

                        connected = false;
                        clientSocket.Close();
                        //display the appropriate message
                        logs.AppendText("Could not connect to the server!\n");
                    }
                }
                //if the port or IP were erronous
                else
                {
                    connected = false;
                    //display the appropriate message
                    logs.AppendText("Check the port or IP\n");
                }
            }

            else
            {

                //update the state
                connected = false;

                //change the button text to connect
                connect_button.Text = "Connect";

                //diplay the approptiate message
                logs.AppendText("Disconnected from the server!\n");

                //close the socket
                clientSocket.Close();
            }
        }

        private void Verification()
        {
            //while we are connected
            while (connected)
            {
                try
                {
                    Byte[] random_number_buffer = new Byte[128];
                    clientSocket.Receive(random_number_buffer);

                    string random_number = Encoding.Default.GetString(random_number_buffer).Trim('\0');


                    if (random_number == "<alreadyExists>")
                    {
                        logs.AppendText("This username is already connected. Try with a different name.\n");
                        connected = false;
                        connect_button.Text = "Connect";
                        clientSocket.Close();
                    }

                    else
                    {
                        logs.AppendText("Server sent a 128-bit random number\n");

                        try
                        {

                            byte[] hmacsha512 = signWithRSA(random_number, 4096, s_RSA_decrypted); //applyHMACwithSHA512(random_number, RSA_decrypted);
                            clientSocket.Send(hmacsha512);
                            logs.AppendText("Client sent signed random number\n");


                            byte[] receive_buffer = new Byte[1040];
                            clientSocket.Receive(receive_buffer);

                            byte[] enc_hmac_key = new Byte[512];   // enc hmac of 256-bit random number
                            byte[] signed_key_ack = new Byte[512];
                            byte[] ack = new Byte[16];

                            Array.Copy(receive_buffer, 0, enc_hmac_key, 0, 512);
                            Array.Copy(receive_buffer, 512, signed_key_ack, 0, 512);
                            Array.Copy(receive_buffer, 1024, ack, 0, 16);

                            string receive_message_auth = Encoding.Default.GetString(ack).Trim('\0');


                            if (receive_message_auth == "<verified>")
                            {
                                logs.AppendText("Verification of 256-bit random number is successful.\n");

                                server_pub = System.IO.File.ReadAllText("server_pub.txt");

                                string hmac_key = Encoding.Default.GetString(enc_hmac_key);
                                string input = hmac_key + "<verified>";
                                bool verificationResult = verifyWithRSA(input, 4096, server_pub, signed_key_ack);

                                if (verificationResult)
                                {
                                    byte[] decryptedRSA = decryptWithRSA(hmac_key, 4096, s_RSA_decrypted);
                                    logs.AppendText("Decrytion with RSA of 256-bit number HMAC is successful.\nServer is verified.\n");
                                }
                                else
                                {
                                    logs.AppendText("Verified with RSA failed.\nServer is not verified.\n");
                                    connected = false;
                                    connect_button.Text = "Connect";
                                    clientSocket.Close();

                                }

                            }

                            else
                            {
                                logs.AppendText("Verification of 256-bit random number is failed.\n");
                                connect_button.Text = "Connect";
                                clientSocket.Close();
                                connected = false;

                            }

                        }
                        catch
                        {
                            logs.AppendText("Password is not correct!\n");
                            connect_button.Text = "Connect";
                            clientSocket.Close();
                            connected = false;
                        }

                    }

                }


                catch
                {
                    //if we are not terminating and are still connected
                    if (!terminating && connected)
                    {
                        //display the appropriate message
                        logs.AppendText("The server has disconnected\n");

                        //change to button text to connect
                        connect_button.Text = "Connect";

                    }

                    //close the socket
                    clientSocket.Close();

                    //update the state
                    connected = false;
                }

            }

        }







        //handles the application closing
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //update the states
            connected = false;
            terminating = true;

            //close the app
            Environment.Exit(0);
        }

        private void port_label_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void ip_box_TextChanged(object sender, EventArgs e)
        {

        }


        /// HELPER FUNCTIONS
        static byte[] hashWithSHA384(string input)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create a hasher object from System.Security.Cryptography
            SHA384CryptoServiceProvider sha384Hasher = new SHA384CryptoServiceProvider();
            // hash and save the resulting byte array
            byte[] result = sha384Hasher.ComputeHash(byteInput);

            return result;
        }
        // decryption with AES-256
        static byte[] decryptWithAES256(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-256
            aesObject.KeySize = 256;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            result = decryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);

            return result;
        }

        // Decrypt a string into a string using a key and an IV 
        static string Decrypt(string cipherData, byte[] key, byte[] iv)
        {

            try
            {
                using (var rijndaelManaged =
                       new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CFB })
                using (var memoryStream =
                       new MemoryStream(Convert.FromBase64String(cipherData)))
                using (var cryptoStream =
                       new CryptoStream(memoryStream,
                           rijndaelManaged.CreateDecryptor(key, iv),
                           CryptoStreamMode.Read))
                {
                    return new StreamReader(cryptoStream).ReadToEnd();
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
            // You may want to catch more exceptions here...
        }




        // HMAC with SHA-512
        static byte[] applyHMACwithSHA512(string input, byte[] key)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create HMAC applier object from System.Security.Cryptography
            HMACSHA512 hmacSHA512 = new HMACSHA512(key);
            // get the result of HMAC operation
            byte[] result = hmacSHA512.ComputeHash(byteInput);

            return result;
        }


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



        static bool verifyWithRSA(string input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            try
            {
                result = rsaObject.VerifyData(byteInput, "SHA512", signature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
        static byte[] decryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                result = rsaObject.Decrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }


    }
}
