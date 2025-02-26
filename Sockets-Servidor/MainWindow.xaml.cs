using System.Net.Sockets;
using System.Net;
using System.Text;
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
using System.Net.NetworkInformation;
using Sockets_Servidor.credenciales;
using Newtonsoft.Json;

namespace Sockets_Servidor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener server;
        private bool serverRunning = false;

        // Diccionario para almacenar clientes activos (IP -> {Departamento, MAC})
        private Dictionary<string, (string Departamento, string MAC)> clientesActivos = new Dictionary<string, (string, string)>();

        // Listas de mensajes por departamento
        private Dictionary<string, List<string>> mensajesDepartamentos = new Dictionary<string, List<string>>
        {
            { "Cochabamba", new List<string>() },
            { "Santa Cruz", new List<string>() },
            { "La Paz", new List<string>() },
            { "Oruro", new List<string>() },
            { "Potosí", new List<string>() },
            { "Tarija", new List<string>() },
            { "Chuquisaca", new List<string>() },
            { "Beni", new List<string>() },
            { "Pando", new List<string>() }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();
                serverRunning = true;

                Dispatcher.Invoke(() => txtPuerto.Text = ("Servidor escuchando en el puerto 5000..."));

                while (serverRunning)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => txtPuerto.Text=($"Error: {ex.Message}"));
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            string clientMAC = ObtenerMacAddress(clientIP);
            string departamento = null;

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                // Leer el primer mensaje (Departamento)
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                departamento = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim().Replace("Departamento: ", "");

                if (mensajesDepartamentos.ContainsKey(departamento))
                {
                    clientesActivos[clientIP] = (departamento, clientMAC);
                    //Dispatcher.Invoke(() => lstMensajes.Items.Add($"Nuevo cliente registrado: {clientIP} - {departamento} - {clientMAC}"));
                }
                else
                {
                    byte[] responseData = Encoding.UTF8.GetBytes("Departamento no válido.");
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    client.Close();
                    return;
                }

                // Leer mensajes periódicos
                while (client.Connected)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    //Dispatcher.Invoke(() => lstMensajes.Items.Add($"[{departamento}] {clientIP}: {receivedData}"));

                    // Guardar mensaje en la lista del departamento correspondiente
                    if (mensajesDepartamentos.ContainsKey(departamento))
                    {
                        mensajesDepartamentos[departamento].Add(receivedData);
                        ActualizarListaDepartamento(departamento, receivedData);
                    }
                }
            }
            catch (IOException)
            {
                //Dispatcher.Invoke(() => lstMensajes.Items.Add($"Cliente desconectado: {clientIP} ({departamento})"));
            }
            catch (Exception ex)
            {
                //Dispatcher.Invoke(() => lstMensajes.Items.Add($"Error con cliente {clientIP}: {ex.Message}"));
            }
            finally
            {
                client.Close();
                if (departamento != null && clientesActivos.ContainsKey(clientIP))
                {
                    clientesActivos.Remove(clientIP);
                }
            }
        }

        private string ObtenerMacAddress(string ipAddress)
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.ToString() == ipAddress)
                        {
                            return nic.GetPhysicalAddress().ToString();
                        }
                    }
                }
            }
            catch (Exception) { }
            return "Desconocido";
        }

        private void ActualizarListaDepartamento(string departamento, string mensaje)
        {
            //el mensaje viene en json string estado = $"{{\"usado\": {espacioUsado / (1024 * 1024)}, \"libre\": {espacioLibre / (1024 * 1024)}}}";
            //descerializar el mensaje
            var mensajeDeserializado = JsonConvert.DeserializeObject<dynamic>(mensaje);
            //convertir a GB
            //double espacioUsado = double.Parse(int.Parse(mensajeDeserializado.usado)) / (1024 * 1024);
            //double espacioLibre = double.Parse(int.Parse(mensajeDeserializado.libre)) / (1024 * 1024);
            //double espacioTotal = espacioUsado + espacioLibre;
            Dispatcher.Invoke(() =>
            {
                switch (departamento)
                {
                    case "Cochabamba": txtCochaTotalGb.Text = (mensajeDeserializado.usado + mensajeDeserializado.libre) + "GB"; txtCochaUsoGb.Text = mensajeDeserializado.usado + "GB"; txtCochaLibreGb.Text = mensajeDeserializado.libre + "GB"; break;
                        //case "Cochabamba": lstCochabamba.Items.Add(mensaje); break;
                        //case "Santa Cruz": lstSantacruz.Items.Add(mensaje); break;
                        //case "La Paz": lstLapaz.Items.Add(mensaje); break;
                        //case "Oruro": lstOruro.Items.Add(mensaje); break;
                        //case "Potosí": lstPotosi.Items.Add(mensaje); break;
                        //case "Tarija": lstTarija.Items.Add(mensaje); break;
                        //case "Chuquisaca": lstChuquisaca.Items.Add(mensaje); break;
                        //case "Beni": lstBeni.Items.Add(mensaje); break;
                        //case "Pando": lstPando.Items.Add(mensaje); break;
                }
            });
        }

        private void CerrarApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(StartServer);
            txtEstado.Text = "Servidor iniciado...";
        }

        private void BtnDetener_Click(object sender, RoutedEventArgs e)
        {
            serverRunning = false;
            server?.Stop();
            txtEstado.Text = "Servidor detenido.";
        }

        private void btn_tempo_Click(object sender, RoutedEventArgs e)
        {
            // navegar a la ventana Bdd
            Bdd ventana = new Bdd();
            ventana.Show();
            this.Close();
        }
    }
}