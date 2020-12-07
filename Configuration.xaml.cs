using System;
using System.Collections.Generic;
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
using VMS.TPS.Common.Model.Types;

namespace ProyectoLA_PlanningScript_V1
{
    /// <summary>
    /// Lógica de interacción para Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
        readonly BaseDeDatos<Configuracion> bd = new BaseDeDatos<Configuracion>(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\Data\\DBConfiguration.jason");
        readonly List<string> MLC = new List<string>();
        readonly List<string> Energia = new List<string>();
        readonly List<string> Tecnica = new List<string>();
        public StructureSet SetSt;
        public PlanSetup _plan;

        private void Mostrar(List<Configuracion> lista)
        {
            DGV.ItemsSource = null;
            DGV.ItemsSource = lista;
        }

        public Configuration(ScriptContext context)
        {
            InitializeComponent();
            SetSt = context.StructureSet;
            _plan = context.PlanSetup;
            MLC.Add("Millennium_120");
            MLC.Add("VMS HD 120");
            MLC.Add("EDGE_MLC");
            MLC.Add("Millennium 120");
            MLC.Add("HD_120");
            cbMLC.ItemsSource = MLC;

            Energia.Add("6X");
            Energia.Add("10X");
            cbEnergia.ItemsSource = Energia;

            Tecnica.Add("ARC");
            Tecnica.Add("SRS ARC");
            cbTecnica.ItemsSource = Tecnica;

            List<string> listaNombres = new List<string>();
            foreach (var item in SetSt.Structures)
            {
                listaNombres.Add(item.Id);
            }
            cbestructura.ItemsSource = listaNombres;

            try
            {
                txtPVD.Text = _plan.GetCalculationModel(CalculationType.PhotonVolumeDose).ToString();
                txtDVHestimation.Text = _plan.GetCalculationModel(CalculationType.DVHEstimation).ToString();
                txtPVO.Text = _plan.GetCalculationModel(CalculationType.PhotonVMATOptimization).ToString();
                txtnumero.Text = _plan.Beams.Count(x=>!x.IsSetupField).ToString();
                txtDosis.Text = Convert.ToString((_plan.DosePerFraction.Dose / 100.0) * _plan.NumberOfFractions);
                txtTasaDosis.Text = _plan.Beams.FirstOrDefault(x=>!x.IsSetupField).DoseRate.ToString();
                txtMaquina.Text = _plan.Beams.First().TreatmentUnit.Id.ToString();
                cbMLC.Text = _plan.Beams.FirstOrDefault(x => !x.IsSetupField).MLC.Id.ToString();
                txtPlan.Text = _plan.Beams.First().Plan.Id.ToString();
                txtCurso.Text = _plan.Course.Id;
                txtfracciones.Text = _plan.NumberOfFractions.ToString();
                cbTecnica.Text= _plan.Beams.FirstOrDefault(x=>!x.IsSetupField).Technique.Id.ToString();
                cbEnergia.Text = _plan.Beams.FirstOrDefault(x=>!x.IsSetupField).EnergyModeDisplayName.ToString();
                txtcolimador.Text= _plan.Beams.FirstOrDefault(x => !x.IsSetupField).ControlPoints.First().CollimatorAngle.ToString();
            }
            catch (Exception){}
            try
            {
                var beamLogs = _plan.Beams.FirstOrDefault(x => !x.IsSetupField).CalculationLogs;
                if (beamLogs.Any(x => x.Category == "Dose"))
                {
                    txtgrilla.Text= beamLogs.FirstOrDefault(x => x.Category == "Dose").MessageLines.FirstOrDefault(x => x.Contains("CalculationGridSizeInCM")).Split('=').Last();

                    if (beamLogs.FirstOrDefault(x => x.Category == "Dose").MessageLines.FirstOrDefault(x => x.Contains("HeterogeneityCorrection")).Split('=').Last() == "ON") boolHCorrection.IsChecked = true;
                }
            }
            catch (Exception){}
            try
            {
                cbestructura.Text = SetSt.Structures.FirstOrDefault(x => x.Id.Contains("PTV_Total")).Id.ToString();
            }
            catch (Exception){}
            bd.Cargar();
            Mostrar(bd.valores); //da error no se porqie
        }

        private void Insertar_Click(object sender, RoutedEventArgs e)
        {
            Configuracion valores = new Configuracion()
            {
                Maquina = txtMaquina.Text.ToString(),
                MLC = cbMLC.Text.ToString(),
                Energia = cbEnergia.Text.ToString(),
                Curso = txtCurso.Text.ToString(),
                Plan = txtPlan.Text.ToString(),
                PVO = txtPVO.Text.ToString(),
                PVD = txtPVD.Text.ToString(),
                DVH = txtDVHestimation.Text.ToString(),
                Grilla = txtgrilla.Text.ToString(),
                CorreccionH = boolHCorrection.IsChecked,
                ArcoCompleto = boolCompleto.IsChecked,
                Inicio = Convert.ToDouble(txtinicio.Text.ToString()),
                Fin = Convert.ToDouble(txtfin.Text.ToString()),
                ArcoNumero = Convert.ToInt32(txtnumero.Text.ToString()),
                Estructura = cbestructura.Text.ToString(),
                Colimador = Convert.ToDouble(txtcolimador.Text.ToString()),
                Template = txtTemplate.Text.ToString(),
                NumeroFracciones = Convert.ToInt32(txtfracciones.Text.ToString()),
                Dosis = Convert.ToDouble(txtDosis.Text.ToString()),
                Modelo = txtDVHmodel.Text.ToString(),
                TasaDosis = Convert.ToInt32(txtTasaDosis.Text.ToString()),
                Tecnica = cbTecnica.Text.ToString(),
                IsMama = boolEsmama.IsChecked,
                IsRP=boolRP.IsChecked

            };
            string operacion = "insertar";
            if (txtTemplate.Text.ToString() == "")
            {
                MessageBox.Show("No se puede " + operacion + " porque el campo de template esta vacio, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (var valor in bd.valores)
            {
                if (valor.Template == txtTemplate.Text.ToString())
                {
                    MessageBox.Show("No se puede " + operacion + " porque tiene el mismo nombre que otro template, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            bd.Insertar(valores);
            MessageBox.Show("Se inserto el Template: \"" + txtTemplate.Text.ToString() + "\", con éxito.", "Insertar", MessageBoxButton.OK, MessageBoxImage.Information);
            Mostrar(bd.valores);
        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            Configuracion valores = new Configuracion()
            {
                Maquina = txtMaquina.Text.ToString(),
                MLC = cbMLC.Text.ToString(),
                Energia = cbEnergia.Text.ToString(),
                Curso = txtCurso.Text.ToString(),
                Plan = txtPlan.Text.ToString(),
                PVO = txtPVO.Text.ToString(),
                PVD = txtPVD.Text.ToString(),
                DVH = txtDVHestimation.Text.ToString(),
                Grilla = txtgrilla.Text.ToString(),
                CorreccionH = boolHCorrection.IsChecked,
                ArcoCompleto = boolCompleto.IsChecked,
                Inicio = Convert.ToDouble(txtinicio.Text.ToString()),
                Fin = Convert.ToDouble(txtfin.Text.ToString()),
                ArcoNumero = Convert.ToInt32(txtnumero.Text.ToString()),
                Estructura = cbestructura.Text.ToString(),
                Colimador = Convert.ToDouble(txtcolimador.Text.ToString()),
                Template = txtTemplate.Text,
                NumeroFracciones = Convert.ToInt32(txtfracciones.Text.ToString()),
                Dosis = Convert.ToDouble(txtDosis.Text.ToString()),
                Modelo=txtDVHmodel.Text.ToString(),
                TasaDosis= Convert.ToInt32(txtTasaDosis.Text.ToString()),
                Tecnica = cbTecnica.Text.ToString(),
                IsMama = boolEsmama.IsChecked,
                IsRP=boolRP.IsChecked
            };
            string operacion = "actualizar";
            if (txtTemplate.Text.ToString() == "")
            {
                MessageBox.Show("No se puede " + operacion + " porque el campo de template esta vacio, cambie el nombre e intentelo de nuevo", "Error al " + operacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            bd.Actualizar(x => x.Template == txtTemplate.Text.ToString(), valores);
            MessageBox.Show("Se actualizó el Template: " + txtTemplate.Text.ToString() + " , con éxito.", "Actualizar", MessageBoxButton.OK, MessageBoxImage.Information);
            Mostrar(bd.valores);
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (txtTemplate.Text.ToString() == "")
            {
                MessageBox.Show("No se puede elimnar porque no tiene un Template cargado. Cargue uno e intentelo de nuevo", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            bd.Eliminar(x => x.Template == txtTemplate.Text.ToString());
            MessageBox.Show("Se eliminó el Template:" + txtTemplate.Text.ToString() + ", con éxito.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Information);
            Mostrar(bd.valores);
        }

        private void DGV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Configuracion row = (Configuracion)DGV.SelectedItems[0];
                Configuracion valores = bd.Buscar(x => x.Template.ToString() == row.Template.ToString())[0];
                txtMaquina.Text = valores.Maquina;
                cbMLC.Text = valores.MLC;
                cbEnergia.Text = valores.Energia;
                txtCurso.Text = valores.Curso;
                txtPlan.Text = valores.Plan;
                txtPVO.Text = valores.PVO;
                txtPVD.Text = valores.PVD;
                txtDVHestimation.Text = valores.DVH;
                txtgrilla.Text =valores.Grilla;
                boolHCorrection.IsChecked = valores.CorreccionH;
                boolCompleto.IsChecked = valores.ArcoCompleto;
                txtinicio.Text = Convert.ToString(valores.Inicio);
                txtfin.Text = Convert.ToString(valores.Fin);
                txtnumero.Text = Convert.ToString(valores.ArcoNumero);
                cbestructura.Text = valores.Estructura;
                txtcolimador.Text = Convert.ToString(valores.Colimador);
                txtTemplate.Text = valores.Template;
                txtfracciones.Text = Convert.ToString(valores.NumeroFracciones);
                txtDosis.Text = Convert.ToString(valores.Dosis);
                txtDVHmodel.Text = valores.Modelo;
                txtTasaDosis.Text = Convert.ToString(valores.TasaDosis);
                cbTecnica.Text = valores.Tecnica;
                boolEsmama.IsChecked = valores.IsMama;
                boolRP.IsChecked= valores.IsRP;
            }
            catch (Exception)
            {
            }
        }

        private void BoolCompleto_Checked(object sender, RoutedEventArgs e)
        {
            txtinicio.Text = "181";
            txtfin.Text = "179";
        }

        private void Cargar_Click(object sender, RoutedEventArgs e)
        {
            txtMaquina.Text = "";
            cbMLC.Text = "";
            cbEnergia.Text = "";
            txtCurso.Text = "";
            txtPlan.Text = "";
            txtPVO.Text = "";
            txtPVD.Text = "";
            txtDVHestimation.Text = "";
            txtgrilla.Text = "";
            boolHCorrection.IsChecked = false;
            boolCompleto.IsChecked = false;
            txtinicio.Text = "";
            txtfin.Text = "";
            txtnumero.Text = "";
            cbestructura.Text = "";
            txtcolimador.Text = "";
            txtTemplate.Text = "";
            txtfracciones.Text = "";
            txtDosis.Text = "";
            txtDVHmodel.Text = "";
            txtTasaDosis.Text = "";
            cbTecnica.Text = "";
            boolEsmama.IsChecked = false;
            boolRP.IsChecked = false;
        }

        private void TxtBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            var lista = bd.Buscar(x => x.Template.Contains(txtBuscar.Text.ToString()));
            Mostrar(lista);
        }

        private void TxtBuscar_MouseEnter(object sender, MouseEventArgs e)
        {
            txtBuscar.Text = "";
        }

        private void TxtBuscar_MouseLeave(object sender, MouseEventArgs e)
        {
            txtBuscar.Text = "Buscar...";
        }

        private void ConfigSt_Click(object sender, RoutedEventArgs e)
        {
            ConfigStructures config = new ConfigStructures(SetSt, txtTemplate.Text.ToString()); // llamo la ventana de WPF de creditos
            config.ShowDialog();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}