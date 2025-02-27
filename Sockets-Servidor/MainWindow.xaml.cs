﻿using System.Net.Sockets;
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
using Sockets_Servidor.histograma_grafico;
using System.Globalization;
using System.Collections.ObjectModel;

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

        private class DepartmentControls
        {
            public TextBlock? Nombre { get; set; }
            public TextBlock? TotalGb { get; set; }
            public TextBlock? UsoGb { get; set; }
            public TextBlock? LibreGb { get; set; }
            public UIElement? PieChart1 { get; set; }
            public UIElement? PieChart2 { get; set; }
        }

        // Diccionario para relacionar nombre de departamento con sus controles
        private Dictionary<string, DepartmentControls> departmentControls;

        // Asegúrate de que los nombres y las claves sean consistentes con el XAML.
        // Por ejemplo, usamos "SantaCruz" en vez de "Santa Cruz".
        private Dictionary<string, List<string>> mensajesDepartamentos = new Dictionary<string, List<string>>
        {
            { "Cochabamba", new List<string>() },
            { "Beni", new List<string>() },
            { "SantaCruz", new List<string>() },
            { "Pando", new List<string>() },
            { "Chuquisaca", new List<string>() },
            { "LaPaz", new List<string>() },
            { "Oruro", new List<string>() },
            { "Potosi", new List<string>() },
            { "Tarija", new List<string>() }
        };


        public MainWindow()
        {
            InitializeComponent();

            // Estado global inicial
            ServidoresArribatxt.Text = "Reportando 0 de 9";

            // Inicializamos el diccionario de controles para cada departamento
            departmentControls = new Dictionary<string, DepartmentControls>
    {
        { "Cochabamba", new DepartmentControls {
                Nombre = txtCochabambaNombre,
                TotalGb = txtCochaTotalGb,
                UsoGb = txtCochaUsoGb,
                LibreGb = txtCochaLibreGb,
                PieChart1 = pieChart1,
                PieChart2 = pieChart2
            }
        },
        { "Beni", new DepartmentControls {
                Nombre = txtBeniNombre,
                TotalGb = txtBeniTotalGb,
                UsoGb = txtBeniUsoGb,
                LibreGb = txtBeniLibreGb,
                PieChart1 = null,  // Si no tienes referencia, puedes omitirlo
                PieChart2 = null
            }
        },
        { "SantaCruz", new DepartmentControls {
                Nombre = txtSantaCruzNombre,
                TotalGb = txtSantaCruzTotalGb,
                UsoGb = txtSantaCruzUsoGb,
                LibreGb = txtSantaCruzLibreGb,
                PieChart1 = null,
                PieChart2 = null
            }
        },
        { "Pando", new DepartmentControls {
                Nombre = txtPandoNombre,
                TotalGb = txtPandoTotalGb,
                UsoGb = txtPandoUsoGb,
                LibreGb = txtPandoLibreGb,
                PieChart1 = null,
                PieChart2 = null
            }
        },
        { "Chuquisaca", new DepartmentControls {
                Nombre = txtChuquisacaNombre,
                TotalGb = txtChuquisacaTotalGb,
                UsoGb = txtChuquisacaUsoGb,
                LibreGb = txtChuquisacaLibreGb,
                PieChart1 = null,
                PieChart2 = null
            }
        },
        { "LaPaz", new DepartmentControls {
                Nombre = txtLaPazNombre,
                TotalGb = txtLaPazTotalGb,
                UsoGb = txtLaPazUsoGb,
                LibreGb = txtLaPazLibreGb,
                PieChart1 = null,
                PieChart2 = null
            }
        },
        { "Oruro", new DepartmentControls {
                Nombre = txtOruroNombre,
                TotalGb = txtOruroTotalGb,
                UsoGb = txtOruroUsoGb,
                LibreGb = txtOruroLibreGb,
                PieChart1 = null,
                PieChart2 = null
            }
        },
        { "Potosi", new DepartmentControls {
                Nombre = txtPotosiNombre,
                TotalGb = txtPotosiTotalGb,
                UsoGb = txtPotosiUsoGb,
                LibreGb = txtPotosiLibreGb,
                PieChart1 = null,
                PieChart2 = null
            }
        },
        { "Tarija", new DepartmentControls {
                Nombre = txtTarijaNombre,
                TotalGb = txtTarijaTotalGb,
                UsoGb = txtTarijaUsoGb,
                LibreGb = txtTarijaLibreGb,
                PieChart1 = null,
                PieChart2 = null
            }
        }
    };

            // Configuración inicial de cada botón: datos ocultos y nombre en rojo
            foreach (var dep in departmentControls.Values)
            {
                dep.Nombre.Foreground = new SolidColorBrush(Colors.Red);
                dep.TotalGb.Visibility = Visibility.Collapsed;
                dep.UsoGb.Visibility = Visibility.Collapsed;
                dep.LibreGb.Visibility = Visibility.Collapsed;
                if (dep.PieChart1 != null)
                    dep.PieChart1.Visibility = Visibility.Collapsed;
                if (dep.PieChart2 != null)
                    dep.PieChart2.Visibility = Visibility.Collapsed;
            }
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
            // Usamos RemoteEndPoint.ToString() para tener una clave única
            string clientKey = client.Client.RemoteEndPoint.ToString();
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            string clientMAC = ObtenerMacAddress(clientIP);
            string departamento = null;

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4094];

            try
            {
                // Leer el primer mensaje (Departamento)
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                departamento = Encoding.UTF8.GetString(buffer, 0, bytesRead)
                                .Trim().Replace("Departamento: ", "");

                if (mensajesDepartamentos.ContainsKey(departamento))
                {
                    // Se usa clientKey en lugar de clientIP para permitir múltiples conexiones desde la misma IP
                    clientesActivos[clientKey] = (departamento, clientMAC);
                    // Puedes registrar el cliente si lo deseas
                }
                else
                {
                    byte[] responseData = Encoding.UTF8.GetBytes("Departamento no válido.");
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    client.Close();
                    return;
                }

                // Mientras el cliente esté conectado se leen sus mensajes
                while (client.Connected)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    if (mensajesDepartamentos.ContainsKey(departamento))
                    {
                        mensajesDepartamentos[departamento].Add(receivedData);
                        ActualizarListaDepartamento(departamento, receivedData);

                        // Actualizar el gráfico si la ventana está abierta
                        if (ventanaGrafico != null && ventanaGrafico.IsVisible)
                        {
                            Dispatcher.Invoke(() => ActualizarGrafico(departamento));
                        }
                    }
                }
            }
            catch (IOException)
            {
                //Dispatcher.Invoke(() => lstMensajes.Items.Add($"Cliente desconectado: {clientIP} ({departamento})"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error con cliente {clientIP}: {ex.Message}");
                Dispatcher.Invoke(() => ActualizarEstadoDepartamento(departamento));
                //Dispatcher.Invoke(() => lstMensajes.Items.Add($"Error con cliente {clientIP}: {ex.Message}"));
            }
            finally
            {
                //client.Close();
                //if (departamento != null && clientesActivos.ContainsKey(clientIP))
                //{
                //    clientesActivos.Remove(clientIP);
                //    Dispatcher.Invoke(() => ActualizarEstadoDepartamento(departamento));
                //}
                client.Close();
                if (departamento != null && clientesActivos.ContainsKey(clientKey))
                {
                    clientesActivos.Remove(clientKey);
                    Dispatcher.Invoke(() => ActualizarEstadoDepartamento(departamento));
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

        private (long usado, long libre) ParseEstadoMessage(string mensaje)
        {
            try
            {
                // Intenta deserializar si el mensaje parece JSON
                if (mensaje.TrimStart().StartsWith("{"))
                {
                    dynamic msg = JsonConvert.DeserializeObject(mensaje);
                    // Se espera que msg.usado y msg.libre sean numéricos
                    long usado = Convert.ToInt64(msg.usado);
                    long libre = Convert.ToInt64(msg.libre);
                    return (usado, libre);
                }
            }
            catch { /* Si falla, se procede a parsear el texto */ }

            try
            {
                // Extracción manual del mensaje en texto
                // Ejemplo: "Almacenamiento - Usado: 123 MB, Libre: 456 MB"
                int indexUsado = mensaje.IndexOf("Usado:");
                int indexMB1 = mensaje.IndexOf("MB", indexUsado);
                string usadoStr = mensaje.Substring(indexUsado + "Usado:".Length, indexMB1 - (indexUsado + "Usado:".Length)).Trim();
                long usado = long.Parse(usadoStr);

                int indexLibre = mensaje.IndexOf("Libre:");
                int indexMB2 = mensaje.IndexOf("MB", indexLibre);
                string libreStr = mensaje.Substring(indexLibre + "Libre:".Length, indexMB2 - (indexLibre + "Libre:".Length)).Trim();
                long libre = long.Parse(libreStr);

                return (usado, libre);
            }
            catch (Exception ex)
            {
                // Si falla el parseo, se retornan valores 0
                Console.WriteLine("Error al parsear mensaje: " + ex.Message);
                return (0, 0);
            }
        }

        private void ActualizarListaDepartamento(string departamento, string mensaje)
        {
            //descerializar mensaje Json
            dynamic msg = JsonConvert.DeserializeObject(mensaje);
            //var (usado, libre) = ParseEstadoMessage(mensaje);
            /*
            === Datos Recibidos ===
                {
                  "Fecha": "2025-02-26 21:41:43",
                  "Departamento": "Pando",
                  "NombreEquipo": "LAPTOP-27QI9R81",
                  "Usuario": "rf924",
                  "IP": "192.168.1.12",
                  "MAC": "0A-00-27-00-00-17",
                  "Discos": [
                    {
                      "Disco": "C:\\",
                      "TipoDisco": "Fixed",
                      "SistemaArchivos": "NTFS",
                      "DiscoTotalGB": "338,33",
                      "EspacioUsadoGB": "323,86",
                      "EspacioLibreGB": "14,46",
                      "PorcentajeUso": "95,72%"
                    },
                    {
                      "Disco": "D:\\",
                      "TipoDisco": "Fixed",
                      "SistemaArchivos": "NTFS",
                      "DiscoTotalGB": "137,02",
                      "EspacioUsadoGB": "122,47",
                      "EspacioLibreGB": "14,55",
                      "PorcentajeUso": "89,38%"
                    }
                  ],
                  "RAM": {
                    "Total": "19789 MB",
                    "Usado": "17562 MB"
                  }
                }

            */

            Dispatcher.Invoke(() =>
            {
                if (departmentControls.TryGetValue(departamento, out DepartmentControls controls))
                {
                    // Aquí se muestra la suma de MB, ajusta la unidad si es necesario.
                    //controls.TotalGb.Text = (usado + libre) + " GB";
                    //controls.UsoGb.Text = usado + " GB";
                    //controls.LibreGb.Text = libre + " GB";
                    controls.TotalGb.Text = msg.Discos[0].DiscoTotalGB + " GB Total";
                    controls.UsoGb.Text = msg.Discos[0].EspacioUsadoGB + " GB Uso";
                    controls.LibreGb.Text = msg.Discos[0].EspacioLibreGB + " GB Libre";
                }
                ActualizarEstadoDepartamento(departamento);
            });
        }

        private void ActualizarEstadoDepartamento(string departamento)
        {
            bool activo = clientesActivos.Values.Any(c => c.Departamento == departamento);
            Console.WriteLine($"Clientes activos en {departamento}: {clientesActivos.Values.Count(c => c.Departamento == departamento)}");
            Dispatcher.Invoke(() =>
            {
                if (departmentControls.TryGetValue(departamento, out DepartmentControls controls))
                {
                    if (activo)
                    {
                        controls.Nombre.Foreground = new SolidColorBrush(Colors.White);
                        controls.TotalGb.Visibility = Visibility.Visible;
                        controls.UsoGb.Visibility = Visibility.Visible;
                        controls.LibreGb.Visibility = Visibility.Visible;
                        if (controls.PieChart1 != null)
                            controls.PieChart1.Visibility = Visibility.Visible;
                        if (controls.PieChart2 != null)
                            controls.PieChart2.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        controls.Nombre.Foreground = new SolidColorBrush(Colors.Red);
                        controls.TotalGb.Visibility = Visibility.Collapsed;
                        controls.UsoGb.Visibility = Visibility.Collapsed;
                        controls.LibreGb.Visibility = Visibility.Collapsed;
                        if (controls.PieChart1 != null)
                            controls.PieChart1.Visibility = Visibility.Collapsed;
                        if (controls.PieChart2 != null)
                            controls.PieChart2.Visibility = Visibility.Collapsed;

                        controls.TotalGb.Text = "400 GB";
                        controls.UsoGb.Text = "24 GB Uso";
                        controls.LibreGb.Text = "376 GB Libre";
                    }
                }
                int activeCount = clientesActivos.Values.Select(x => x.Departamento).Distinct().Count();
                ServidoresArribatxt.Text = $"Reportando {activeCount} de 9";
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
            
            Bdd ventana = new Bdd();
            ventana.Show();
            this.Close();
        }



        private W_Grafico ventanaGrafico; // Variable para mantener la ventana del gráfico abierta
        private Dictionary<string, DatosGrafico> historialGraficos = new Dictionary<string, DatosGrafico>(); // Historial de datos por departamento

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "¿Desea ver el histograma?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                
                var grid = sender as Grid;
                var departamento = grid?.Tag as string; 

                if (departamento != null && mensajesDepartamentos.ContainsKey(departamento))
                {
                    
                    int activeCount = clientesActivos.Values.Select(x => x.Departamento).Distinct().Count();

                    
                    if (ventanaGrafico == null || !ventanaGrafico.IsVisible)
                    {
                        ventanaGrafico = new W_Grafico();
                        ventanaGrafico.Show();
                    }

                    
                    ventanaGrafico.ActualizarServidoresReportando(activeCount);

                
                    ActualizarGrafico(departamento);
                }
            }
        }


        private void ActualizarGrafico(string departamento)
        {
            if (!mensajesDepartamentos.ContainsKey(departamento) || mensajesDepartamentos[departamento].Count == 0)
                return;

            var ultimoMensaje = mensajesDepartamentos[departamento].LastOrDefault();
            if (ultimoMensaje != null)
            {
                dynamic msg = JsonConvert.DeserializeObject(ultimoMensaje);

                dynamic discoConMasAlmacenamiento = null;
                double maxAlmacenamiento = 0;

                double ramUsadaMB = Convert.ToDouble(((string)msg.RAM.Usado).Replace(" MB", "").Trim(), CultureInfo.InvariantCulture);
                double ramTotalMB = Convert.ToDouble(((string)msg.RAM.Total).Replace(" MB", "").Trim(), CultureInfo.InvariantCulture);
                double porcentajeUsoRAM = (ramUsadaMB / ramTotalMB) * 100;

                foreach (var disco in msg.Discos)
                {
                    double discoTotalGB = Convert.ToDouble(((string)disco.DiscoTotalGB).Replace(",", "."), CultureInfo.InvariantCulture);
                    if (discoTotalGB > maxAlmacenamiento)
                    {
                        maxAlmacenamiento = discoTotalGB;
                        discoConMasAlmacenamiento = disco;
                    }
                }

                if (discoConMasAlmacenamiento != null)
                {
                    double espacioUsadoGB = Convert.ToDouble(((string)discoConMasAlmacenamiento.EspacioUsadoGB).Replace(",", "."), CultureInfo.InvariantCulture);
                    double espacioLibreGB = Convert.ToDouble(((string)discoConMasAlmacenamiento.EspacioLibreGB).Replace(",", "."), CultureInfo.InvariantCulture);
                    double discoTotalGB = Convert.ToDouble(((string)discoConMasAlmacenamiento.DiscoTotalGB).Replace(",", "."), CultureInfo.InvariantCulture);

                    if (!historialGraficos.ContainsKey(departamento))
                    {
                        historialGraficos[departamento] = new DatosGrafico
                        {
                            NombreRouter = departamento,
                            TotalEspacio = discoTotalGB.ToString("F2") + " GB",
                            UsoEspacio = espacioUsadoGB.ToString("F2") + " GB",
                            LibreEspacio = espacioLibreGB.ToString("F2") + " GB",
                            RAM = msg.RAM.Total,
                            IP = msg.IP,
                            Historial = new ObservableCollection<DatosGrafico.HistorialUso>(),
                            HistogramaValores = new List<double>(),
                            HistogramaLabels = new List<string>()
                        };
                    }

                    var datos = historialGraficos[departamento];

                   
                    DateTime fechaHora = DateTime.TryParse(msg.Fecha.ToString(), out DateTime fechaMsg) ? fechaMsg : DateTime.Now;
                    string fechaFormateada = fechaHora.ToString("yyyy-MM-dd");
                    string horaFormateada = fechaHora.ToString("HH:mm:ss");

                    datos.Historial.Add(new DatosGrafico.HistorialUso
                    {
                        Fecha = fechaFormateada,
                        Hora = horaFormateada,
                        UsoDiscoTB = Math.Round(porcentajeUsoRAM, 1)
                    });

                    datos.HistogramaValores.Add(Math.Round(porcentajeUsoRAM, 1));

                 
                    string etiquetaHistograma = horaFormateada;
                    datos.HistogramaLabels.Add(etiquetaHistograma);

                  
                    if (datos.HistogramaLabels.Count > 10)
                    {
                        datos.HistogramaLabels.RemoveAt(0);
                        datos.HistogramaValores.RemoveAt(0);
                    }

                  
                    ventanaGrafico.ActualizarDatos(datos);
                }
            }
        }
    }

    
}
    
    
