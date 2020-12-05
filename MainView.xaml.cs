using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
namespace ProyectoLA_PlanningScript_V1
{
    /// <summary>
    /// Lógica de interacción para MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public PlanningExecution PlanExecute;
        public StructureSet my_ss;
        public ScriptContext context;
        readonly BaseDeDatos<Configuracion> BDConfig = new BaseDeDatos<Configuracion>(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\DBConfiguration.jason");
        readonly BaseDeDatosStructures<BoolStringClass> BDStructures = new BaseDeDatosStructures<BoolStringClass>(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\DBStructures.jason");

        public MainView(ScriptContext Context)
        {
            InitializeComponent();
            PatientName.Text = Context.Patient.Name;
            PatientID.Text = Context.Patient.Id;
            StructSet.Text = Context.StructureSet.Id;
            CT.Text = Context.Image.Id;
            my_ss = Context.StructureSet;
            context = Context;
            CbTemplate.ItemsSource = TemplateName();
        }
        private List<string> TemplateName()
        {
            List<string> Names = new List<string>();
            BDConfig.Cargar();
            foreach (var data in BDConfig.valores)
            {
                Names.Add(data.Template);
            }
            return Names;
        }
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            BDConfig.Cargar();
            BDStructures.Cargar();
            if (CbTemplate.Text.ToString() == "") if (!BDStructures.valores.Any(x => x.First().Template.ToString() == CbTemplate.Text.ToString()))
                {
                    MessageBox.Show("No ha seleccionado un template de planificación, por favor, elija uno y ejecute el programa de nuevo", "ProyectoLA", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            MessageBox.Show(CbTemplate.Text);
            if (!BDConfig.valores.Any(x => x.Template.ToString() == CbTemplate.Text.ToString()))
            {
                MessageBox.Show("No existe un Template de planificación que coincida con lo seleccionado, por favor, ingrese a configuración cree uno y ejecute el programa de nuevo", "ProyectoLA", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!BDStructures.valores.Any(x => x.First().Template.ToString() == CbTemplate.Text.ToString()))
            {
                MessageBox.Show("No existe un Template de estructuras que coincida con lo seleccionado, por favor, cree el conjunto de estructuras y ejecute el programa de nuevo", "ProyectoLA", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Configuracion valoresConfig = BDConfig.Buscar(x => x.Template.ToString() == CbTemplate.Text.ToString())[0];//aqui enlazo lo que elijen del template con la base de datos
            ObservableCollection<BoolStringClass> valoresStructures = BDStructures.Buscar(x=>x.First().Template.ToString()== CbTemplate.Text.ToString())[0];
            MessageBox.Show(valoresConfig.Template);
            MessageBox.Show(valoresStructures.First().Template);
            PlanExecute = new PlanningExecution(context, valoresConfig, valoresStructures);
            MessageBox.Show("Proceso Terminado. Verifique la correcta ejecución de la tarea.", "ProyectoLA", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Credits_Click(object sender, RoutedEventArgs e)
        {
            Credits cred = new Credits(); // llamo la ventana de WPF de creditos
            cred.ShowDialog();
            //MessageBox.Show("Proyecto de Estandarización LatinoAmericano\nMSc. Alberto Alarcón Paredes\nMagister en Física Médica\n Universidad Mayor de San Simón \n Instituto Balseiro-FUESMEN-Universidad Nacional del Cuyo");
        }

        private void CB1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Apply.IsEnabled = true;
            //CB1.SelectedValue.ToString();
        }

        private void BtConfiguration_Click(object sender, RoutedEventArgs e)
        {
            Configuration configu = new Configuration(context); // llamo la ventana de WPF de creditos
            configu.ShowDialog();
        }

        private void CbTemplate_MouseEnter(object sender, MouseEventArgs e)
        {
            LoadNames();
        }

        private void LoadNames()
        {
            CbTemplate.ItemsSource = null;
            CbTemplate.ItemsSource = TemplateName();
        }
    }
}
