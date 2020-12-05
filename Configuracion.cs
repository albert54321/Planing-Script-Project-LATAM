using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoLA_PlanningScript_V1
{
    public class Configuracion
    {
        public string Template { get; set; }
        public string Maquina { get; set; }
        public string Energia { get; set; }
        public string MLC { get; set; }
        public string Curso { get; set; }
        public string Plan { get; set; }
        public string PVD { get; set; }
        public string PVO { get; set; }
        public string DVH { get; set; }
        public bool? CorreccionH { get; set; }
        public bool? ArcoCompleto { get; set; }
        public double Inicio { get; set; }
        public double Fin { get; set; }
        public string Grilla { get; set; }
        public int ArcoNumero { get; set; }
        public string Estructura { get; set; }
        public double Colimador { get; set; }
        public int NumeroFracciones { get; set; }
        public Double Dosis { get; set; }
        public string Modelo { get; set; }
        public int TasaDosis { get; set; }
        public string Tecnica { get; set; }
        public bool? IsMama { get; set; }
        public bool? IsRP { get; set; }
    }
    public class BaseDeDatos<T>
    {
        public List<T> valores = new List<T>();
        public string ruta;
        public BaseDeDatos(string r)
        {
            this.ruta = r;
        }
        public void Guardar()
        {
            string texto = JsonConvert.SerializeObject(valores);
            File.WriteAllText(ruta, texto);
        }

        public void Cargar()
        {
            try
            {
                string archivo = File.ReadAllText(ruta);
                valores = JsonConvert.DeserializeObject<List<T>>(archivo);
            }
            catch (Exception)
            {
            }
        }

        public void Insertar(T nuevo)
        {
            valores.Add(nuevo);
            Guardar();
        }
        public List<T> Buscar(Func<T, bool> criterio)
        {
            return valores.Where(criterio).ToList();
        }
        public void Eliminar(Func<T, bool> criterio)
        {
            valores = valores.Where(x => !criterio(x)).ToList();
            Guardar();
        }
        public void Actualizar(Func<T, bool> criterio, T nuevo)
        {
            valores = valores.Select(x =>
            {
                if (criterio(x)) x = nuevo;
                return x;
            }
            ).ToList();
            Guardar();
        }
    }
    public class BaseDeDatosStructures<T>
    {
        public List<ObservableCollection<T>> valores = new List<ObservableCollection<T>>();
        public string ruta;
        public BaseDeDatosStructures(string r)
        {
            this.ruta = r;
        }
        public void Guardar()
        {
            string texto = JsonConvert.SerializeObject(valores);
            File.WriteAllText(ruta, texto);
        }

        public void Cargar()
        {
            try
            {
                string archivo = File.ReadAllText(ruta);
                valores = JsonConvert.DeserializeObject<List<ObservableCollection<T>>>(archivo);
            }
            catch (Exception)
            {
            }
        }

        public void Insertar(ObservableCollection<T> nuevo)
        {
            valores.Add(nuevo);
            Guardar();
        }
        public List<ObservableCollection<T>> Buscar(Func<ObservableCollection<T>, bool> criterio)
        {
            return valores.Where(criterio).ToList();
        }
        public void Eliminar(Func<ObservableCollection<T>, bool> criterio)
        {
            valores = valores.Where(x => !criterio(x)).ToList();
            Guardar();
        }
        public void Actualizar(Func<ObservableCollection<T>, bool> criterio, ObservableCollection<T> nuevo)
        {
            valores = valores.Select(x =>
            {
                if (criterio(x)) x = nuevo;
                return x;
            }
            ).ToList();
            Guardar();
        }
    }
    public static class Extensions//hermosa extension de linq
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> col)
        {
            return new ObservableCollection<T>(col);
        }
    }

    public class BoolStringClass
    {
        public string TheText { get; set; }
        public bool IsSelected { get; set; }
        public string Template { get; set; }
        public string Dose { get; set; }
        public string DoseOAR { get; set; }
        public string VolumenOAR { get; set; }
        public string Prioridad { get; set; }
    }
}

