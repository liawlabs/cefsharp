using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

//https://ourcodeworld.com/articles/read/173/how-to-use-cefsharp-chromium-embedded-framework-csharp-in-a-winforms-application#:~:text=To%20add%20CefSharp%2C%20go%20to,WinForms%20distributtion%20and%20install%20it.&text=Follow%20the%20installation%20setup%20(accept%20therms%20and%20install).
//https://www.youtube.com/watch?v=fOzBVy-sDbM
//https://github.com/cefsharp/CefSharp
//https://getbootstrap.com/docs/5.3/getting-started/introduction/
//https://grantwinney.com/hosting-a-simple-webpage-in-winforms-with-cefsharp/

namespace CEFSharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            browser.KeyboardHandler = new CustomKeyboardHandler(txtName, dtp);

            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.JavascriptMessageReceived += Browser_JavascriptMessageReceived;


            browser.LoadUrl($@"{Application.StartupPath}\index.html");
        }

        private void Browser_JavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {
            var message = ((IDictionary<string, object>)e.Message).Single();
            switch (message.Key)
            {
                case "appr":
                    dtp.Invoke(new Action(() =>
                        dtp.Value = DateTime.TryParse(message.Value.ToString(), out var apprvDate) ? apprvDate : DateTime.Now
                    ));
                    break;
                case "app":
                    cbApp.Invoke(new Action(() =>
                    {
                        if (message.Value.ToString() != "")
                            cbApp.Text = message.Value.ToString();
                        else
                            cbApp.SelectedIndex = -1;
                    }));
                    break;
            }
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                browser.ExecuteScriptAsync(@"
                let app = document.getElementById('application');
                app.addEventListener('change', (e) => {
                    CefSharp.PostMessage({app: application.options[application.selectedIndex].text});
                });

                let apprv = document.getElementById('approval');
                apprv.addEventListener('input', (e) => {
                    CefSharp.PostMessage({apprv: approval.value});
                });
            ");
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            browser.ExecuteScriptAsync($"document.getElementById('name').value = '{txtName.Text}';");
        }

        private void dtp_ValueChanged(object sender, EventArgs e)
        {
            browser.ExecuteScriptAsync($"document.getElementById('approval').value = '{dtp.Value:yyyy-MM-ddThh:mm}';");
        }

        private void cbApp_SelectedIndexChanged(object sender, EventArgs e)
        {
            browser.ExecuteScriptAsync($"document.getElementById('application').value = '{cbApp.Text}';");
        }
    }
}

public class CustomKeyboardHandler : IKeyboardHandler
{
    private readonly TextBox txtName;
    private readonly DateTimePicker dtp;

    public CustomKeyboardHandler(TextBox txtName, DateTimePicker dtp)
    {
        this.txtName = txtName;
        this.dtp = dtp;
    }

    public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
    {
        chromiumWebBrowser.EvaluateScriptAsync("document.getElementById('name').value;")
                            .ContinueWith(x => txtName.Invoke(new Action(() =>
                            {
                                txtName.Text = Convert.ToString(x.Result.Result);
                            })));

        chromiumWebBrowser.EvaluateScriptAsync("document.getElementById('approval').value;")
                            .ContinueWith(x => dtp.Invoke(new Action(() =>
                            {
                                dtp.Value = DateTime.TryParse(x.Result.Result.ToString(), out var aprvDate) ? aprvDate : DateTime.Now;
                            })));

        return true;
    }

    public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
    {
        // If you return true here, the keyboard event is considered 'handled', and OnKeyEvent will not fire.
        return false;
    }
}
