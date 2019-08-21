using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using System.IO.Ports;
using System.IO;
using Lambda.LambdaGenPS.Interop;
//using System.Timers;

namespace CCControls
{
    public partial class CurrCyclCtrls : Form
    {
        ILambdaGenPS GenIvi;
        private object visa;

        public CurrCyclCtrls()
        {
            InitializeComponent();
        }

        public class Globals
        {
            public static string DEFAULT_BIAS_CURRENT = "0";
            public static string DEFAULT_BIAS_ON_TIME = "7";
            public static string DEFAULT_BIAS_OFF_TIME = "5";
            public static string DEFAULT_OVERTEMP_SETPOINT = "100";
            public static string DEFAULT_CURRENT_ON_SETPOINT = "85";
            public static string DEFAULT_CURRENT_OFF_SETPOINT = "25";
            public static string BAUDRATE = "57600";
            public static int BIAS_CURRENT_STATUS_INITIAL = 0;
            public static bool INTERLOCK = true;
            public static string VOLTAGE_COMPLIANCE = "60";
            public static int VERBOSE = 1;
            public static double SERIAL_WAIT_TIME = 0.02;
            public static int GRAPH_FREQUENCY = 500;
            public static int GRAPH_COUNTER = 0;
            public static bool[] PS_CONNECTION = { false, false, false, false, false, false,
                                                    false, false, false, false, false, false };
            public static double[] PS_CURRENTS = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                                     0.0, 0.0, 0.0, 0.0, 0.0, 0.0,};
            public static double[] PS_VOLTAGES = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                                     0.0, 0.0, 0.0, 0.0, 0.0, 0.0,};
            public static int[] CH_TEMPS = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            public static int[] FAN_SPEEDS = { 0, 0, 0, 0, 0, 0, 0, 0 };
            public static int[] CYCLE_NUMBERS = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public static string[] FILENAMES = { "Default 1.txt", "Default 2.txt", "Default 3.txt", "Default 4.txt",
                                                "Default 5.txt", "Default 6.txt", "Default 7.txt", "Default 8.txt",
                                                "Default 9.txt", "Default 10.txt", "Default 11.txt", "Default 12.txt" };
            public static string DIRECTORY = "L:\\Kat Han\\Current Cycling\\Kats Data\\Data\\";
            public static int CYCLE_MINUTES = 0;
            public static bool CURRENT_BIAS_STATUS = false;
            public static bool PAUSED_STATUS = false;
            public static bool PAUSED_FANS = false;

            public static int[] numCells = new int[12];
            public static double[] cellVOC = new double[12];
            public static int[] tempSensorPerPort = new int[12];
            //public static TextBox[] tempSensors = new []{ txtTempSensSample1 };
            public static bool[] samplesInPort = new bool[12];
            // public static Label[] samplesList = { lblSample1, lblSample2};
            public static SerialPort serArd;
            public static SerialPort serTDK;

            public static int first_time;
            public static int cycle_number;
            public static int SAVETIME = 3;
            public static int TimeKeeper = 0; 

            public static List<int> portsInUse = new List<int>();

