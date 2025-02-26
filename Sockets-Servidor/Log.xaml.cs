using LiveCharts.Wpf;
using LiveCharts;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Sockets_Servidor
{
    /// <summary>
    /// Lógica de interacción para Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        private string serverIP;
        private string serverName;
        private string departamento;
        private Dictionary<string, (string Departamento, string MAC)> clientesActivos;
        private List<string> mensajes;
        private DispatcherTimer timer;
        public Log(string serverIP, string serverName, string departamento, Dictionary<string, (string, string)> clientes, List<string> mensajes)
        {
            InitializeComponent();
            this.serverIP = serverIP;
            this.serverName = serverName;
            this.departamento = departamento;
            this.clientesActivos = clientes;
            this.mensajes = mensajes;
            lblServerInfo.Text = $"{serverName} ({serverIP})";
            lblDepartamento.Text = $"Departamento: {departamento}";

           


        }

        //private void SetupChart()
        //{
        //    var series = new LineSeries
        //    {
        //        Title = "Uso",
        //        Values = new ChartValues<int> { 0 }, // Inicializar con un valor por defecto
        //        PointGeometry = DefaultGeometries.Circle
        //    };
        //    usageChart.Series.Clear(); // Limpiar cualquier serie existente
        //    usageChart.Series.Add(series);
        //}

        //private void ActualizarDatos()
        //{
        //    if (clientesActivos.ContainsKey(departamento))
        //    {
        //        var cliente = clientesActivos[departamento];
        //        lstMensajes.ItemsSource = $"Cliente: {cliente.Departamento} - MAC: {cliente.MAC}";
        //    }

        //    lstMensajes.ItemsSource = null;
        //    lstMensajes.ItemsSource = mensajes;

        //    // Simulación de carga de uso
        //    Random rnd = new Random();
        //    var series = (LineSeries)usageChart.Series[0];

        //    series.Values.Add(rnd.Next(10, 100)); // Agregar nuevo punto aleatorio

        //    if (series.Values.Count > 20) // Limitar a 20 puntos para evitar sobrecarga
        //    {
        //        series.Values.RemoveAt(0);
        //    }

        //}

        private void VolverButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}