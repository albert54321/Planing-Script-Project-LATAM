﻿#pragma checksum "..\..\..\MainView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "F3BF6CE6C1A546297ECFC0A74150057A4CFCE9FCC4C3CE29FB28BBA5D4850B91"
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

using ProyectoLA_PlanningScript_V1;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ProyectoLA_PlanningScript_V1 {
    
    
    /// <summary>
    /// MainView
    /// </summary>
    public partial class MainView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 39 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox PatientName;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox PatientID;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox StructSet;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox CT;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Apply;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Close;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Credits;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtConfiguration;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CbTemplate;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\MainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox Selected;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ProyectoLA_PlanningScript_V2.esapi;component/mainview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.PatientName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.PatientID = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.StructSet = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.CT = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.Apply = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\..\MainView.xaml"
            this.Apply.Click += new System.Windows.RoutedEventHandler(this.Apply_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.Close = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\..\MainView.xaml"
            this.Close.Click += new System.Windows.RoutedEventHandler(this.Close_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.Credits = ((System.Windows.Controls.Button)(target));
            
            #line 49 "..\..\..\MainView.xaml"
            this.Credits.Click += new System.Windows.RoutedEventHandler(this.Credits_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.BtConfiguration = ((System.Windows.Controls.Button)(target));
            
            #line 50 "..\..\..\MainView.xaml"
            this.BtConfiguration.Click += new System.Windows.RoutedEventHandler(this.BtConfiguration_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.CbTemplate = ((System.Windows.Controls.ComboBox)(target));
            
            #line 58 "..\..\..\MainView.xaml"
            this.CbTemplate.MouseEnter += new System.Windows.Input.MouseEventHandler(this.CbTemplate_MouseEnter);
            
            #line default
            #line hidden
            
            #line 58 "..\..\..\MainView.xaml"
            this.CbTemplate.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CB1_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 10:
            this.Selected = ((System.Windows.Controls.TextBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

