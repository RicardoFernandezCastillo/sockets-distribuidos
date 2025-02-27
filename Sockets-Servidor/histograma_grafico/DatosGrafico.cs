using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sockets_Servidor.histograma_grafico
{
    public class DatosGrafico
    {
        public string NombreRouter { get; set; }
        public string TotalEspacio { get; set; }
        public string UsoEspacio { get; set; }
        public string LibreEspacio { get; set; }
        public string RAM { get; set; }
        public string IP { get; set; }
        public ObservableCollection<HistorialUso> Historial { get; set; } // Usar ObservableCollection
        public List<double> HistogramaValores { get; set; }
        public List<string> HistogramaLabels { get; set; }

        public DatosGrafico()
        {
            Historial = new ObservableCollection<HistorialUso>(); // Inicializar la colección
            HistogramaValores = new List<double>();
            HistogramaLabels = new List<string>();
        }

        public class HistorialUso
        {
            public string Fecha { get; set; }
            public string Hora { get; set; }
            public double UsoDiscoTB { get; set; }
        }


    }
}
