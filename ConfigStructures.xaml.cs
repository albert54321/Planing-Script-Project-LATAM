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
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;

namespace ProyectoLA_PlanningScript_V1
{
    /// <summary>
    /// Lógica de interacción para ConfigStructures.xaml
    /// </summary>
    public partial class ConfigStructures : Window
    {
        public ObservableCollection<BoolStringClass> TheList { get; set; }
        readonly BaseDeDatosStructures<BoolStringClass> bd = new BaseDeDatosStructures<BoolStringClass>(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data\\DBStructures.jason");
        string operacion = "";
        StructureSet ss;
        public ConfigStructures(StructureSet sset,string txtTemplate)
        {
            InitializeComponent();
            bd.Cargar();
            ss = sset;
            TheList = new ObservableCollection<BoolStringClass>();
            foreach (var st in ss.Structures)
            {
                TheList.Add(new BoolStringClass { IsSelected = false, TheText = st.Id, Template = txtTemplate,Dose="0",DoseOAR="0",VolumenOAR="-", Prioridad="0" });
            }
            LbEstructuras.ItemsSource = TheList;
            this.DataContext = this;
            Mostrar();
            TbBuscarAct.Text = txtTemplate;
        }
        private void Mostrar()
        {
            bd.Cargar();
            List<StringTransfer> NombresTemplate = new List<StringTransfer>();
            foreach (var item in bd.valores)
            {
                NombresTemplate.Add(new StringTransfer { Template = item.First().Template.ToString() });
            }
            DGV.ItemsSource = null;
            DGV.ItemsSource = NombresTemplate;
        }
        private bool VerifNames(string nombre)
        {
            bd.Cargar();
            return bd.valores.Any(x => x.First().Template.ToString() == nombre);
        }
        private void BtInsertar_Click(object sender, RoutedEventArgs e)
        {
            operacion = "insertar";
            if (TbBuscarAct.Text.ToString() == "" || TbBuscarAct.Text.ToString() == "Insertar, Actualizar o Eliminar...")
            {
                MessageBox.Show("No se puede " + operacion + " porque el campo de template esta vacio, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (VerifNames(TbBuscarAct.Text.ToString()))
            {
                MessageBox.Show("No se puede " + operacion + " porque el nombre del template ya existe en la base de datos, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (var element in TheList)
            {
                element.Template = TbBuscarAct.Text.ToString();
            }
            bd.Insertar(TheList);
            MessageBox.Show("Se inserto el Template:  \"" + TbBuscarAct.Text.ToString() + "\", con éxito. La ventana se cerrará", "Insertar", MessageBoxButton.OK, MessageBoxImage.Information);
            Mostrar();
            this.Close();
        }

        private void BtActualizar_Click(object sender, RoutedEventArgs e)
        {
            operacion = "actualizar";
            if (TbBuscarAct.Text.ToString() == "" || TbBuscarAct.Text.ToString() == "Insertar, Actualizar o Eliminar...")
            {
                MessageBox.Show("No se puede " + operacion + " porque el campo de template esta vacio, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (!VerifNames(TbBuscarAct.Text.ToString()))
            {
                MessageBox.Show("No se puede " + operacion + " porque el nombre del template no coincide con ningún elemento en la base de datos, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (var item in TheList)
            {
                item.Template = TbBuscarAct.Text.ToString();
            }
            bd.Actualizar(x => x.All(y => y.Template.ToString() == TbBuscarAct.Text.ToString()), TheList);
            MessageBox.Show("Se actualizó el Template:  " + TbBuscarAct.Text.ToString() + " , con éxito.", "Actualizar", MessageBoxButton.OK, MessageBoxImage.Information);
            Mostrar();
        }
        private void BtLimpiar_Click(object sender, RoutedEventArgs e)
        {
            TbBuscarAct.Text = "";
            foreach (var element in TheList)
            {
                element.IsSelected = false;
                element.Template = "******************************";
                element.Dose = "0";
                element.DoseOAR = "0";
                element.VolumenOAR = "-";
                element.Prioridad = "0";
            }
            LbEstructuras.ItemsSource = null;
            LbEstructuras.ItemsSource = TheList;
        }
        private void BtEliminar_Click(object sender, RoutedEventArgs e)
        {
            operacion = "eliminar";
            if (TbBuscarAct.Text.ToString() == "" || TbBuscarAct.Text.ToString() == "Insertar, Actualizar o Eliminar...")
            {
                MessageBox.Show("No se puede " + operacion + " porque no tiene un Template cargado. Cargue uno e intentelo de nuevo", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (!VerifNames(TbBuscarAct.Text.ToString()))
            {
                MessageBox.Show("No se puede " + operacion + " porque el nombre del template no coincide con ningún elemento en la base de datos, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            bd.Eliminar(x => x.First().Template.ToString() == TbBuscarAct.Text.ToString());
            MessageBox.Show("Se eliminó el Template:  " + TbBuscarAct.Text.ToString() + ", con éxito.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Information);
            Mostrar();
        }
        private void DGV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                bd.Cargar();
                StringTransfer row = (StringTransfer)DGV.SelectedItems[0];
                ObservableCollection<BoolStringClass> valores = bd.Buscar(x => x.First().Template.ToString() == row.Template.ToString())[0];
                TheList = valores;
                LbEstructuras.ItemsSource = TheList;
                TbBuscarAct.Text = TheList.First().Template.ToString();
            }
            catch (Exception)
            {
            }
        }
        private void TbBuscarAct_MouseEnter(object sender, MouseEventArgs e)
        {
            if (TbBuscarAct.Text.ToString() == "" || TbBuscarAct.Text.ToString() == "Insertar, Actualizar o Eliminar...") TbBuscarAct.Text = "";
        }

        private void TbBuscarAct_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TbBuscarAct.Text.ToString() == "" || TbBuscarAct.Text.ToString() == "Insertar, Actualizar o Eliminar...") TbBuscarAct.Text = "Insertar, Actualizar o Eliminar...";
        }

        public class StringTransfer
        {
            public string Template { get; set; }
        }
    }
}