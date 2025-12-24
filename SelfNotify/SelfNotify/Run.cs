using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Net;


public class Run
{

    static public void info(string text)
    {

        NotifyWindow nw = new NotifyWindow();

        string Title = "";

        nw = new NotifyWindow(Title, text);
        nw.TitleClicked += new System.EventHandler(titleClick);

        nw.TextClicked += new System.EventHandler(textClick);
        nw.SetDimensions(130, 110);
        nw.Notify();
    }

    static protected void titleClick(object sender, System.EventArgs e)
    {

    }

    static protected void textClick(object sender, System.EventArgs e)
    {


    }

}