            public static SerialPort temp;
            public static string name;
        }
        

        private void btnChoosePath_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Globals.DIRECTORY = fbd.SelectedPath + "\\";
                    txtDirectory.Text = Globals.DIRECTORY;
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    Console.WriteLine("Directory Set: " + Globals.DIRECTORY);

                    //MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                }
            }
        }

        // Should there be a connect button? Or should it all be done in start
        private void btnConnect_Click(object sender, EventArgs e)
        {
            //Globals.temp = new SerialPort("COM7", Convert.ToInt32(Globals.BAUDRATE));
            //if (Globals.temp.IsOpen)
            //{
            //    Globals.temp.Close();
            //}
            //Globals.temp.Open();
            //Globals.temp.WriteLine("IDN?\r\n");
            //string data = "";
            //StringBuilder sb = new StringBuilder();
            //bool cont = true;
            //while(cont)
            //{
            //    int nugget = Globals.temp.ReadByte();
            //    if(nugget == 13) // || nugget == 0) // 13 -> Carriage Return; 0 -> null
            //    {
            //        cont = false;
            //    } 
            //    sb.Append(Convert.ToChar(nugget));
            //}
            //data = sb.ToString();
            //Console.WriteLine("After");
            //Console.WriteLine(data);
            

            #region Correct Code For Loop
            string[] ports = SerialPort.GetPortNames();
            
            string[] IDN = new string[ports.Length];
            Console.WriteLine("The following serial ports were found:");
            
            Globals.serArd = new SerialPort();
            Globals.serTDK = new SerialPort();
            
            // Display each port name to the console.
            foreach (string port in ports)
            {
                Console.WriteLine(port);
                Globals.temp = new SerialPort("COM7", Convert.ToInt32(Globals.BAUDRATE));
                if(Globals.temp.IsOpen)
                {
                    Globals.temp.Close();
                }
                Console.WriteLine("Attempt to Open");
                Globals.temp.Open();
                Console.WriteLine("Past Opened");
                Thread.Sleep(50);
                Console.WriteLine("Attempt to Write");
                Globals.temp.WriteLine("ADR 1\r\n");
                //Thread.Sleep(1000);
                Globals.temp.WriteLine("IDN?\r\n");
            
                Console.WriteLine("Wrote, wait for respone");
                readData(0);
            
                Thread.Sleep(1000);
            
                if (Globals.name == "ARD")
                {
                    Console.WriteLine("Entered if for setting port name");
                    Globals.serArd.PortName = port;
                } else if(Globals.name == "TDK")
                {
                    Globals.serTDK.PortName = port;;
                }
                Globals.name = "";
                Console.WriteLine("exited");
                Globals.temp.Close();
                //Console.WriteLine(temp.ReadLine());
            }
            
            #endregion

            #region Setup Arduino
            //Console.WriteLine("Port for Arduino: ");
            //Console.WriteLine(Globals.serArd.PortName);
            //
            //Globals.serArd.BaudRate = Convert.ToInt32(Globals.BAUDRATE);
            //Console.WriteLine("Open Arduino");
            //if (Globals.serArd.IsOpen)
            //{
            //    Globals.serArd.Close();
            //}
            //Globals.serArd.Open();
            //Globals.serArd.DataReceived += new SerialDataReceivedEventHandler(ard_DataReceived);
            //Console.WriteLine("Write to Ard");
            //Globals.serArd.WriteLine("IDN?\r\n");
            //
            //Console.WriteLine("Wrote");
            //Thread.Sleep(1000);
            //
            //Globals.serArd.Close();
            #endregion

            //Console.WriteLine("Port for TDK: ");
            //Console.WriteLine(Globals.serTDK.PortName);

            #region Code from python
            //string strArd = Console.ReadLine();
            //string strArd = "COM5";
            //Console.WriteLine("Port for TDK");
            ////string strTDK = Console.ReadLine();
            //string strTDK = "COM7";
            //
            //Console.WriteLine("Attempting to set up ports");
            //Globals.serArd = new SerialPort(strArd, Convert.ToInt32(Globals.BAUDRATE));
            //Globals.serTDK = new SerialPort(strTDK, Convert.ToInt32(Globals.BAUDRATE));
            //Console.WriteLine("Ports initialized");
            //
            //// Set up TDK; Add way to check if port is open/ try statement
            //Console.WriteLine("attempt to connect to TDK");
            //Globals.serTDK.Open();
            //Console.WriteLine("Connected to TDK");
            //string[] IDN = new string[12 * 4];
            //for(int i = 0; i < 1; i++)
            //{
            //    Globals.serTDK.WriteLine("ADR " + (i + 1).ToString());
            //    Globals.serTDK.WriteLine("RST");
            //    Globals.serTDK.WriteLine("IDN?");
            //    IDN[i * 4] = Globals.serTDK.ReadLine();
            //    Console.WriteLine(IDN[i]);
            //    Globals.serTDK.WriteLine("SN?");
            //    IDN[i * 4 + 1] = Globals.serTDK.ReadLine();
            //    Console.WriteLine(IDN[i*4 + 1]);
            //    Globals.serTDK.WriteLine("REV?");
            //    IDN[i * 4 + 2] = Globals.serTDK.ReadLine();
            //    Console.WriteLine(IDN[i * 4 + 2]);
            //    Globals.serTDK.WriteLine("DATE?");
            //    IDN[i * 4 + 3] = Globals.serTDK.ReadLine();
            //    Console.WriteLine(IDN[i * 4 + 3]);
            //
            //    // Change power supply connections to true/false depending on identity
            //    Globals.PS_CONNECTION[i] = IDN[i * 4].Contains("TDK");
            //}
            //
            //// Set up Arduino
            //Globals.serArd.Open();
            //
            //Console.WriteLine("Opened Ard");
            #endregion
        }

        // 0 -> temp; 1 -> Arduino; 2 -> TDK
        private void readData(int n)
        {
            var srPorts = new SerialPort[] { Globals.temp, Globals.serArd, Globals.serTDK };
            string data = "";
            StringBuilder sb = new StringBuilder();
            bool cont = true;
            while (cont)
            {
                int nugget = srPorts[n].ReadByte();
                if (nugget == 13 || nugget == 0) // 13 -> Carriage Return; 0 -> null
                {
                    cont = false;
                }
                sb.Append(Convert.ToChar(nugget));
            }
            data = sb.ToString();
            Console.WriteLine("After");
            Console.WriteLine(data);

            if (data.Substring(0, 3) == "ARD")
            {
                Globals.name = "ARD";
            }
            else if (data.Substring(0,3) == "TDK")
            {
                Globals.name = "TDK";
            }
        }

        // Might be unnecessary
        #region Data Received Methods
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("In DataReceived");
            Thread.Sleep(500);
            byte[] buffer = new byte[32];
            char[] outArray = new char[32];
            string data = "";
            //data = Globals.temp.ReadLine();
            try
            {
                data = Globals.temp.ReadLine();
            }
            catch (System.IO.IOException error)
            {
                return;
            }
            catch (System.InvalidOperationException error)
            {
                return;
            }
            //var data = (Globals.temp.Read(buffer, 0, 32));
            //Console.WriteLine(data.Substring(0, 2));
            if(data.Substring(0,3) == "ARD")
            {
                Globals.name = "ARD";
            } else
            {
                Globals.name = "TDK";
            }
            Console.WriteLine(data);
            Thread.Sleep(500);
        }

        void ard_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("In DataReceived");
            Thread.Sleep(500);
            string data = "";
            //data = Globals.temp.ReadLine();
            try
            {
                data = Globals.serArd.ReadLine();
            }
            catch (System.IO.IOException error)
            {
                return;
            }
            catch (System.InvalidOperationException error)
            {
                return;
            }
            //var data = (Globals.temp.Read(buffer, 0, 32));
            //Console.WriteLine(data.Substring(0, 2));
            if (data.Substring(0, 3) == "ARD")
            {
                Globals.name = "ARD";
            }
            else
            {
                Globals.name = "TDK";
            }
            Console.WriteLine(data);
            Thread.Sleep(500);
        }
        #endregion

        private void btnStart_Click(object sender, EventArgs e)
        {
            Globals.cycle_number = 1;
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            Globals.first_time = (int)t.TotalSeconds;
            var numCellsBoxes = new TextBox[] { txtNumCells1, txtNumCells2, txtNumCells3, txtNumCells4, txtNumCells5, txtNumCells6, txtNumCells7, txtNumCells8, txtNumCells9, txtNumCells10, txtNumCells11, txtNumCells12 };
            var cellVOCs = new TextBox[] { txtVoc1, txtVoc2, txtVoc3, txtVoc4, txtVoc5, txtVoc6, txtVoc7, txtVoc8, txtVoc9, txtVoc10, txtVoc11, txtVoc12 };
            var TempSensors = new TextBox[] { txtTempSensSample1, txtTempSensSample2, txtTempSensSample3, txtTempSensSample4, txtTempSensSample5, txtTempSensSample6, txtTempSensSample7, txtTempSensSample8, txtTempSensSample9, txtTempSensSample10, txtTempSensSample11, txtTempSensSample12 };
            var chkBoxes = new CheckBox[] { chkbxPort1, chkbxPort2, chkbxPort3, chkbxPort4, chkbxPort5, chkbxPort6, chkbxPort7, chkbxPort8, chkbxPort9, chkbxPort10, chkbxPort11, chkbxPort12 };
            

            for (int i = 0; i < 12; i++)
            {
                Globals.numCells[i] = Convert.ToInt32(numCellsBoxes[i].Text);
                Globals.cellVOC[i] = Convert.ToDouble(cellVOCs[i].Text);
                Globals.tempSensorPerPort[i] = Convert.ToInt32(TempSensors[i].Text);
                Globals.samplesInPort[i] = chkBoxes[i].Checked;
            }

            for(int i = 0; i < Globals.samplesInPort.Length; i++)
            {
                if (Globals.samplesInPort[i])
                {
                    Globals.portsInUse.Add(i + 1);
                }
                // Console.WriteLine(Globals.tempSensorPerPort[i].ToString());
                // Console.WriteLine(Globals.numCells[i].ToString());
                // Console.WriteLine(Globals.cellVOC[i].ToString());

            }
            Console.WriteLine("Sample Ports in use:");
            Globals.portsInUse.ForEach(i => Console.Write("{0}\t", i));
            Console.WriteLine("");
            ArdTimerMethod();
            
        }

        private static void WriteToFile(string message, string OutputFile, string first_time_seconds)
        {
            string path = @"" + OutputFile + first_time_seconds + ".txt";
            using (FileStream fs = File.OpenWrite(path))
            {
                Byte[] info = Encoding.ASCII.GetBytes(message);
                fs.Write(info, 0, info.Length);
            }
            // FileStream fileStream = new FileStream(@"" + OutputFile + first_time_seconds + ".txt", FileMode.Open);
            // fileStream.Write(message);
            // fileStream.Close();
        }
        

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            Globals.TimeKeeper++;
            if(Globals.TimeKeeper <= 60* Convert.ToInt32(txtBiasOn.Text))
            {
                Globals.CURRENT_BIAS_STATUS = true;
                ArdTimerMethod();
            }
            else
            {
                Globals.CURRENT_BIAS_STATUS = false;

            }

            if(Globals.TimeKeeper > 60* (Convert.ToInt32(txtBiasOn.Text) + Convert.ToInt32(txtBiasOff.Text)))
            {
                Globals.TimeKeeper = 0;
            }
        }

            // Call from C# timer
        private void ArdTimerMethod()
        {
            // Write to Arduino
            Console.WriteLine("Trying to write to Arduino");
            Globals.serArd.WriteLine("WRITE_TO_ARDUINO");
            // sleep if needed time.sleep(SERIAL WAIT TIME)
            Console.WriteLine("intBiasCurrentState = " + Globals.CURRENT_BIAS_STATUS.ToString());

            int intBiasCurrentState = 0;
            if (Globals.CURRENT_BIAS_STATUS) intBiasCurrentState = 1;
            Globals.serArd.WriteLine(intBiasCurrentState.ToString() + "," + txtCurrOnTempSet.Text + "," + 
                        txtCurrOffTempSet.Text + "," + txtOverTempSet.Text + "," + "\r\n");
            // sleep
            Console.WriteLine("Wrote to Arduino");

            if (Globals.PAUSED_FANS) Globals.serArd.WriteLine("FAN_PAUSE");
            else Globals.serArd.WriteLine("FAN_RESUME");

            // Read from Arduino
            Globals.serArd.WriteLine("READ_TO_PC");
            // sleep
            string ardData = Globals.serArd.ReadLine();
            // sleep
            Console.WriteLine("Printing Arduino Output: ");
            Console.WriteLine(ardData);

            string[] ardDataArr = ardData.Split(',');
            Console.WriteLine(ardDataArr.Length);

            try
            {
                for (int i = 0; i < Globals.CH_TEMPS.Length; i++)
                {
                    Globals.CH_TEMPS[i] = Convert.ToInt32(ardDataArr[i]);
                }
                for (int i = 16; i < Globals.FAN_SPEEDS.Length; i++)
                {
                    Globals.FAN_SPEEDS[i - 16] = Convert.ToInt32(ardDataArr[i]);
                }

                string curr_status = ardDataArr[24];
                Console.WriteLine("Check Arduino Current Bias Status: " + curr_status);
                bool status = false;
                if (curr_status == "on") status = true; //check if this is correct, is "on" expected
                Globals.CURRENT_BIAS_STATUS = status;

                Console.WriteLine("Arduino Smoke sensor Lockout Status: " + ardDataArr[25]);
            } catch
            {
                for (int i = 0; i < Globals.CH_TEMPS.Length; i++)
                {
                    Globals.CH_TEMPS[i] = -1;
                }
                for (int i = 16; i < Globals.FAN_SPEEDS.Length; i++)
                {
                    Globals.FAN_SPEEDS[i - 16] = -1;
                }
                Globals.CURRENT_BIAS_STATUS = false;
            }
        }

        // Call this method from C# Timer
        private void TDKTimerMethod()
        {
            int ON_TIME = Convert.ToInt32(txtBiasOn.Text);
            int OFF_TIME = Convert.ToInt32(txtBiasOff.Text);
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int time_now = (int)t.TotalSeconds;

            if((time_now - Globals.first_time) / 60 >= (Globals.cycle_number - 1) * (ON_TIME + OFF_TIME) + ON_TIME
                && time_now - Globals.first_time / 60 < (Globals.cycle_number) * (ON_TIME + OFF_TIME))
            {
                Globals.CURRENT_BIAS_STATUS = false;
            }
            else if ((time_now - Globals.first_time) / 60 >= (Globals.cycle_number) * (ON_TIME + OFF_TIME))
            {
                for (int i = 0; i < Globals.CYCLE_NUMBERS.Length; i++)
                {
                    Globals.CYCLE_NUMBERS[i]++;
                    Globals.cycle_number++;
                    Globals.CURRENT_BIAS_STATUS = true;
                }
            }
            else
            {
                Console.WriteLine("Timer does not make sense");
                Console.WriteLine("Current Bias Status = " + Convert.ToString(Globals.CURRENT_BIAS_STATUS));
            }

            Globals.CYCLE_MINUTES = (time_now - Globals.first_time) / 60 - (Globals.cycle_number - 1) *
                (ON_TIME + OFF_TIME);
            SendPowerSupplyCommand();
        }

        private void SendPowerSupplyCommand()
        {
            int[] current_settings = new int[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; ;
            //if (Globals.CURRENT_BIAS_STATUS)
            //{
            //    current_settings = { Convert.ToInt32(txtSetCurr1.Text), Convert.ToInt32(txtSetCurr2.Text),
            //             Convert.ToInt32(txtSetCurr3.Text), Convert.ToInt32(txtSetCurr4.Text),
            //             Convert.ToInt32(txtSetCurr5.Text), Convert.ToInt32(txtSetCurr6.Text),
            //             Convert.ToInt32(txtSetCurr7.Text), Convert.ToInt32(txtSetCurr8.Text),
            //             Convert.ToInt32(txtSetCurr9.Text), Convert.ToInt32(txtSetCurr10.Text),
            //             Convert.ToInt32(txtSetCurr11.Text), Convert.ToInt32(txtSetCurr12.Text)};
            //}
            //
            //else
            //{
            //    current_settings = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            //}
                
            int[,] TDK_DATA = new int[12, 6];

            for (int i = 0; i < 12; i++)
            {

                Globals.serTDK.WriteLine("ADR " + Convert.ToString(i + 1) + "\r\n");
                // Sleep if need time.sleep(SERIAL_WAIT_TIME)
                string ADR_return = Globals.serTDK.ReadLine();
                Console.WriteLine(ADR_return);
                // time.sleep(SERIAL_WAIT_TIME)
                Globals.serTDK.WriteLine("IDN?\r\n");
                // time.sleep(SERIAL_WAIT_TIME)

                string IDN_RETURNED = Globals.serTDK.ReadLine();
                // time.sleep(SERIAL_WAIT_TIME)
                Console.WriteLine(IDN_RETURNED); 
                // time.sleep(SERIAL_WAIT_TIME)
                if (IDN_RETURNED.Substring(0,2) == "TDK")
                {
                    // Set interlock
                    if (Globals.INTERLOCK)
                    {
                        Globals.serTDK.WriteLine("RIE ON\r\n");
                        // time.sleep(SERIAL_WAIT_TIME)
                    }
                    else
                    {
                        Globals.serTDK.WriteLine("RIE OFF\r\n");
                        // time.sleep(SERIAL_WAIT_TIME)
                    }
                    Globals.serTDK.WriteLine("RIE?\r\n");
                    // time.sleep(SERIAL_WAIT_TIME)
                    Console.WriteLine("remote interlock on?");
                    string remote_interlock = Globals.serTDK.ReadLine();
                    Console.WriteLine(remote_interlock);
                    //time.sleep(SERIAL_WAIT_TIME)
                    // Set compliance voltage
                    Globals.serTDK.WriteLine("PV " + Convert.ToString(Globals.VOLTAGE_COMPLIANCE) + "\r\n");
                    //time.sleep(0.2)
                    //time.sleep(SERIAL_WAIT_TIME)

                    // Set current
                    Globals.serTDK.WriteLine("PC " + Convert.ToString(current_settings[i]) + "\r\n");
                    //time.sleep(0.2)
                    //time.sleep(SERIAL_WAIT_TIME)

                    // Turn on output
                    if (Globals.CURRENT_BIAS_STATUS)
                    {
                        Globals.serTDK.WriteLine("OUT ON\r\n");
                        Globals.CURRENT_BIAS_STATUS = true;
                        //time.sleep(0.2)
                    }
                    else
                    {
                        Globals.serTDK.WriteLine("OUT OFF\r\n");
                        Globals.CURRENT_BIAS_STATUS = false;
                        //time.sleep(0.2)
                    }
                    string a = Globals.serTDK.ReadLine();
                    //time.sleep(SERIAL_WAIT_TIME)
                    // Read TDK Data
                    Globals.serTDK.WriteLine("DVC?\r\n");
                    //time.sleep(SERIAL_WAIT_TIME)
                    //                if (VERBOSE == 1): print "DVC?\r\n"
                    string TDK_OUTPUT = Globals.serTDK.ReadLine();
                    //time.sleep(0.2)
                    //                if (VERBOSE == 1): print TDK_OUTPUT
                    string[] TDK_OUTPUT_LIST = TDK_OUTPUT.Split(',');
                    //                if (VERBOSE == 1): print "TDK_OUTPUT_LIST"
                    Console.WriteLine(TDK_OUTPUT_LIST);
                    for (int j = 0; j < 6; j++)
                    {
                        TDK_DATA[i, j] = Convert.ToInt32(TDK_OUTPUT_LIST[j]);
                    }
                
                }
            }


            // time.sleep(0.01)
            for (int i = 0; i < Globals.PS_CONNECTION.Length; i++)
            {
                Globals.PS_CURRENTS[i] = TDK_DATA[i, 2];
                Globals.PS_VOLTAGES[i] = TDK_DATA[i, 0];
            }

            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int time_now = (int)t.TotalSeconds;
            double hours_now = (double)t.TotalHours;
            Console.WriteLine("hours ran" + hours_now);
            //        print "dict to gui temp 15"
            //        print dict_to_GUI["Temperatures"][15]

            //for (int i = 0; i )
            //for i in range(len(dict_to_GUI["PowerSupplies"])):
            //    dict_to_GUI["PowerSupplies"][i]["Current"] = str(TDK_DATA[i, 2])
            //    dict_to_GUI["PowerSupplies"][i]["Voltage"] = str(TDK_DATA[i, 0])

            // calculate estimated Rs

            double[] RoomTempVoc = new double[12];
            double[] TempDifference = new double[12];
            double[] VoltageTempAdjustment = new double[12];
            double[] VoltageFromRs = new double[12];
            double[] CalculatedVoc = new double[12];
            double[] AdjustedVoltage = new double[12];
            double[] Rs_Estimates = new double[12];
            for (int i = 0; i < Globals.PS_CONNECTION.Length; i++)
            {
                if (Globals.PS_CONNECTION[i])
                {
                    RoomTempVoc[i] = Convert.ToDouble(Globals.cellVOC[i]) * Convert.ToDouble(Globals.numCells[i]);
                    TempDifference[i] = Convert.ToDouble(Globals.CH_TEMPS[i]) - 23.0;
                    VoltageTempAdjustment[i] = Convert.ToDouble(Globals.numCells[i]) + 0.002 * TempDifference[i];
                    VoltageFromRs[i] = Convert.ToDouble(Globals.PS_VOLTAGES[i]) - RoomTempVoc[i] + VoltageTempAdjustment[i];
                    Rs_Estimates[i] = VoltageFromRs[i] / Convert.ToDouble(Globals.PS_CURRENTS[i]) / Convert.ToDouble(Globals.numCells[i]) * 38.5; //should 38.5 be numerator or denom
                }
            }
            if(Globals.first_time % Globals.SAVETIME == 0)
            {
                // Graph counter variable may be needed
                Globals.GRAPH_COUNTER++;

                for (int i = 0; i < Globals.portsInUse.Count; i++)
                {

                    string pathname = @"" + Globals.DIRECTORY + Globals.FILENAMES[Globals.portsInUse[i] - 1]; //I think this should be -1 on the port (portinuse will be the 2nd port but index is 1)
                    using (FileStream fs = File.OpenWrite(pathname))
                    {
                        int port = Globals.portsInUse[i];
                        string message = Convert.ToString(Globals.CYCLE_NUMBERS[port]) + "," + Convert.ToString(time_now) + "," + Convert.ToString(Globals.CYCLE_MINUTES)
                            + "," + Convert.ToString(Globals.CURRENT_BIAS_STATUS) + "," + Convert.ToString(Globals.FILENAMES[port]) + "," + Convert.ToString(TDK_DATA[port, 2]) 
                            + "," + Convert.ToString(TDK_DATA[port, 0]) + "," + Convert.ToString(Globals.numCells[port]) + "," + Convert.ToString(Globals.cellVOC[port]) 
                            + "," + Convert.ToString(Rs_Estimates[port]) + "," + 
                            Convert.ToString(Globals.CH_TEMPS[0]) + "," + Convert.ToString(Globals.CH_TEMPS[1]) + "," + Convert.ToString(Globals.CH_TEMPS[2]) + "," + Convert.ToString(Globals.CH_TEMPS[3]) + "," +
                            Convert.ToString(Globals.CH_TEMPS[4]) + "," + Convert.ToString(Globals.CH_TEMPS[5]) + "," + Convert.ToString(Globals.CH_TEMPS[6]) + "," + Convert.ToString(Globals.CH_TEMPS[7]) + "," +
                            Convert.ToString(Globals.CH_TEMPS[8]) + "," + Convert.ToString(Globals.CH_TEMPS[9]) + "," + Convert.ToString(Globals.CH_TEMPS[10]) + "," + Convert.ToString(Globals.CH_TEMPS[11]) + "," +
                            Convert.ToString(Globals.CH_TEMPS[12]) + "," + Convert.ToString(Globals.CH_TEMPS[13]) + "," + Convert.ToString(Globals.CH_TEMPS[14]) + "," + Convert.ToString(Globals.CH_TEMPS[15]) + "," + "\n";

                        Byte[] info = Encoding.ASCII.GetBytes(message);
                        fs.Write(info, 0, info.Length);
                    }
                    //if(Globals.GRAPH_COUNTER > Globals.GRAPH_FREQUENCY)
                    //{
                    //    string pathnamefile = Convert.ToString()
                    //}

            //using (FileStream fs = File.OpenWrite(path))
            //{
            //    Byte[] info = Encoding.ASCII.GetBytes(message);
            //    fs.Write(info, 0, info.Length);
            //}
                }
            }

            }

            // May not need if we just stop and start everytime
        private void btnPauseCycles_Click(object sender, EventArgs e)
        {
            Globals.PAUSED_STATUS = true;
        }

        private void btnPauseCyclesFans_Click(object sender, EventArgs e)
        {
            Globals.PAUSED_STATUS = true;
            Globals.PAUSED_FANS = true;
        }

        // May not need if we just stop and start everytime
        private void btnResume_Click(object sender, EventArgs e)
        {
            Globals.PAUSED_STATUS = false;
            Globals.PAUSED_FANS = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Globals.CURRENT_BIAS_STATUS = false;
            // Globals.ARDUINO_TIMER = false;
            // Globals.TDK_TIMER = false;
            SerialPort serArd = new SerialPort("COM4", Convert.ToInt32(Globals.BAUDRATE));
            serArd.Open();
            for(int i = 0; i < Globals.samplesInPort.Length; i++)
            {
                SerialPort serTDK = new SerialPort("", Convert.ToInt32(Globals.BAUDRATE));
                serTDK.Open();
                serTDK.Write("ADR " + i.ToString() + "\r\n");
                // time.sleep(0.01);
                serTDK.Write("OUT OFF\r\n");
                // time.sleep(0.01);
                serTDK.Close();
            }

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            // if (Globals.TDK_TIMER) { btnStop_Click(sender, e); }
            // End timer thread
            this.Close();
        }

        // May Not need following events if start method just checks for all
        #region Checkboxes
        private void chkbxPort1_CheckedChanged(object sender, EventArgs e)
        {
            //if(chkbxPort1.Checked) { Globals.}
        }

        private void chkbxPort2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort7_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort8_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort9_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort10_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort11_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkbxPort12_CheckedChanged(object sender, EventArgs e)
        {

        }
        #endregion

        private void btnSamplesList_Click(object sender, EventArgs e)
        {
            // Globals.DIRECTORY = txtDirectory.Text;
            var Labels = new Label[] { lblSample1, lblSample2, lblSample3, lblSample4, lblSample5, lblSample6, lblSample7, lblSample8, lblSample9, lblSample10, lblSample11, lblSample12 };
            var checkboxes = new CheckBox[] { chkbxPort1, chkbxPort2, chkbxPort3, chkbxPort4, chkbxPort5, chkbxPort6, chkbxPort7, chkbxPort8, chkbxPort9, chkbxPort10, chkbxPort11, chkbxPort12 };
            var numCellsBoxes = new TextBox[] { txtNumCells1, txtNumCells2, txtNumCells3, txtNumCells4, txtNumCells5, txtNumCells6, txtNumCells7, txtNumCells8, txtNumCells9, txtNumCells10, txtNumCells11, txtNumCells12 };
            var cellVOCs = new TextBox[] { txtVoc1, txtVoc2, txtVoc3, txtVoc4, txtVoc5, txtVoc6, txtVoc7, txtVoc8, txtVoc9, txtVoc10, txtVoc11, txtVoc12 };
            var TempSensors = new TextBox[] { txtTempSensSample1, txtTempSensSample2, txtTempSensSample3, txtTempSensSample4, txtTempSensSample5, txtTempSensSample6, txtTempSensSample7, txtTempSensSample8, txtTempSensSample9, txtTempSensSample10, txtTempSensSample11, txtTempSensSample12 };
            var setCurr = new TextBox[] { txtSetCurr1, txtSetCurr2, txtSetCurr3, txtSetCurr4, txtSetCurr5, txtSetCurr6, txtSetCurr7, txtSetCurr8, txtSetCurr9, txtSetCurr10, txtSetCurr11, txtSetCurr12 };
            var cycNums = new Label[] {lblCycle1, lblCycle2, lblCycle3, lblCycle4, lblCycle5, lblCycle6, lblCycle7, lblCycle8, lblCycle9, lblCycle10, lblCycle11, lblCycle12 };

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                
                int i = 0;
                using (sr)
                    while (!sr.EndOfStream && i < 12)
                    {
                        string s = sr.ReadLine();
                        string[] settings = s.Split(',');
                        if (Convert.ToInt32(settings[0]) == 1)
                        {
                            string path = Globals.DIRECTORY + settings[1] + ".txt";
                            Console.WriteLine("Path: " + path);
                            Labels[i].Text = settings[1];
                            checkboxes[i].Checked = true;
                            numCellsBoxes[i].Text = settings[2];
                            cellVOCs[i].Text = settings[3];
                            TempSensors[i].Text = settings[4];
                            setCurr[i].Text = settings[5];
                            if (File.Exists(path))
                            {
                                Globals.FILENAMES[i] = settings[1] + ".txt";
                                Console.WriteLine("Label " + (i + 1).ToString() + " in use. Previous File; Name: " + Globals.FILENAMES[i]);
                                if(new FileInfo(path).Length != 0)
                                {
                                    string last = File.ReadLines(path).Last();
                                    string[] cycleinfo = last.Split(',');
                                    cycNums[i].Text = cycleinfo[0];
                                } else
                                {
                                    cycNums[i].Text = "0";
                                }
                            } else
                            {
                                File.Create(path);

                                Globals.FILENAMES[i] = settings[1] + ".txt";
                                Console.WriteLine("Label " + (i + 1).ToString() + " in use. Created new File; Name: " + Globals.FILENAMES[i]);
                            }
                        } else
                        {
                            Labels[i].Text = "null";
                            numCellsBoxes[i].Text = "null";
                            cellVOCs[i].Text = "null";
                            TempSensors[i].Text = "null";
                            setCurr[i].Text = "null";
                        }
                        i++;
                    }
                sr.Close();
            }
        }

        private void CurrCyclCtrls_Load(object sender, EventArgs e)
        {

        }

        private void btnLoad1_Click(object sender, EventArgs e)
        {
        
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    int ind = Globals.DIRECTORY.Length;
                    lblSample1.Text = fbd.SelectedPath.Substring(ind);
                    Console.WriteLine("Sample 1: " + lblSample1.Text);
                    Globals.FILENAMES[1] = lblSample1.Text;

                    //MessageBox.Show("Files found: " + files.Length.ToString(), "Message");
                }
            }
        
        }

        private void btnNew1_Click(object sender, EventArgs e)
        {

        }
    }
}

        