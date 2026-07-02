using System;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Label labelTitle;
        private Label labelDate;
        private TextBox textBoxDate;
        private Button buttonCurrent;
        private Button buttonArchive;
        private Button buttonAbout;
        private ListBox listBoxResults;
        private Label labelStatus;

        public Form1()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;

            this.Text = "Currency Rates - Central Bank of Russia";
            this.Size = new Size(750, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            labelTitle = new Label();
            labelTitle.Text = "Currency Rates - Central Bank of Russia";
            labelTitle.Location = new Point(20, 15);
            labelTitle.Size = new Size(700, 30);
            labelTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            labelTitle.ForeColor = Color.DarkBlue;

            Label labelDateText = new Label();
            labelDateText.Text = "Archive Date (DD.MM.YYYY):";
            labelDateText.Location = new Point(20, 55);
            labelDateText.Size = new Size(180, 20);
            labelDateText.Font = new Font("Segoe UI", 9);

            textBoxDate = new TextBox();
            textBoxDate.Location = new Point(210, 52);
            textBoxDate.Size = new Size(150, 22);
            textBoxDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
            textBoxDate.Font = new Font("Segoe UI", 9);

            Label labelHint = new Label();
            labelHint.Text = "example: 25.12.2025";
            labelHint.Location = new Point(370, 55);
            labelHint.Size = new Size(120, 20);
            labelHint.Font = new Font("Segoe UI", 8);
            labelHint.ForeColor = Color.Gray;

            buttonArchive = new Button();
            buttonArchive.Text = "Archive Rate";
            buttonArchive.Location = new Point(20, 90);
            buttonArchive.Size = new Size(150, 28);
            buttonArchive.BackColor = Color.LightBlue;
            buttonArchive.FlatStyle = FlatStyle.Flat;
            buttonArchive.Click += ButtonArchive_Click;

            buttonAbout = new Button();
            buttonAbout.Text = "About";
            buttonAbout.Location = new Point(180, 90);
            buttonAbout.Size = new Size(100, 28);
            buttonAbout.BackColor = Color.LightYellow;
            buttonAbout.FlatStyle = FlatStyle.Flat;
            buttonAbout.Click += (s, e) => {
                MessageBox.Show(
                    "Currency Rates - Central Bank of Russia\n" +
                    "Version: 1.0\n" +
                    "Developer: Your Name\n" +
                    "Group: IT-101\n" +
                    "June 2026",
                    "About",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };

            buttonCurrent = new Button();
            buttonCurrent.Text = "Current Rates";
            buttonCurrent.Location = new Point(20, 130);
            buttonCurrent.Size = new Size(700, 35);
            buttonCurrent.BackColor = Color.LightGreen;
            buttonCurrent.FlatStyle = FlatStyle.Flat;
            buttonCurrent.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            buttonCurrent.Click += ButtonCurrent_Click;

            labelStatus = new Label();
            labelStatus.Text = "Ready";
            labelStatus.Location = new Point(20, 180);
            labelStatus.Size = new Size(700, 20);
            labelStatus.Font = new Font("Segoe UI", 9);
            labelStatus.ForeColor = Color.Green;

            listBoxResults = new ListBox();
            listBoxResults.Location = new Point(20, 210);
            listBoxResults.Size = new Size(700, 440);
            listBoxResults.Font = new Font("Segoe UI", 9);
            listBoxResults.BackColor = Color.WhiteSmoke;
            listBoxResults.ScrollAlwaysVisible = true;

            this.Controls.Add(labelTitle);
            this.Controls.Add(labelDateText);
            this.Controls.Add(textBoxDate);
            this.Controls.Add(labelHint);
            this.Controls.Add(buttonArchive);
            this.Controls.Add(buttonAbout);
            this.Controls.Add(buttonCurrent);
            this.Controls.Add(labelStatus);
            this.Controls.Add(listBoxResults);

            LoadCurrentRates();
        }

        private void LoadCurrentRates()
        {
            listBoxResults.Items.Clear();
            labelStatus.Text = "Loading...";
            labelStatus.ForeColor = Color.Blue;

            string url = "https://www.cbr.ru/scripts/XML_daily.asp";

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.Encoding = System.Text.Encoding.UTF8;

                    string xml = client.DownloadString(url);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    string dateStr = doc.DocumentElement.GetAttribute("Date");
                    DateTime date = DateTime.ParseExact(dateStr, "dd.MM.yyyy",
                        System.Globalization.CultureInfo.InvariantCulture);

                    listBoxResults.Items.Add("============================================================");
                    listBoxResults.Items.Add("         CURRENT RATES - CENTRAL BANK OF RUSSIA");
                    listBoxResults.Items.Add("============================================================");
                    listBoxResults.Items.Add($"Date: {date:dd.MM.yyyy}");
                    listBoxResults.Items.Add("------------------------------------------------------------");

                    XmlNodeList valutes = doc.GetElementsByTagName("Valute");
                    string[] neededCodes = { "USD", "EUR", "CNY", "GBP", "BYN", "JPY", "KZT", "UAH", "TRY", "CHF" };

                    foreach (XmlNode valute in valutes)
                    {
                        string charCode = valute.SelectSingleNode("CharCode")?.InnerText;

                        if (Array.Exists(neededCodes, code => code == charCode))
                        {
                            string value = valute.SelectSingleNode("Value")?.InnerText;
                            string nominal = valute.SelectSingleNode("Nominal")?.InnerText;

                            value = value.Replace(',', '.');
                            decimal rate = decimal.Parse(value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            string line = $"{charCode,-5} {rate,12:F4} RUB";

                            if (!string.IsNullOrEmpty(nominal) && nominal != "1")
                            {
                                line += $" (per {nominal} units)";
                            }

                            listBoxResults.Items.Add(line);
                        }
                    }

                    listBoxResults.Items.Add("============================================================");

                    // ============================================================
                    // ЭКСПЕРИМЕНТАЛЬНАЯ ФУНКЦИЯ: ПОДСЧЕТ ВАЛЮТ
                    // ============================================================
                    listBoxResults.Items.Add($"TOTAL CURRENCIES: {neededCodes.Length}");

                    labelStatus.Text = $"Loaded: {DateTime.Now:HH:mm:ss}";
                    labelStatus.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    listBoxResults.Items.Add($"ERROR: {ex.Message}");
                    labelStatus.Text = "Error loading";
                    labelStatus.ForeColor = Color.Red;
                    MessageBox.Show($"Failed to load rates:\n{ex.Message}",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ButtonCurrent_Click(object sender, EventArgs e)
        {
            LoadCurrentRates();
        }

        private void ButtonArchive_Click(object sender, EventArgs e)
        {
            string userDate = textBoxDate.Text.Trim();

            if (string.IsNullOrEmpty(userDate))
            {
                MessageBox.Show("Enter date in DD.MM.YYYY format!", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime date;
            try
            {
                date = DateTime.ParseExact(userDate, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Invalid format!\nUse: DD.MM.YYYY\nExample: 25.12.2025",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            listBoxResults.Items.Clear();
            labelStatus.Text = $"Loading {userDate}...";
            labelStatus.ForeColor = Color.Blue;
            buttonArchive.Enabled = false;

            string url = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={userDate}";

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.Encoding = System.Text.Encoding.UTF8;

                    string xml = client.DownloadString(url);

                    if (xml.Contains("<ValCurs") && xml.Contains("<Valute"))
                    {
                        ParseXmlAndDisplay(userDate, xml);
                        labelStatus.Text = $"Archive {userDate} loaded";
                        labelStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        listBoxResults.Items.Add("============================================================");
                        listBoxResults.Items.Add($"           NO DATA FOR {userDate}");
                        listBoxResults.Items.Add("============================================================");
                        listBoxResults.Items.Add("Possible reasons:");
                        listBoxResults.Items.Add("- Weekend or holiday");
                        listBoxResults.Items.Add("- Date is too old");
                        listBoxResults.Items.Add("- Invalid date format");
                        listBoxResults.Items.Add("============================================================");
                        labelStatus.Text = $"No data for {userDate}";
                        labelStatus.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex)
                {
                    listBoxResults.Items.Add("============================================================");
                    listBoxResults.Items.Add("           CONNECTION ERROR");
                    listBoxResults.Items.Add("============================================================");
                    listBoxResults.Items.Add(ex.Message);
                    listBoxResults.Items.Add("============================================================");
                    labelStatus.Text = "Connection error";
                    labelStatus.ForeColor = Color.Red;
                }
                finally
                {
                    buttonArchive.Enabled = true;
                }
            }
        }

        private void ParseXmlAndDisplay(string userDate, string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            listBoxResults.Items.Add("============================================================");
            listBoxResults.Items.Add($"         ARCHIVE RATE FOR {userDate}");
            listBoxResults.Items.Add("============================================================");

            XmlNodeList valutes = doc.GetElementsByTagName("Valute");
            string[] neededCodes = { "USD", "EUR", "CNY", "GBP", "BYN", "JPY", "KZT" };

            int found = 0;

            foreach (XmlNode valute in valutes)
            {
                string charCode = valute.SelectSingleNode("CharCode")?.InnerText;

                if (Array.Exists(neededCodes, code => code == charCode))
                {
                    string value = valute.SelectSingleNode("Value")?.InnerText;
                    string nominal = valute.SelectSingleNode("Nominal")?.InnerText;

                    value = value.Replace(',', '.');
                    decimal rate = decimal.Parse(value,
                        System.Globalization.CultureInfo.InvariantCulture);

                    string line = $"{charCode,-5} {rate,12:F4} RUB";

                    if (!string.IsNullOrEmpty(nominal) && nominal != "1")
                    {
                        line += $" (per {nominal} units)";
                    }

                    listBoxResults.Items.Add(line);
                    found++;
                }
            }

            if (found == 0)
            {
                listBoxResults.Items.Add("CURRENCIES NOT FOUND");
            }

            listBoxResults.Items.Add("============================================================");

            // ============================================================
            // ЭКСПЕРИМЕНТАЛЬНАЯ ФУНКЦИЯ: ПОДСЧЕТ ВАЛЮТ В АРХИВЕ
            // ============================================================
            listBoxResults.Items.Add($"TOTAL CURRENCIES IN ARCHIVE: {found}");

            listBoxResults.Items.Add("============================================================");
        }
    }
}