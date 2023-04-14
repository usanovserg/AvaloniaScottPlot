using Avalonia.Controls;
using AvaloniaApplication1.Entity;
using ScottPlot;
using ScottPlot.Avalonia;
using System.Collections.Generic;
using System.Timers;
using System;
using Avalonia.Input;
using AvaloniaApplication1.ViewModels;
using System.Diagnostics;

namespace AvaloniaApplication1.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            

            DataContext = _vm;

            AScrollBar.AddHandler(PointerPressedEvent, MouseDownHandler, handledEventsToo: true);
            AScrollBar.AddHandler(PointerReleasedEvent, MouseUpHandler, handledEventsToo: true);
        }

        

        VM _vm = new VM();

        private void MouseDownHandler(object sender, PointerPressedEventArgs e)
        {
            _vm.IsPressedScrollBar = true;
        }

        private void MouseUpHandler(object sender, PointerReleasedEventArgs e)
        {
            _vm.IsPressedScrollBar = false;
        }
    }
}
