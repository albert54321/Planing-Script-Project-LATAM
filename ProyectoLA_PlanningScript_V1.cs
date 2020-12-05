using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using ProyectoLA_PlanningScript_V1;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.28")]
[assembly: AssemblyFileVersion("1.0.0.28")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context, System.Windows.Window window /*, ScriptEnvironment environment*/)
        {
            Licence();
            if (context.Patient == null || context.StructureSet == null || context.Image == null)
            {
                MessageBox.Show(
                    "Por favor cargue un paciente, imagen 3D, y conjunto de estructuras antes de ejecutar esta aplicación",
                    "ProyectoLA", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
                return;
            }
            context.Patient.BeginModifications();
            var MainWindow = new MainView(context);
            window.Content = MainWindow;
            window.Height = MainWindow.Height;
            window.Width = MainWindow.Width;
            window.Title = "ProyectoLA Planificación";
        }
        private void Licence()
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            List<string> users = new List<string> { @"SDE\alberto", @"SDE\alejandro", @"SDE\federico", @"SDE\nicolas", @"SDE\wtrinca", @"SDE\milton", @"SDE\MILTON" };
            if (!users.Any(x => x == userName))
            {
                MessageBox.Show("No esta autorizado para Ejecutar este Plug-In. \nPongase en contacto con el autor: alarcon.alberto01@gmail.com");
                System.Windows.Application.Current.Shutdown();
                return;
            }
        }
    }
}
