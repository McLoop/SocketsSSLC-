using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using Newtonsoft.Json;

namespace Cliente
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string ServerCertificateName =
           "MySslSocketCertificate";
        TcpClient client;
        X509CertificateCollection clientCertificateCollection;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            var clientCertificate = getServerCert();
            clientCertificateCollection = new
               X509CertificateCollection(new X509Certificate[]
               { clientCertificate });
            //IPAddress ip = IPAddress.Parse(txtHost.Text);
            client = new TcpClient(txtHost.Text, int.Parse(txtPort.Text));
            using (var sslStream = new SslStream(client.GetStream(),
               false, ValidateCertificate))
            {
                sslStream.AuthenticateAsClient(ServerCertificateName,
                   clientCertificateCollection, SslProtocols.Tls12, false);

                var outputMessage = "Cliente 0";
                var outputBuffer = Encoding.UTF8.GetBytes(outputMessage);
                sslStream.Write(outputBuffer);
                Console.WriteLine("Sent: {0}", outputMessage);
                var inputBuffer = new byte[4096];
                var inputBytes = 0;
                while (inputBytes == 0)
                {
                    inputBytes = sslStream.Read(inputBuffer, 0,
                       inputBuffer.Length);
                    //MessageBox.Show("Esperando...");
                }
                var inputMessage = Encoding.UTF8.GetString(inputBuffer,
                       0, inputBytes);
                //transformar recibido a json
                string json = "";
                All all = JsonConvert.DeserializeObject<All>(inputMessage);
                json = JsonConvert.SerializeObject(all, Formatting.Indented);
                //llevar al txt
                //txtStatus.Text += string.Format("Recibido del server: {0} \n", inputMessage);
                txtStatus.Text =  json;
            }
        }
        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            { return true; }
            // ignore chain errors as where self signed
            if (sslPolicyErrors ==
               SslPolicyErrors.RemoteCertificateChainErrors)
            { return true; }
            return false;
        }

        private static X509Certificate getServerCert()
        {
            try
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
            catch(Exception ex)
            {
                MessageBox.Show("No hay certificado"+ex.Message);
                return null;
            }

        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
                
        }
    }
}
