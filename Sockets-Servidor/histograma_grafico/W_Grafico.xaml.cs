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

namespace Sockets_Servidor.histograma_grafico
{
    /// <summary>
    /// Lógica de interacción para W_Grafico.xaml
    /// </summary>
    public partial class W_Grafico : Window
    {
        public W_Grafico()
        {
            InitializeComponent();

        }

        public void ActualizarServidoresReportando(int activeCount)
        {
            ServidoresArribatxt.Text = $"Reportando {activeCount} de 9";
        }

        public void ActualizarDatos(DatosGrafico datos)
        {
        
            this.DataContext = datos;

            txtNombreRouter.Text = $"Nombre Cliente:\n{datos.NombreRouter}";
            txtTotalEspacio.Text = $"Total de espacio:\n{datos.TotalEspacio} GB";
            txtUsoEspacio.Text = $"Uso de espacio:\n{datos.UsoEspacio} GB";
            txtLibreEspacio.Text = $"Espacio libre:\n{datos.LibreEspacio} GB";
            txtRam.Text = $"Memoria RAM:\n{datos.RAM} GB";
            txtIP.Text = $"Dirección IP:\n{datos.IP}";

            histogramaChart.Series[0].Values = new LiveCharts.ChartValues<double>(datos.HistogramaValores);
            histogramaChart.AxisX[0].Labels = datos.HistogramaLabels;
        }

        private void CerrarVentana(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
