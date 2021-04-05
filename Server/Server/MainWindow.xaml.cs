using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;
using System.Management;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;

namespace Server
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip = IPAddress.Parse(txtHost.Text);
            var serverCertificate = getServerCert();
            var listener = new TcpListener(ip, int.Parse(txtPort.Text));
            listener.Start();
            this.Dispatcher.Invoke(() =>
            {
                txtStatus.Text += "Servidor Iniciado con exito. \n";
            });
            MessageBox.Show("Certificado Encontrado");
            while (true)
            {
                using (var client = listener.AcceptTcpClient())
                using (var sslStream = new SslStream(client.GetStream(),
                   false, ValidateCertificate))
                {
                    sslStream.AuthenticateAsServer(serverCertificate,
                       true, SslProtocols.Tls12, false);
                    //MessageBox.Show("Certificado encontrado");
                    var inputBuffer = new byte[4096];
                    var inputBytes = 0;
                    while (inputBytes == 0)
                    {
                        inputBytes = sslStream.Read(inputBuffer, 0,
                           inputBuffer.Length);
                        //MessageBox.Show("Esperando...");
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        var inputMessage = Encoding.UTF8.GetString(inputBuffer,
                       0, inputBytes);
                        MessageBox.Show("Mensaje Recibido");
                        if(inputMessage=="Cliente 0")
                        {
                            //mandar especificaciones del sistema
                            All all = new All()
                            {
                                GPUs = new List<GPU>(),
                                Storages = new List<Storage>()
                            };
                            ManagementObjectSearcher myVideoObject = new ManagementObjectSearcher("select * from Win32_VideoController");
                            foreach (ManagementObject obj in myVideoObject.Get())
                            {
                                all.GPUs.Add(new GPU()
                                {
                                    Name = obj["Name"].ToString(),
                                    Status = obj["Status"].ToString(),
                                    AdapterRAM = obj["AdapterRAM"].ToString(),
                                    AdapterDACType = obj["AdapterDACType"].ToString(),
                                    DriverVersion = obj["DriverVersion"].ToString()
                                });
                            }
                            DriveInfo[] allDrives = DriveInfo.GetDrives();
                            foreach (DriveInfo d in allDrives)
                            {
                                if (d.IsReady == true)
                                {
                                    all.Storages.Add(new Storage()
                                    {
                                        TotalAvailableSpace = d.TotalFreeSpace,
                                        TotalSizeOfDrive = d.TotalSize,
                                        RootDirectory = d.RootDirectory.Name
                                    });
                                }
                            }
                            PerformanceCounter ram = new PerformanceCounter();
                            ComputerInfo infoDevice = new ComputerInfo();
                            ram.CategoryName = "Memory";
                            ram.CounterName = "Available Bytes";
                            all.MemoryRam = new MemoryRam()
                            {
                                TotalPhysicalMemory = infoDevice.TotalPhysicalMemory,
                                TotalFreeSpace = ram.NextValue()
                            };
                            string result = JsonConvert.SerializeObject(all);
                            byte[] data = Encoding.ASCII.GetBytes(result);
                            //current.Send(data);
                            Console.WriteLine("Info sent to client");
                            //fin codigo lenny
                            //var outputMessage = data;
                            //var outputBuffer = Encoding.UTF8.GetBytes(outputMessage); 
                            var outputBuffer = data;
                            sslStream.Write(outputBuffer);
                            //fin mandar especificaciones
                        }
                        txtStatus.Text += string.Format("Recibido: {0} \n", inputMessage);
                        Console.WriteLine("Conectado con: {0}", inputMessage);
                    });
                }
            }

        }
        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //return true;
            if (sslPolicyErrors == SslPolicyErrors.None)
            { return true; }
            if (sslPolicyErrors ==
                  SslPolicyErrors.RemoteCertificateChainErrors)
            { return true; }
            return false;
        }

        private static X509Certificate getServerCert()
        {
            X509Store store = new X509Store(StoreName.My,
               StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2 foundCertificate = null;
            foreach (X509Certificate2 currentCertificate
               in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name
                   != null && currentCertificate.IssuerName.
                   Name.Equals("CN=MySslSocketCertificate"))
                {
                    foundCertificate = currentCertificate;
                    break;
                }
            }


            return foundCertificate;
        }
    }

}
