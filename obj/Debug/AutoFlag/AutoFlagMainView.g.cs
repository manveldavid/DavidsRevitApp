﻿#pragma checksum "..\..\..\AutoFlag\AutoFlagMainView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "C56C122157CAD786AAF4F79B6FBD60CABC9014D0279595A1E694A72814BAFE87"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using DavidsRevitApp.AutoFlag;
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


namespace DavidsRevitApp.AutoFlag {
    
    
    /// <summary>
    /// AutoFlagMainView
    /// </summary>
    public partial class AutoFlagMainView : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CreateFlagBtn;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SelectFlagBtn;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ParseFlagBtn;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button DirectionFlagBtn;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox ConstructionCheckBox;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button WatchConstructionIdsBtn;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label SelectAutoFlagLable;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image SelectAutoFlagImage;
        
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
            System.Uri resourceLocater = new System.Uri("/DavidsRevitApp;component/autoflag/autoflagmainview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
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
            this.CreateFlagBtn = ((System.Windows.Controls.Button)(target));
            
            #line 9 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
            this.CreateFlagBtn.Click += new System.Windows.RoutedEventHandler(this.CreateFlagBtn_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.SelectFlagBtn = ((System.Windows.Controls.Button)(target));
            
            #line 10 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
            this.SelectFlagBtn.Click += new System.Windows.RoutedEventHandler(this.SelectFlagBtn_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ParseFlagBtn = ((System.Windows.Controls.Button)(target));
            
            #line 11 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
            this.ParseFlagBtn.Click += new System.Windows.RoutedEventHandler(this.ParseFlagBtn_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.DirectionFlagBtn = ((System.Windows.Controls.Button)(target));
            
            #line 12 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
            this.DirectionFlagBtn.Click += new System.Windows.RoutedEventHandler(this.DirectionFlagBtn_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ConstructionCheckBox = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 6:
            this.WatchConstructionIdsBtn = ((System.Windows.Controls.Button)(target));
            
            #line 14 "..\..\..\AutoFlag\AutoFlagMainView.xaml"
            this.WatchConstructionIdsBtn.Click += new System.Windows.RoutedEventHandler(this.WatchConstructionIdsBtn_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.SelectAutoFlagLable = ((System.Windows.Controls.Label)(target));
            return;
            case 8:
            this.SelectAutoFlagImage = ((System.Windows.Controls.Image)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

