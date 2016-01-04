using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace RssReader
{
    public partial class Form1 : Form
    {
        public string xml = @"PodCast.xml";

        public Form1()
        {
            InitializeComponent();


            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), xml);

            XElement root = XElement.Load(path);

            List<podCast> links = new List<podCast>();

            links = (from el in root.Elements("Podcast")
                     select (new podCast
                    {
                        nome = (string)el.Element("nome"),
                        link = (string)el.Element("link")
                    })).ToList();


            comboBox1.DataSource = new BindingSource(links, null);
            comboBox1.DisplayMember = "nome";
            comboBox1.ValueMember = "link";

            comboBox1.Update();
        }

        public void carregaCombo()
        {
            DataSet ds = new DataSet();

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), xml);

            ds.ReadXml(path);

            dgPodCast.DataSource = ds.Tables[0];

            Dictionary<string, string> List = new Dictionary<string, string>();

            foreach (DataRow item in ds.Tables[0].Rows)
            {
                List.Add(item[0].ToString().Trim(), item[1].ToString().Trim());
            }



            comboBox1.DataSource = new BindingSource(List, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            comboBox1.Update();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {

            string value = ((RssReader.podCast)(comboBox1.SelectedItem)).link;

            List<arquivo> list = new List<arquivo>();
            string url = value;

            //https://msdn.microsoft.com/pt-br/library/system.xml.xmlreader.create(v=vs.110).aspx
            XmlReader reader = XmlReader.Create(url);

            //https://msdn.microsoft.com/pt-br/library/bb515814(v=vs.110).aspx
            SyndicationFeed feed = SyndicationFeed.Load(reader);

            reader.Close();

            foreach (SyndicationItem item in feed.Items)
            {
                foreach (var link in item.Links.ToList().Where(x => x.MediaType == "audio/mpeg"))
                {
                    if (link.Uri != null)
                    {
                        list.Add(new arquivo
                        {
                            subject = item.Title != null ? item.Title.Text : string.Empty,
                            summary = item.Summary != null ? item.Summary.Text : string.Empty,
                            dataPublicacao = item.PublishDate != null ? item.PublishDate.ToString("dd/MM/yyyy HH:mm") : DateTime.MinValue.ToString("dd/MM/yyyy HH:mm"),
                            PodCast = link.Uri.ToString()
                        });
                    }

                }

            }

            XDocument xDoc = XDocument.Load(xml);

            foreach (arquivo item in list)
            {
                XElement srcTree = new XElement("arquivo",
                          new XElement("subject", item.subject),
                          new XElement("summary", item.summary),
                          new XElement("dataPublicacao", item.dataPublicacao),
                          new XElement("PodCast", item.PodCast)
                          );
            }



            dataGridView1.DataSource = list;

            axWindowsMediaPlayer1.URL = "";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string boga = string.Empty;
            boga = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();

            if (!string.IsNullOrEmpty(boga))
            {
                //https://msdn.microsoft.com/pt-br/library/windows/desktop/dd562470(v=vs.85).aspx
                axWindowsMediaPlayer1.URL = boga;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }



    }

    public class arquivo
    {
        public string subject { get; set; }
        public string summary { get; set; }
        public string dataPublicacao { get; set; }
        public string PodCast { get; set; }
    }
    public class podCast
    {
        public podCast()
        {
            arquivos = new List<arquivo>();
        }
        public string nome { get; set; }
        public string link { get; set; }
        public List<arquivo> arquivos { get; set; }
    }
}
