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
using Google.Cloud.Firestore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sockets_Servidor.credenciales
{
    /// <summary>
    /// Lógica de interacción para Bdd.xaml
    /// </summary>
    public partial class Bdd : Window
    {

        private const string ProjectId = "servidoresdb-683e3"; //ID de proyecto
        //private const string CredentialsPath = "credenciales/credenciales.json"; // ruta a archivo JSON
        //volver 3 directorios y buscar la carpeta credenciales
        private const string CredentialsPath = "../../../credenciales/credenciales.json"; // ruta a archivo JSON
        public Bdd()
        {
            InitializeComponent();
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", CredentialsPath);
        }
        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            if (int.TryParse(AgeTextBox.Text, out int age))
            {
                await AddUserToFirestore(name, age);
                ResultTextBlock.Text = "Usuario agregado con éxito.";
            }
            else
            {
                ResultTextBlock.Text = "Por favor, ingresa una edad válida.";
            }
        }

        private async Task AddUserToFirestore(string name, int age)
        {
            FirestoreDb db = FirestoreDb.Create(ProjectId);
            var docRef = db.Collection("usuarios").Document(name);
            await docRef.SetAsync(new { Nombre = name, Edad = age });
        }
    }
}
