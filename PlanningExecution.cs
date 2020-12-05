using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.Types;
using VMS.TPS.Common.Model.API;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Collections.ObjectModel;

namespace ProyectoLA_PlanningScript_V1
{
    public class PlanningExecution : INotifyPropertyChanged
    {
        //Propiedades:
        private string template;
        public string Template
        {
            get { return template; }
            set
            {
                template = value;
                NotifyPropertyChanged("Template");
            }
        }
        Configuracion valoresConfig;
        ObservableCollection<BoolStringClass> valoresSts;

        //public string Template { get; set; }
        public string FileName { get; set; }

        public List<string> StructuresNames { get; set; }

        public StructureSet My_ss { get; set; }

        //Eventos:
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //Genericos

        private string[] SeparateLines(string lines)
        {
            Regex CSVParser = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            //Separating columns to array
            string[] optionline = CSVParser.Split(lines);//divide la linea con los signos mostrado anteriormente
            if (optionline.Length <= 1) optionline = lines.Split(',');
            return optionline;
        }

        private Structure Add_Structure(StructureSet ss, string[] optionline)
        {
            Structure st = ss.Structures.FirstOrDefault(x => x.Id == optionline[3]);
            if (st == null) st = ss.AddStructure(optionline[4], optionline[3]);
            return st;
        }

        private Structure Add_AuxStructure(StructureSet ss)
        {
            Structure st = ss.Structures.FirstOrDefault(x => x.Id == "Auxi");
            if (st == null) st = ss.AddStructure("CONTROL", "Auxi");
            return st;
        }

