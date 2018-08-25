using Appline;
using Communication;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ApplicationClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ContextLine<SomeDataContext> line;
        private bool carefulRecursionApplineCalls = false;

        public MainWindow()
        {
            InitializeComponent();
            ApplineConfigs();
        }
        private void ApplineConfigs()
        {
            this.Closing += MainWindow_Closing;
            NotifyContext<SomeDataContext> notify = new NotifyContext<SomeDataContext>();
            this.DataContext = new SomeDataContext();
            notify.Connected += Notify_Connected;
            notify.Disconnected += Notify_Disconnected;
            notify.Timeout += Notify_Timeout;
            notify.Message += Notify_Message;
            notify.ContextChanges += Notify_ContextChanges;
            notify.Exception += Notify_Exception;
            line = LineFactory.Application<SomeDataContext>(notify, 3000, ApplicationClient.App.ARGS);
        }

        private void Notify_Exception(Exception obj)
        {
            MessageBox.Show(obj.ToString());
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                line?.Close();
            }
        }

        private void Notify_ContextChanges(SomeDataContext obj)
        {
            Dispatcher.Invoke(() =>
            {
                this.DataContext = obj;
            });
        }

        private void Notify_Message(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                if (!carefulRecursionApplineCalls)
                    this.AnyText.Text = msg;
                carefulRecursionApplineCalls = false;
            });
        }
        private void Notify_Timeout()
        {
            Dispatcher.Invoke(() =>
            {
                this.ApplineStatus.Content = "Status Timeout";
            });
        }
        private void Notify_Disconnected()
        {
            Dispatcher.Invoke(() =>
            {
                this.ApplineStatus.Content = "Status Disconnect";
            });
        }
        private void Notify_Connected()
        {
            Dispatcher.Invoke(() =>
            {
                this.ApplineStatus.Content = "Status Connect";
            });
        }
        private void AnyTextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            carefulRecursionApplineCalls = true;
            line.Send((sender as TextBox).Text);
        }
    }
}
