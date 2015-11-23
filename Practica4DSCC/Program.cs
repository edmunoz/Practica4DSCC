using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

//Estas referencias son necesarias para usar GLIDE
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;

namespace Practica4DSCC
{
    public partial class Program
    {
        //Objetos de interface gráfica GLIDE
        private GHI.Glide.Display.Window iniciarWindow;
        private GHI.Glide.Display.Window temperatura;
        private Button btn_inicio;
        private TextBlock text;
        private TextBlock temp;
        private Slider sliderBar;
        
        private GT.Timer timer;
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            timer = new GT.Timer(20000); // every second (1000ms)
            timer.Tick += timer_Tick;

            

            //Ethernet
            this.ethernetJ11D.NetworkInterface.Open();
            this.ethernetJ11D.NetworkInterface.EnableDhcp();
            this.ethernetJ11D.UseThisNetworkInterface();
            this.ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            this.ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;

  

            //Carga la ventana principal
            iniciarWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.inicioWindow));
            temperatura = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.lecturaTemperatura));
            GlideTouch.Initialize();

            //Inicializa el boton en la interface
            btn_inicio = (Button)iniciarWindow.GetChildByName("button_iniciar");
            btn_inicio.TapEvent += btn_inicio_TapEvent;

            //Selecciona iniciarWindow como la ventana de inicio
            Glide.MainWindow = iniciarWindow;

            text = (TextBlock)Glide.MainWindow.GetChildByName("text_net_status");
                        
        }

        void timer_Tick(GT.Timer timer)
        {
            HttpRequest request = HttpHelper.CreateHttpGetRequest("http://api.thingspeak.com/channels/46434/fields/2/last");
            request.ResponseReceived += request_ResponseReceived;  
            request.SendRequest();
            Debug.Print("SendRequest");
        }

        void request_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            var x = response.Text;
            Debug.Print(x);
            temp = (TextBlock)Glide.MainWindow.GetChildByName("texto_temperatura");
            temp.Text = x;
            sliderBar = (Slider)Glide.MainWindow.GetChildByName("slider");
            sliderBar.Value = Convert.ToDouble(x);
            Glide.MainWindow = temperatura;
        }

        void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            
            this.btn_inicio.Enabled = true;
            text.Text = ethernetJ11D.NetworkSettings.IPAddress;
            Glide.MainWindow = iniciarWindow;
            
            
            Debug.Print("Activado");
        }

        void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            timer.Stop();
            this.btn_inicio.Enabled = false;
            text.Text = "No Network";
            Glide.MainWindow = iniciarWindow;
            Debug.Print("Desactivado");
        }

        void btn_inicio_TapEvent(object sender)
        {

            Glide.MainWindow = temperatura;
            timer.Start();
            Debug.Print("Iniciar");
            


        }
    }
}
