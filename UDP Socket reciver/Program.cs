using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace UDP_Socket_reciver
{
    /// <summary>
    /// Version 0.1.1
    /// Copyright © Aske H Knudsen 2016, Aske@maxpedal.dk
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
    /// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    /// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    ///  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
    /// 
    /// This program will promt the user for ports to listen on, on start-up
    /// A thread will be spawned pr port
    /// The program will test each port, no error message if the port is not responding... It might crash?? 
    /// The program have been tested to handle up to a gigabit pr second connection in the current state. 
    /// That might be dependant on the disk on the host system

    /// 
    /// TODO:
    /// * Save the data from the ports
    /// * Mix the data from the ports
    /// * Determine the best way to handle recivers
    /// * Send the result to the correct reciver
    /// </summary>
    class Program
    {
        public static int randoPort = 9675;         //Random port, not used for much...
        public static int recivedPackages = 0;      //Int to keep track of the amount of recived packages
        //Main
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the ports you want to listen to seperated by a space"); 
            string Sports = Console.ReadLine(); //Reads the user input
            string[] ports = Sports.Split(' '); //Splits on spaces and insert results in an array
            foreach (string port in ports)      //Foreach to loop the ports
            {
                try //Catch errors, most likely from user-input
                {
                    int iPort;  //Temp storage for the port number
                    Int32.TryParse(port, out iPort); // Parse the string to an int
                    if (iPort <= 65535 && iPort > 0) //Check if the port number is in range
                    {
                        Thread th = new Thread(() => server(iPort)); //Initialize a thread per port, might be ineffective
                        th.Start(); //Start the thread
                    }
                    else //If not in range or not a number
                        Console.WriteLine(port + " is not a valid port, this will be ignored");  //Print a error message to the user
                }
                catch (Exception e) //Catch all errors 
                {
                    Console.WriteLine(e); //Write the errors, only for debug purposes
                }
                
            }
            Console.WriteLine("Press any key to exit"); //Not used, 
            Console.ReadKey(); //Keep the application from exitning
        }
        //EventHander for a recived package
        private static void DataRecived(IAsyncResult ar)
        {
            //Stopwatch sw = new Stopwatch(); //Setup a stopwatch
            //sw.Start(); // Start the stopwatch
            recivedPackages++; //Increment the amount of recived packages
            UdpClient c = (UdpClient)ar.AsyncState; //Create a Async UDP client
            IPEndPoint recivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0); // Set up endpoint
            Byte[] recivedBytes = c.EndReceive(ar, ref recivedIpEndPoint); //Array to keep the recived data
            String recivedText = ASCIIEncoding.ASCII.GetString(recivedBytes); //Convert data to ASCII and print in console
            Console.Write(          
                recivedIpEndPoint + //Write the endpoint IP
                ": " +  //Add a :
                recivedText + //Add the recived as text
                Environment.NewLine +  //Newline, mono compatible
                "Recived Packages: " + //Add text
                recivedPackages + //Add the the ammount of recived pacages, only for debug purposes
                Environment.NewLine   //Add a newline, mono compatible
                //"Time elapsed: " +
                //sw.ElapsedMilliseconds.ToString() //Timer stuff, debugging only
                ); 
            c.BeginReceive(DataRecived, ar.AsyncState); // Begin reciveing again
        }
        //UDP socket listener        
        private static void server(int port)
        {      
            UdpClient reciver = new UdpClient(port);                    //New instance of the UdpClient class 
            Console.WriteLine("Listening on: " + port.ToString());      //Starting asyncron receiving
            reciver.BeginReceive(DataRecived, reciver);                 //Start the UDP reciver
            //Sends a test message to self to test if the program is working as intended
            UdpClient sender = new UdpClient(randoPort++);
            Byte[] sendBytes = Encoding.ASCII.GetBytes("This is a test on port: " + port.ToString());
            sender.Send(sendBytes, sendBytes.Length, "localhost", port);
        }
    }
}
