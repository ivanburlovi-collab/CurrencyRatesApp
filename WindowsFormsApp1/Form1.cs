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
        private ListBox listBoxResults;
        private Label labelStatus;

        public Form1()
        {
            // Включаем TLS 1.2
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;

            this.Text = "💵 Курсы валют ЦБ РФ (официальный API)";
            this.Size = new Size(700, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ЗАГОЛОВОК
            labelTitle = new Label();
            labelTitle.Text = "💵 Курсы валют Центрального Банка России";
            labelTitle.Location = new Point(20, 15);
            labelTitle.Size = new Size(650, 30);
            labelTitle.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            labelTitle.ForeColor = Color.DarkBlue;

            // ДАТА
            Label labelDateText = new Label();
            labelDateText.Text = "Дата архива (ДД.ММ.ГГГГ):";
            labelDateText.Location = new Point(20, 55);
            labelDateText.Size = new Size(180, 20);
            labelDateText.Font = new Font("Microsoft Sans Serif", 9);

            textBoxDate = new TextBox();
            textBoxDate.Location = new Point(210, 52);
            textBoxDate.Size = new Size(150, 22);
            textBoxDate.Text = DateTime.Now.ToString("dd.MM.yyyy");
            textBoxDate.Font = new Font("Microsoft Sans Serif", 9);

            Label labelHint = new Label();
            labelHint.Text = "например: 25.12.2025";
            labelHint.Location = new Point(370, 55);
            labelHint.Size = new Size(120, 20);
            labelHint.Font = new Font("Microsoft Sans Serif", 8);
            labelHint.ForeColor = Color.Gray;

            // КНОПКА АРХИВА
            buttonArchive = new Button();
            buttonArchive.Text = "📅 Архивный курс";
            buttonArchive.Location = new Point(20, 90);
            buttonArchive.Size = new Size(150, 28);
            buttonArchive.BackColor = Color.LightBlue;
            buttonArchive.FlatStyle = FlatStyle.Flat;
            buttonArchive.Click += ButtonArchive_Click;

            // КНОПКА ТЕКУЩИХ КУРСОВ
            buttonCurrent = new Button();
            buttonCurrent.Text = "💰 Текущие курсы валют";
            buttonCurrent.Location = new Point(20, 130);
            buttonCurrent.Size = new Size(650, 35);
            buttonCurrent.BackColor = Color.LightGreen;
            buttonCurrent.FlatStyle = FlatStyle.Flat;
            buttonCurrent.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            buttonCurrent.Click += ButtonCurrent_Click;

            // СТАТУС
            labelStatus = new Label();
            labelStatus.Text = "✅ Готов к работе";
            labelStatus.Location = new Point(20, 180);
            labelStatus.Size = new Size(650, 20);
            labelStatus.ForeColor = Color.Green;

            // СПИСОК РЕЗУЛЬТАТОВ
            listBoxResults = new ListBox();
            listBoxResults.Location = new Point(20, 210);
            listBoxResults.Size = new Size(650, 440);
            listBoxResults.Font = new Font("Consolas", 9);
            listBoxResults.BackColor = Color.WhiteSmoke;
            listBoxResults.ScrollAlwaysVisible = true;

            // ДОБАВЛЯЕМ ВСЕ НА ФОРМУ
            this.Controls.Add(labelTitle);
            this.Controls.Add(labelDateText);
            this.Controls.Add(textBoxDate);
            this.Controls.Add(labelHint);
            this.Controls.Add(buttonArchive);
            this.Controls.Add(buttonCurrent);
            this.Controls.Add(labelStatus);
            this.Controls.Add(listBoxResults);

            // АВТОЗАГРУЗКА ПРИ СТАРТЕ
            LoadCurrentRates();
        }

        // ============================================================
        // 1️⃣ ЗАГРУЗКА ТЕКУЩИХ КУРСОВ (XML)
        // ============================================================
        private void LoadCurrentRates()
        {
            listBoxResults.Items.Clear();
            labelStatus.Text = "⏳ Загрузка...";
            labelStatus.ForeColor = Color.Blue;

            // Официальный API ЦБ РФ
            string url = "https://www.cbr.ru/scripts/XML_daily.asp";

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.Encoding = System.Text.Encoding.UTF8;

                    string xml = client.DownloadString(url);

                    // Парсим XML
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    // Получаем дату
                    string dateStr = doc.DocumentElement.GetAttribute("Date");
                    DateTime date = DateTime.ParseExact(dateStr, "dd.MM.yyyy",
                        System.Globalization.CultureInfo.InvariantCulture);

                    listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
                    listBoxResults.Items.Add("║              💵 ТЕКУЩИЕ КУРСЫ ВАЛЮТ ЦБ РФ                    ║");
                    listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");
                    listBoxResults.Items.Add($"║ Дата: {date:dd.MM.yyyy}                                          ║");
                    listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");

                    // Получаем все валюты
                    XmlNodeList valutes = doc.GetElementsByTagName("Valute");

                    // Список нужных валют
                    string[] neededCodes = { "USD", "EUR", "CNY", "GBP", "BYN", "JPY", "KZT", "UAH", "TRY", "CHF" };

                    foreach (XmlNode valute in valutes)
                    {
                        string charCode = valute.SelectSingleNode("CharCode")?.InnerText;

                        if (Array.Exists(neededCodes, code => code == charCode))
                        {
                            string name = valute.SelectSingleNode("Name")?.InnerText;
                            string value = valute.SelectSingleNode("Value")?.InnerText;
                            string nominal = valute.SelectSingleNode("Nominal")?.InnerText;

                            // Заменяем запятую на точку
                            value = value.Replace(',', '.');
                            decimal rate = decimal.Parse(value,
                                System.Globalization.CultureInfo.InvariantCulture);

                            listBoxResults.Items.Add($"║ {charCode,-3} {name,-25} {rate,12:F4} руб. ║");
                            if (!string.IsNullOrEmpty(nominal) && nominal != "1")
                            {
                                listBoxResults.Items.Add($"║    (за {nominal} единиц)                                ║");
                            }
                        }
                    }

                    listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");

                    labelStatus.Text = $"✅ Загружено: {DateTime.Now:HH:mm:ss}";
                    labelStatus.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    listBoxResults.Items.Add($"❌ ОШИБКА: {ex.Message}");
                    labelStatus.Text = "❌ Ошибка загрузки";
                    labelStatus.ForeColor = Color.Red;
                    MessageBox.Show($"Не удалось загрузить курсы:\n{ex.Message}",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ============================================================
        // 2️⃣ КНОПКА: ТЕКУЩИЕ КУРСЫ
        // ============================================================
        private void ButtonCurrent_Click(object sender, EventArgs e)
        {
            LoadCurrentRates();
        }

        // ============================================================
        // 3️⃣ КНОПКА: АРХИВНЫЙ КУРС
        // ============================================================
        private void ButtonArchive_Click(object sender, EventArgs e)
        {
            string userDate = textBoxDate.Text.Trim();

            if (string.IsNullOrEmpty(userDate))
            {
                MessageBox.Show("Введите дату в формате ДД.ММ.ГГГГ!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем формат даты
            DateTime date;
            try
            {
                date = DateTime.ParseExact(userDate, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Неверный формат!\nИспользуйте: ДД.ММ.ГГГГ\nНапример: 25.12.2025",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            listBoxResults.Items.Clear();
            labelStatus.Text = $"⏳ Загрузка {userDate}...";
            labelStatus.ForeColor = Color.Blue;
            buttonArchive.Enabled = false;

            // Официальный API ЦБ РФ с указанием даты
            string url = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={userDate}";

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.Encoding = System.Text.Encoding.UTF8;

                    string xml = client.DownloadString(url);

                    // Проверяем, есть ли данные
                    if (xml.Contains("<ValCurs") && xml.Contains("<Valute"))
                    {
                        ParseXmlAndDisplay(userDate, xml);
                        labelStatus.Text = $"✅ Архив {userDate} загружен";
                        labelStatus.ForeColor = Color.Green;
                    }
                    else
                    {
                        listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
                        listBoxResults.Items.Add($"║           ❌ ДАННЫХ ЗА {userDate} НЕТ                         ║");
                        listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");
                        listBoxResults.Items.Add("║    Возможные причины:                                          ║");
                        listBoxResults.Items.Add("║    • Выходной или праздничный день                           ║");
                        listBoxResults.Items.Add("║    • Дата слишком старая                                     ║");
                        listBoxResults.Items.Add("║    • Ошибка в формате даты                                   ║");
                        listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
                        labelStatus.Text = $"❌ Нет данных за {userDate}";
                        labelStatus.ForeColor = Color.Red;
                    }
                }
                catch (WebException ex)
                {
                    listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
                    listBoxResults.Items.Add($"║           ❌ ОШИБКА ПОДКЛЮЧЕНИЯ                              ║");
                    listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");
                    listBoxResults.Items.Add($"║    {ex.Message,-50} ║");
                    listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
                    labelStatus.Text = "❌ Ошибка подключения";
                    labelStatus.ForeColor = Color.Red;
                }
                catch (Exception ex)
                {
                    listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
                    listBoxResults.Items.Add("║           ❌ НЕИЗВЕСТНАЯ ОШИБКА                              ║");
                    listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");
                    listBoxResults.Items.Add($"║    {ex.Message,-50} ║");
                    listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
                    labelStatus.Text = "❌ Ошибка";
                    labelStatus.ForeColor = Color.Red;
                }
                finally
                {
                    buttonArchive.Enabled = true;
                }
            }
        }

        // ============================================================
        // 4️⃣ ПАРСИНГ XML И ОТОБРАЖЕНИЕ
        // ============================================================
        private void ParseXmlAndDisplay(string userDate, string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
            listBoxResults.Items.Add($"║           📅 АРХИВНЫЙ КУРС ЗА {userDate}                       ║");
            listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");

            XmlNodeList valutes = doc.GetElementsByTagName("Valute");
            string[] neededCodes = { "USD", "EUR", "CNY", "GBP", "BYN", "JPY", "KZT" };

            int found = 0;
            foreach (XmlNode valute in valutes)
            {
                string charCode = valute.SelectSingleNode("CharCode")?.InnerText;

                if (Array.Exists(neededCodes, code => code == charCode))
                {
                    string name = valute.SelectSingleNode("Name")?.InnerText;
                    string value = valute.SelectSingleNode("Value")?.InnerText;
                    string nominal = valute.SelectSingleNode("Nominal")?.InnerText;

                    value = value.Replace(',', '.');
                    decimal rate = decimal.Parse(value,
                        System.Globalization.CultureInfo.InvariantCulture);

                    listBoxResults.Items.Add($"║ {charCode,-3} {name,-25} {rate,12:F4} руб. ║");
                    if (!string.IsNullOrEmpty(nominal) && nominal != "1")
                    {
                        listBoxResults.Items.Add($"║    (за {nominal} единиц)                                ║");
                    }
                    found++;
                }
            }

            if (found == 0)
            {
                listBoxResults.Items.Add("║    ❌ ВАЛЮТЫ НЕ НАЙДЕНЫ                                  ║");
            }

            listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
        }
    }
}