        private Structure NullEmpty(StructureSet ss, string name)
        {
            Structure st = ss.Structures.FirstOrDefault(x => x.Id == name);
            if (st == null || st.IsEmpty) throw new Exception("No se puede realizar la operacion dada con la estructura:" + st.Id);
            return st;
        }
        private void FindCouch(StructureSet ss)
        {
            if (!ss.Structures.Any(x => x.Id.Contains("Couch")))
            {
                System.Windows.MessageBox.Show("No se encontro  la camilla, no se olvide añadirlo", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                throw new Exception("Añada la camilla y vuelva ha ejecutar el programa");
            };
        }


        //Metodos de la clase:
        public PlanningExecution(ScriptContext context,Configuracion valoresC, ObservableCollection<BoolStringClass> valoresStructures )
        {
            valoresConfig = valoresC;
            valoresSts = valoresStructures;
            My_ss = context.StructureSet;
            StartPlan(context);
        }

        public void StartPlan(ScriptContext context)
        {
            Patient patient = context.Patient;
            StructureSet ss = context.StructureSet;
            PlanSetup ps = context.PlanSetup;
            if (patient == null || ss == null)
            {
                System.Windows.MessageBox.Show("Please load a patient, 3D image, and structure set before running this script.", "Planificación", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (!ss.Structures.Any(x => x.Id == valoresConfig.Estructura))
            {
                System.Windows.MessageBox.Show(valoresConfig.Estructura + " no se pudo encontrar, script no se ejecutará");
                return;
            }
            ExternalPlanSetup cureps = SettingPlan(context);
            if (Question() == MessageBoxResult.No)////////////?????
            {
                return;//pregunta si desea  continuar esto es para para parar en el arreglo
            }
            else Optimization_dose(cureps);
            //Normalization(cureps, ptv_prvs, RxDose, NFractions, 97);//normaliza al valor de ptv-prvs98%
        }

        public void Optimization_dose(ExternalPlanSetup cureps, bool CalculateDose = true)
        {
            Dictionary<string, DoseValue> DVH_dose = new Dictionary<string, DoseValue>();
            Dictionary<string, string> DVH_struct = new Dictionary<string, string>();
            foreach (var item in valoresSts.Where(x=>x.TheText.Contains("PTV") && x.IsSelected==true) )
            {
                DVH_dose.Add(item.TheText.ToString(), new DoseValue(Convert.ToDouble(item.Dose), "Gy"));
                MessageBox.Show("nombre " + item.TheText + " select" + item.IsSelected);
            }
            foreach (var item in valoresSts.Where(x => !x.TheText.Contains("PTV") && x.IsSelected == true && x.VolumenOAR=="-")) 
            {
                DVH_struct.Add(item.TheText.ToString(), item.TheText.ToString());
                MessageBox.Show("nombre " + item.TheText + " select" + item.IsSelected);
            }
            try
            {
                if (Convert.ToBoolean(valoresConfig.IsRP))
                {
                    string[] N_Bowel = { "Bowel", "bowels", "intestinos", "Intestino", "intestino", "Delgado" };
                    cureps.CalculateDVHEstimates(valoresConfig.Modelo, DVH_dose, DVH_struct);//ID DEL MODELO // DOSIS /MATCH STRUCTURA
                    foreach (var element in valoresSts)
                    {
                        if (element.VolumenOAR != "-")
                        {
                            cureps.OptimizationSetup.AddPointObjective(My_ss.Structures.FirstOrDefault(x => x.Id == element.TheText.ToString()), OptimizationObjectiveOperator.Upper, new DoseValue(Convert.ToDouble(element.DoseOAR), "Gy"), Convert.ToDouble(element.VolumenOAR), Convert.ToDouble(element.Prioridad));
                        }
                    }
                    //cureps.OptimizationSetup.AddAutomaticNormalTissueObjective(100.0f); //anade normal tissio automatico
                    if (CalculateDose) Opti_cureps(cureps);//calcula lo elemenal y la dosis
                                                           //esto es para queitar obejitvos de una estructura esto porque cada vez que a;ado objetivos no se reemplaza sino se aumenta
                                                           //foreach (OptimizationObjective y in objetives.Where(x => x.StructureId == "Bowel")) cureps.OptimizationSetup.RemoveObjective(y);//quito el objetivo de bowel para que no haya mas
                                                           //IEnumerable<OptimizationObjective> objetives = Enumerable.Empty<OptimizationObjective>();
                                                           //objetives = cureps.OptimizationSetup.Objectives;//trucho inizializar y luego asignar
                                                           //double Bowel_priority = 220;
                                                           //DoseValue bowelDose = cureps.GetDoseAtVolume(bowel, 0, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);
                                                           //while (bowelDose.Dose > 25.2 && Bowel_priority <= (220 + 3 * 50))
                                                           //{
                                                           //    //DialogResult result = System.Windows.Forms.MessageBox.Show("La dosis del instestino es:" + bowelDose.Dose.ToString("0.00") + "_" + bowelDose.UnitAsString + ". Desea terminar el Script?", "Warning", MessageBoxButtons.YesNo);
                                                           //    //if (result == DialogResult.Yes) break;
                                                           //    Bowel_priority = Bowel_priority + 50;

                    //    foreach (OptimizationObjective y in objetives.Where(x => x.StructureId == "Bowel_PRV")) cureps.OptimizationSetup.RemoveObjective(y);//quito el objetivo de bowel para que no haya mas
                    //    cureps.OptimizationSetup.AddPointObjective(bowel, OptimizationObjectiveOperator.Upper, new DoseValue(24.2, "Gy"), 0, Bowel_priority);

                    //    cureps.OptimizationSetup.AddPointObjective(bowel, OptimizationObjectiveOperator.Upper, new DoseValue(19.5, "Gy"), 10, Bowel_priority - 50);
                    //    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Bowel_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 10, Bowel_priority - 110);
                    //    //Opti_cureps(cureps);//calcula lo elemenal y la dosis
                    //    bowelDose = cureps.GetDoseAtVolume(bowel, 0, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);
                    //}
                }
            }
            catch (Exception)
            {
                MessageBoxResult RP = MessageBox.Show("No se pudo enlazar las estructuras con el modelo de RapidPlan dado, revise que todas las estructuras de configuración de estructuras esten correctas.", "Advertencia", MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            if (!Convert.ToBoolean(valoresConfig.IsRP)) 
            {
                MessageBoxResult RP = MessageBox.Show("Pronto se podra optimizar con templates precargados. Espere la próxima versión.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
                //SetDictionaries_Prost(out DVH_struct, out DVH_dose, cureps, PTVs_names, RxDose, is_rapidplan, rapidplan, select);//aqu queda los diccionarios de rapidplan
                //if (CalculateDose) Opti_cureps(cureps);//calcula lo elemenal y la dosis
            }
        }
    
    public static void Opti_cureps(ExternalPlanSetup cureps)
    {
        OptimizerResult optresult = cureps.OptimizeVMAT(new OptimizationOptionsVMAT(OptimizationIntermediateDoseOption.UseIntermediateDose, string.Empty));
        cureps.OptimizeVMAT();
        cureps.CalculateDose();
        //cureps.PlanNormalizationValue = 100.2f;//esta normalizacion es la isododis de normalizaczacion no lo que colocamos en %tratamiento
    }

    public ExternalPlanSetup SetDictionaries_Prost(out Dictionary<string, string> DVH_struct, out Dictionary<string, DoseValue> DVH_dose, ExternalPlanSetup cureps, List<string[]> PTVs_names, Double RxDose, bool is_rapidplan, string[] rapidplan, int select = 0)
        {
            DVH_struct = new Dictionary<string, string>();
            DVH_dose = new Dictionary<string, DoseValue>();
            string[] PTV_ID17 = { "PTV-PRVs!" };
            string[] PTV_ID20 = { "PTV_High_3625" };
            string[] PTV_ID20_ = { "PTV_High_4000" };
            string[] PTV_ID21 = { "PTV_Low_2500" };
            //names
            string[] N_Urethra = { "Urethra", "Uretra", "uretra" };
            string[] N_Trigone = { "Trigone", "trigono", "Trigono" };
            string[] N_Bladder = { "Bladder", "Vejiga", "vejiga", "Vejiga1" };
            string[] N_Rectum = { "Rectum", "recto", "rectum" };
            string[] N_Body = { "Body", "Outer Contour", "body" };
            //cambiar nombres
            string[] N_HJL = { "FemoralJoint_L", "Hip Joint, Left", "Hip Joint Left", "CFI", "CFi" };//hip joint left
            string[] N_HJR = { "FemoralJoint_R", "Hip Joint, Right", "Hip Joint Right", "CFD" };
            string[] N_Penile = { "PenileBulb", "Penile Bulb", "Pene B", "penile bulb", "B Pene", "Bulbo" };
            //oar
            string[] PRV_Rectum = { "Rectum_PRV05" };
            string[] Rect_ant = { "Rectum_A" };
            string[] Rect_post = { "Rectum_P" };
            //diccionario de estructuras para aparear
            string[] N_Colon = { "Colon", "colon", "sigma", "Grueso" };
            string[] N_Bowel = { "Bowel", "bowels", "intestinos", "Intestino", "intestino", "Delgado" };
            if (is_rapidplan)
            {
                foreach (string[] x in PTVs_names)//anade ptv con dose y estructure
                {
                    if (x != PTV_ID21) DVH_dose.Add(x.FirstOrDefault(y => cureps.StructureSet.Structures.Any(s => s.Id == y)), new DoseValue(RxDose, "Gy"));//COLOCAR NOMBRE DE LA ESTRUCTURA
                    else DVH_dose.Add(x.FirstOrDefault(y => cureps.StructureSet.Structures.Any(s => s.Id == y)), new DoseValue(25, "Gy"));//esto para colocar dosis de 25gy a los ganglios
                    DVH_struct.Add(x.FirstOrDefault(y => cureps.StructureSet.Structures.Any(s => s.Id == y)), x[0]);

                }
                DVH_struct.Add(N_Bladder.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), N_Bladder[0]);
                DVH_struct.Add(N_HJL.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), N_HJL[0]);
                DVH_struct.Add(N_HJR.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), N_HJR[0]);
                DVH_struct.Add(N_Rectum.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), N_Rectum[0]);
                DVH_struct.Add(Rect_ant.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), Rect_ant[0]);
                DVH_struct.Add(Rect_post.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), Rect_post[0]);
                DVH_struct.Add(PRV_Rectum.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), PRV_Rectum[0]);
                DVH_struct.Add(N_Trigone.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), N_Trigone[0]);
                DVH_struct.Add(N_Urethra.FirstOrDefault(x => cureps.StructureSet.Structures.Any(s => s.Id == x)), N_Urethra[0]);
                return cureps;
            }
            else
            {

                if (select == 0)
                {
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20[0]), OptimizationObjectiveOperator.Upper, new DoseValue(39, "Gy"), 0, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20[0]), OptimizationObjectiveOperator.Lower, new DoseValue(35.2, "Gy"), 100, 120);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20[0]), OptimizationObjectiveOperator.Lower, new DoseValue(36.25, "Gy"), 98, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Upper, new DoseValue(39, "Gy"), 0, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Lower, new DoseValue(36.25, "Gy"), 100, 120);
                    //oars
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 40, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 20, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(29, "Gy"), 15, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(32, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_ant[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_post[0]), OptimizationObjectiveOperator.Upper, new DoseValue(16, "Gy"), 1, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PRV_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36.25, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 30, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36, "Gy"), 5, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(37, "Gy"), 2, 40);
                    //cureps.OptimizationSetup.AddEUDObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 1, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJL[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJR[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Urethra[0]), OptimizationObjectiveOperator.Upper, new DoseValue(38, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Trigone[0]), OptimizationObjectiveOperator.Upper, new DoseValue(38, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Penile[0]), OptimizationObjectiveOperator.Upper, new DoseValue(20, "Gy"), 30, 40);
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Colon[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(34.5, "Gy"), 0, 140);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.5, "Gy"), 10, 100);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Colon_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(37, "Gy"), 10, 90);
                    }
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Bowel[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.2, "Gy"), 0, 220);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(19.5, "Gy"), 10, 160);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Bowel_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 10, 100);
                    }
                    cureps.OptimizationSetup.AddAutomaticNormalTissueObjective(100.0f); //anade normal tissio automatico
                }
                else if (select == 1)
                {
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20[0]), OptimizationObjectiveOperator.Upper, new DoseValue(39, "Gy"), 0, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20[0]), OptimizationObjectiveOperator.Lower, new DoseValue(35.2, "Gy"), 100, 120);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20[0]), OptimizationObjectiveOperator.Lower, new DoseValue(36.25, "Gy"), 98, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Upper, new DoseValue(39, "Gy"), 0, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Lower, new DoseValue(36.25, "Gy"), 100, 120);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID21[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36.25, "Gy"), 0, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID21[0]), OptimizationObjectiveOperator.Upper, new DoseValue(28, "Gy"), 8, 50);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PTV_ID21[0]), OptimizationObjectiveOperator.Lower, new DoseValue(25, "Gy"), 100, 110);
                    //oars
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 40, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 20, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_ant[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36.25, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_post[0]), OptimizationObjectiveOperator.Upper, new DoseValue(16, "Gy"), 1, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PRV_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36.25, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 30, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36, "Gy"), 5, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(37, "Gy"), 2, 40);
                    //cureps.OptimizationSetup.AddEUDObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 1, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJL[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJR[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Urethra[0]), OptimizationObjectiveOperator.Upper, new DoseValue(38, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Trigone[0]), OptimizationObjectiveOperator.Upper, new DoseValue(38, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Penile[0]), OptimizationObjectiveOperator.Upper, new DoseValue(20, "Gy"), 30, 40);
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Colon[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(34.5, "Gy"), 0, 140);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.5, "Gy"), 10, 100);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Colon_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(37, "Gy"), 10, 90);
                    }
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Bowel[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.2, "Gy"), 0, 220);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(19.5, "Gy"), 10, 160);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Bowel_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 10, 100);
                    }
                    cureps.OptimizationSetup.AddAutomaticNormalTissueObjective(100.0f); //anade normal tissio automatico
                }
                else if (select == 2)
                {
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20_[0]), OptimizationObjectiveOperator.Upper, new DoseValue(43, "Gy"), 0, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20_[0]), OptimizationObjectiveOperator.Lower, new DoseValue(38, "Gy"), 100, 120);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20_[0]), OptimizationObjectiveOperator.Lower, new DoseValue(40, "Gy"), 98, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Upper, new DoseValue(43, "Gy"), 0, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Lower, new DoseValue(40, "Gy"), 100, 120);
                    //oars
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 40, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 20, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(36, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(40, "Gy"), 3, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_ant[0]), OptimizationObjectiveOperator.Upper, new DoseValue(38, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_post[0]), OptimizationObjectiveOperator.Upper, new DoseValue(16, "Gy"), 1, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PRV_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(40, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 30, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(40, "Gy"), 5, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(41, "Gy"), 2, 40);
                    //cureps.OptimizationSetup.AddEUDObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 1, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJL[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJR[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Urethra[0]), OptimizationObjectiveOperator.Upper, new DoseValue(42, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Trigone[0]), OptimizationObjectiveOperator.Upper, new DoseValue(42, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Penile[0]), OptimizationObjectiveOperator.Upper, new DoseValue(20, "Gy"), 30, 40);
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Colon[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(34.5, "Gy"), 0, 140);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.2, "Gy"), 10, 100);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Colon_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(37, "Gy"), 10, 90);
                    }
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Bowel[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.2, "Gy"), 0, 220);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(19.5, "Gy"), 10, 160);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Bowel_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 10, 100);
                    }
                    cureps.OptimizationSetup.AddAutomaticNormalTissueObjective(100.0f); //anade normal tissio automatico
                }
                else if (select == 3)
                {
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20_[0]), OptimizationObjectiveOperator.Upper, new DoseValue(43, "Gy"), 0, 50);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20_[0]), OptimizationObjectiveOperator.Lower, new DoseValue(38, "Gy"), 100, 120);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID20_[0]), OptimizationObjectiveOperator.Lower, new DoseValue(40, "Gy"), 98, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Upper, new DoseValue(43, "Gy"), 0, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PTV_ID17[0]), OptimizationObjectiveOperator.Lower, new DoseValue(40, "Gy"), 100, 120);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID21[0]), OptimizationObjectiveOperator.Upper, new DoseValue(38, "Gy"), 0, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id == PTV_ID21[0]), OptimizationObjectiveOperator.Upper, new DoseValue(28, "Gy"), 8, 50);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PTV_ID21[0]), OptimizationObjectiveOperator.Lower, new DoseValue(25, "Gy"), 100, 110);
                    //oars
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 40, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 20, 60);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_ant[0]), OptimizationObjectiveOperator.Upper, new DoseValue(38, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == Rect_post[0]), OptimizationObjectiveOperator.Upper, new DoseValue(16, "Gy"), 1, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == PRV_Rectum[0]), OptimizationObjectiveOperator.Upper, new DoseValue(40, "Gy"), 1, 100);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(18, "Gy"), 30, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(40, "Gy"), 5, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(41, "Gy"), 2, 40);
                    //cureps.OptimizationSetup.AddEUDObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Bladder[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 1, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJL[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_HJR[0]), OptimizationObjectiveOperator.Upper, new DoseValue(15, "Gy"), 5, 40);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Urethra[0]), OptimizationObjectiveOperator.Upper, new DoseValue(42, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Trigone[0]), OptimizationObjectiveOperator.Upper, new DoseValue(42, "Gy"), 0, 80);
                    cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => x.Id == N_Penile[0]), OptimizationObjectiveOperator.Upper, new DoseValue(20, "Gy"), 30, 40);
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Colon[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(34.5, "Gy"), 0, 140);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => N_Colon.Any(r => r == y.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.5, "Gy"), 10, 100);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Colon_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(37, "Gy"), 10, 90);
                    }
                    if (cureps.StructureSet.Structures.Any(x => x.Id == N_Bowel[0]))
                    {
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(24.2, "Gy"), 0, 220);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(x => N_Bowel.Any(s => s == x.Id)), OptimizationObjectiveOperator.Upper, new DoseValue(19.5, "Gy"), 10, 160);
                        cureps.OptimizationSetup.AddPointObjective(cureps.StructureSet.Structures.FirstOrDefault(y => y.Id.Contains("Bowel_PRV")), OptimizationObjectiveOperator.Upper, new DoseValue(27, "Gy"), 10, 100);
                    }
                    cureps.OptimizationSetup.AddAutomaticNormalTissueObjective(100.0f); //anade normal tissio automatico
                }
                return cureps;
            }
        }

        public static MessageBoxResult Question()//pregunta para paraar en mitad antes de la optimizacion
        {
            MessageBoxResult desicion = MessageBox.Show("Haces configurados, desea continuar a la optimizacion", "Warning", MessageBoxButton.YesNo,MessageBoxImage.Warning);
            return desicion;
        }
        public ExternalPlanSetup SettingPlan(ScriptContext context)
        {

            Patient patient = context.Patient;
            StructureSet sset = context.StructureSet;
            patient.BeginModifications();   // enable writing with this script.
            IEnumerable<Course> sss = patient.Courses;//lista de cursos
            Course curcourse = patient.AddCourse(); ;//creo la  clase course porque no se puede crear dentro de condicional
            //ChangeName(valores.Curso, cs: curcourse);
            ExternalPlanSetup cureps = curcourse.AddExternalPlanSetup(sset);
            //ChangeName(valores.Plan, eps: cureps);
            //set calculation model use default??? nose
            cureps.SetCalculationModel(CalculationType.PhotonVMATOptimization, valoresConfig.PVO);
            cureps.SetCalculationModel(CalculationType.DVHEstimation, valoresConfig.DVH);
            cureps.SetCalculationModel(CalculationType.PhotonVolumeDose, valoresConfig.PVD);//CalculationGridSizeInCM 
            cureps.SetCalculationOption(valoresConfig.PVD, "CalculationGridSizeInCM", valoresConfig.Grilla);
            cureps.SetCalculationOption(valoresConfig.PVD, "HeterogeneityCorrection", Convert.ToBoolean(valoresConfig.CorreccionH)? "ON":"OFF");
            cureps.SetPrescription(valoresConfig.NumeroFracciones, new DoseValue(valoresConfig.Dosis / Convert.ToDouble(valoresConfig.NumeroFracciones), "Gy"), 1.0);//prescription 0.99=99 %tratamiento sip

            //esto da en mm
            Structure ptv_total = sset.Structures.FirstOrDefault(x=>x.Id== valoresConfig.Estructura);
            VVector isocenter = new VVector(RedondeoArriba(Math.Round(ptv_total.CenterPoint.x, 0) / 10.0), RedondeoArriba(Math.Round(ptv_total.CenterPoint.y, 0) / 10.0), RedondeoArriba(Math.Round(ptv_total.CenterPoint.z, 0) / 10.0));//c es la ctte para prostata para bajar el iso
            ExternalBeamMachineParameters ebmp = new ExternalBeamMachineParameters(valoresConfig.Maquina, valoresConfig.Energia, valoresConfig.TasaDosis, valoresConfig.Tecnica, null);//SRS ARC O STATIC SE PUEDE COLOCAR CUALQUIERA QUE ESTE EN LA LISTA.

            for (int i = 1; i <= valoresConfig.ArcoNumero; i++)
            {
                if (i == 1)

                {
                    Beam VMAT1 = BeamWithMaxPostionJaws(ebmp,  true, isocenter, cureps, ptv_total);
                    VMAT1.Id = "Field" + "_CW_" + i.ToString();
                }
                else if (i == 2)
                {
                    Beam VMAT2 = BeamWithMaxPostionJaws(ebmp,  false, isocenter, cureps, ptv_total);
                    VMAT2.Id = "Field" + "_CCW_" + i.ToString();
                }
                else if (i == 3)
                {
                    Beam VMAT3 = BeamWithMaxPostionJaws(ebmp,  true, isocenter, cureps, ptv_total);
                    VMAT3.Id = "Field" + "_CW_" + i.ToString();
                }
                else if (i == 4)
                {
                    Beam VMAT4 = BeamWithMaxPostionJaws(ebmp,  true, isocenter, cureps, ptv_total);
                    VMAT4.Id = "Field" + "_CCW_" + i.ToString();
                }
                else if (i == 5)
                {
                    Beam VMAT4 = BeamWithMaxPostionJaws(ebmp,  true, isocenter, cureps, ptv_total);
                    VMAT4.Id = "Field" + "_CW_" + i.ToString();
                }
            }
            return cureps;
        }
        private double RedondeoArriba(double valor, double paso = 0.5)//redondea a 0.5
        {
            return Math.Ceiling(valor / paso) * paso * 10;
        }
        private Beam BeamWithMaxPostionJaws(ExternalBeamMachineParameters ebmp, bool gantryIsClockwise, VVector isocenter, ExternalPlanSetup eps, Structure st)
        {
            VRect<double> jawsVector = new VRect<double>(-10, -10, 10, 10);
            double[] setting_local = { valoresConfig.Colimador, valoresConfig.Inicio, valoresConfig.Fin };//LOCAL PARA CAMBIAR ANGULOS
            setting_local[2] = valoresConfig.Inicio;
            //////////////////////////////////////////ESTOS 4 BEAMS ES PARA ENCONTRAR LA MAXIMA APERTURA DE OS JAWS
            Beam beam1 = CreateArcBeamAngle(ebmp, jawsVector, setting_local, gantryIsClockwise, isocenter, eps, st);
            setting_local[1] = valoresConfig.Fin; setting_local[2] = valoresConfig.Fin;
            Beam beam2 = CreateArcBeamAngle(ebmp, jawsVector, setting_local, gantryIsClockwise, isocenter, eps, st);
            Beam beam3;
            Beam beam4;
            //todo esto para encontrar el tamano maximo para ajustar
            if (Convert.ToBoolean(valoresConfig.IsMama))
            {
                setting_local[1] = 315; setting_local[2] = 315;
                beam3 = CreateArcBeamAngle(ebmp, jawsVector, setting_local, gantryIsClockwise, isocenter, eps, st);//gantry es clcok wise true si es clock wise
                setting_local[1] = 45; setting_local[2] = 45;
                beam4 = CreateArcBeamAngle(ebmp, jawsVector, setting_local, gantryIsClockwise, isocenter, eps, st);//gantry es clcok wise true si es clock wise
            }
            else 
            {
                setting_local[1] = 120; setting_local[2] = 120;
                beam3 = CreateArcBeamAngle(ebmp, jawsVector, setting_local, gantryIsClockwise, isocenter, eps, st);//gantry es clcok wise true si es clock wise
                setting_local[1] = 220; setting_local[2] = 220;
                beam4 = CreateArcBeamAngle(ebmp, jawsVector, setting_local, gantryIsClockwise, isocenter, eps, st);//gantry es clcok wise true si es clock wise
            }
            //beam con la maxima apertura
            setting_local[1] = valoresConfig.Inicio; setting_local[2] = valoresConfig.Fin;
            Beam VMAT = CreateArcBeamAngle(ebmp, Find4Max(beam1, beam2, beam3, beam4), setting_local, gantryIsClockwise, isocenter, eps, st);
            if (valoresConfig.MLC.Contains("HD")|| valoresConfig.MLC.Contains("hd")) Jaws_corrected(VMAT);//CORRIJE LOS JAWS Y
            eps.RemoveBeam(beam1);
            eps.RemoveBeam(beam2);
            eps.RemoveBeam(beam3);
            eps.RemoveBeam(beam4);
            return VMAT;
        }
        private VRect<double> Find4Max(Beam beam1, Beam beam2, Beam beam3, Beam beam4)
        {
            double[] jawsX1Max = { Math.Abs(beam1.ControlPoints.First().JawPositions.X1), Math.Abs(beam2.ControlPoints.First().JawPositions.X1), Math.Abs(beam3.ControlPoints.First().JawPositions.X1), Math.Abs(beam4.ControlPoints.First().JawPositions.X1) };
            double[] jawsX2Max = { Math.Abs(beam1.ControlPoints.First().JawPositions.X2), Math.Abs(beam2.ControlPoints.First().JawPositions.X2), Math.Abs(beam3.ControlPoints.First().JawPositions.X2), Math.Abs(beam4.ControlPoints.First().JawPositions.X1) };
            double[] jawsY1Max = { Math.Abs(beam1.ControlPoints.First().JawPositions.Y1), Math.Abs(beam2.ControlPoints.First().JawPositions.Y1), Math.Abs(beam3.ControlPoints.First().JawPositions.Y1), Math.Abs(beam4.ControlPoints.First().JawPositions.X1) };
            double[] jawsY2Max = { Math.Abs(beam1.ControlPoints.First().JawPositions.Y2), Math.Abs(beam2.ControlPoints.First().JawPositions.Y2), Math.Abs(beam3.ControlPoints.First().JawPositions.Y2), Math.Abs(beam4.ControlPoints.First().JawPositions.X1) };
            return new VRect<double>(-jawsX1Max.Max(), -jawsY1Max.Max(), jawsX2Max.Max(), jawsY2Max.Max());
        }
        private Beam CreateArcBeamAngle(ExternalBeamMachineParameters ebmp, VRect<double> jawsVector, double[] setting_arc, bool gantryIsClockwise, VVector isocenter, ExternalPlanSetup eps, Structure st)
        {
            Beam beam = null;
            double shift = 0;
            if (setting_arc[1] == setting_arc[2]) shift = Convert.ToDouble(gantryIsClockwise) - 0.5;
            if (gantryIsClockwise)
            {
                //beam1 = eps.AddArcBeam(ebmp, jawsVector, collAngle, angleToStart, angleToStart + gantryDirectionShift, GantryDirection.Clockwise, 0, isocenter);//179
                beam = eps.AddArcBeam(ebmp, jawsVector, setting_arc[0], setting_arc[1], setting_arc[2] + shift, GantryDirection.Clockwise, 0, isocenter);
            }
            else
            {//shift es porq si doy angle to start and stop iguales me da error
                beam = eps.AddArcBeam(ebmp, jawsVector, setting_arc[0], setting_arc[2], setting_arc[1] + shift, GantryDirection.CounterClockwise, 0, isocenter);//179
            }
            if (shift != 0) beam.FitCollimatorToStructure(new FitToStructureMargins(8), st, true, true, false);//ver si 8 no genera problemas
            return beam;
        }
        public static void Jaws_corrected(Beam VMAT)
        {
            //if (!IsMama) VMAT.FitCollimatorToStructure(new FitToStructureMargins(8), ptv_total, true, true, false);//coloco el ismama para que no me cambie los colimadores x que le pasoen general siempre entra excepto en la mama
            BeamParameters beampar = VMAT.GetEditableParameters();//obtiene los beam parametros del arco 1 como ser losjaws isocentro etc
            //BEAMs
            var controlpoint = beampar.ControlPoints.ElementAt<ControlPointParameters>(0);//obtiene los control point ahi estan los jaws position 
            double X1 = controlpoint.JawPositions.X1;//posicion jaws vmat1
            double X2 = controlpoint.JawPositions.X2;
            double Y1 = controlpoint.JawPositions.Y1;
            double Y2 = controlpoint.JawPositions.Y2;
            VRect<double> jaws = new VRect<double>(X1, Y1, X2, Y2);//vrect sirve para applyparameters despues
            if (Y1 <= -107.000001)//-107.00001)
            {
                jaws = new VRect<double>(jaws.X1, -107.0, jaws.X2, jaws.Y2);
            }
            if (Y2 >= 107.000001)//107.00001)
            {
                jaws = new VRect<double>(jaws.X1, jaws.Y1, jaws.X2, 107.0);
            }
            beampar.SetJawPositions(jaws);
            VMAT.ApplyParameters(beampar);
        }








        
    }
}
