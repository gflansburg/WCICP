using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCtrlICP
{
    public partial class LicenseDlg : Form
    {
        public LicenseDlg() : base()
        {
            InitializeComponent();
            lblLicense.Text = GetEmbededResource(Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty, "LICENSE");
        }

        static public string GetEmbededResource(string path, string name)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            if (thisAssembly != null)
            {
                using (Stream? stream = thisAssembly != null ? thisAssembly.GetManifestResourceStream(path + "." + name) : null)
                {
                    if (stream != null)
                    {
                        using (TextReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            return string.Empty;
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
