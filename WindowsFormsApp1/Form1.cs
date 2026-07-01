using System;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

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

            this.Text = "💵 Курсы валют ЦБ РФ";
            this.Size = new Size(650, 700);
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
            labelTitle.Size = new Size(600, 30);
            labelTitle.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            labelTitle.ForeColor = Color.DarkBlue;

            // ДАТА
            Label labelDateText = new Label();
            labelDateText.Text = "Дата архива (ГГГГ/ММ/ДД):";
            labelDateText.Location = new Point(20, 55);
            labelDateText.Size = new Size(180, 20);
            labelDateText.Font = new Font("Microsoft Sans Serif", 9);

            textBoxDate = new TextBox();
            textBoxDate.Location = new Point(210, 52);
            textBoxDate.Size = new Size(150, 22);
            textBoxDate.Text = DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");

            // КНОПКА АРХИВА
            buttonArchive = new Button();
            buttonArchive.Text = "📅 Архивный курс";
            buttonArchive.Location = new Point(375, 50);
            buttonArchive.Size = new Size(150, 28);
            buttonArchive.BackColor = Color.LightBlue;
            buttonArchive.FlatStyle = FlatStyle.Flat;
            buttonArchive.Click += ButtonArchive_Click;

            // КНОПКА ТЕКУЩИХ КУРСОВ
            buttonCurrent = new Button();
            buttonCurrent.Text = "💰 Текущие курсы валют";
            buttonCurrent.Location = new Point(20, 95);
            buttonCurrent.Size = new Size(505, 35);
            buttonCurrent.BackColor = Color.LightGreen;
            buttonCurrent.FlatStyle = FlatStyle.Flat;
            buttonCurrent.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
            buttonCurrent.Click += ButtonCurrent_Click;

            // СТАТУС
            labelStatus = new Label();
            labelStatus.Text = "✅ Готов к работе";
            labelStatus.Location = new Point(20, 145);
            labelStatus.Size = new Size(600, 20);
            labelStatus.ForeColor = Color.Green;

            // СПИСОК РЕЗУЛЬТАТОВ
            listBoxResults = new ListBox();
            listBoxResults.Location = new Point(20, 175);
            listBoxResults.Size = new Size(595, 480);
            listBoxResults.Font = new Font("Consolas", 9);
            listBoxResults.BackColor = Color.WhiteSmoke;
            listBoxResults.ScrollAlwaysVisible = true;

            // ДОБАВЛЯЕМ ВСЕ НА ФОРМУ
            this.Controls.Add(labelTitle);
            this.Controls.Add(labelDateText);
            this.Controls.Add(textBoxDate);
            this.Controls.Add(buttonArchive);
            this.Controls.Add(buttonCurrent);
            this.Controls.Add(labelStatus);
            this.Controls.Add(listBoxResults);

            // АВТОЗАГРУЗКА ПРИ СТАРТЕ
            LoadCurrentRates();
        }

        // ============================================================
        // 1️⃣ ЗАГРУЗКА ТЕКУЩИХ КУРСОВ (JSON API)
        // ============================================================
        private void LoadCurrentRates()
        {
            listBoxResults.Items.Clear();
            labelStatus.Text = "⏳ Загрузка...";
            labelStatus.ForeColor = Color.Blue;

            string url = "https://www.cbr-xml-daily.ru/daily_json.js";

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.Encoding = System.Text.Encoding.UTF8;

                    string json = client.DownloadString(url);
                    JObject data = JObject.Parse(json);
                    JObject valute = (JObject)data["Valute"];

                    listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
                    listBoxResults.Items.Add("║              💵 ТЕКУЩИЕ КУРСЫ ВАЛЮТ ЦБ РФ                    ║");
                    listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");

                    DateTime date = DateTime.Parse(data["Date"].ToString());
                    listBoxResults.Items.Add($"║ Дата: {date:dd.MM.yyyy HH:mm:ss}                              ║");
                    listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");

                    // ВАЛЮТЫ
                    AddCurrency(valute, "USD", "Доллар США");
                    AddCurrency(valute, "EUR", "Евро");
                    AddCurrency(valute, "CNY", "Китайский юань");
                    AddCurrency(valute, "GBP", "Фунт стерлингов");
                    AddCurrency(valute, "BYN", "Белорусский рубль");
                    AddCurrency(valute, "JPY", "Японская иена");
                    AddCurrency(valute, "KZT", "Казахстанский тенге");
                    AddCurrency(valute, "UAH", "Украинская гривна");

                    listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");

                    labelStatus.Text = $"✅ Загружено: {DateTime.Now:HH:mm:ss}";
                    labelStatus.ForeColor = Color.Green;
                }
                catch (Exception ex)
                {
                    listBoxResults.Items.Add($"❌ ОШИБКА: {ex.Message}");
                    labelStatus.Text = "❌ Ошибка загрузки";
                    labelStatus.ForeColor = Color.Red;
                }
            }
        }

        // ============================================================
        // 2️⃣ ДОБАВЛЕНИЕ ВАЛЮТЫ (с изменением)
        // ============================================================
        private void AddCurrency(JObject valute, string code, string name)
        {
            if (valute[code] != null)
            {
                decimal value = (decimal)valute[code]["Value"];
                decimal previous = (decimal)valute[code]["Previous"];
                decimal change = value - previous;
                string arrow = change > 0 ? "▲" : (change < 0 ? "▼" : "•");
                string changeStr = change > 0 ? $"+{change:F4}" : $"{change:F4}";

                listBoxResults.Items.Add($"║ {code,-3} {name,-22} {value,12:F4} руб. ║");
                listBoxResults.Items.Add($"║    Изменение: {arrow} {changeStr,-12} руб.        ║");
            }
        }

        // ============================================================
        // 3️⃣ КНОПКА: ТЕКУЩИЕ КУРСЫ
        // ============================================================
        private void ButtonCurrent_Click(object sender, EventArgs e)
        {
            LoadCurrentRates();
        }

        // ============================================================
        // 4️⃣ КНОПКА: АРХИВНЫЙ КУРС (с резервным XML API)
        // ============================================================
        private void ButtonArchive_Click(object sender, EventArgs e)
        {
            string userDate = textBoxDate.Text.Trim();

            if (string.IsNullOrEmpty(userDate))
            {
                MessageBox.Show("Введите дату в формате ГГГГ/ММ/ДД!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка формата даты
            if (!IsValidDateFormat(userDate))
            {
                MessageBox.Show("Неверный формат!\nИспользуйте: ГГГГ/ММ/ДД\nНапример: 2025/12/25",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            listBoxResults.Items.Clear();
            labelStatus.Text = $"⏳ Загрузка {userDate}...";
            labelStatus.ForeColor = Color.Blue;
            buttonArchive.Enabled = false;

            // ==========================================
            // ПЫТАЕМСЯ ЗАГРУЗИТЬ ИЗ JSON API (cbr-xml-daily.ru)
            // ==========================================
            string urlJson = $"https://www.cbr-xml-daily.ru/archive/{userDate}/daily_json.js";

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add("user-agent", "Mozilla/5.0");
                    client.Encoding = System.Text.Encoding.UTF8;

                    string json = client.DownloadString(urlJson);
                    JObject data = JObject.Parse(json);
                    JObject valute = (JObject)data["Valute"];

                    // Отображаем данные из JSON
                    DisplayArchiveDataJson(userDate, valute);
                    labelStatus.Text = $"✅ Архив {userDate} загружен (JSON)";
                    labelStatus.ForeColor = Color.Green;
                }
                catch (WebException ex)
                {
                    // ==========================================
                    // ЕСЛИ 404 — ПРОБУЕМ ОФИЦИАЛЬНЫЙ XML API ЦБ РФ
                    // ==========================================
                    if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Переформатируем дату из ГГГГ/ММ/ДД в ДД/ММ/ГГГГ
                        string[] parts = userDate.Split('/');
                        string dateFormatted = $"{parts[2]}/{parts[1]}/{parts[0]}";

                        string urlXml = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={dateFormatted}";

                        try
                        {
                            client.Headers.Add("user-agent", "Mozilla/5.0");
                            client.Encoding = System.Text.Encoding.UTF8;

                            string xml = client.DownloadString(urlXml);

                            // Проверяем, есть ли данные в XML
                            if (xml.Contains("<Valute"))
                            {
                                ParseXmlAndDisplay(userDate, xml);
                                labelStatus.Text = $"✅ Архив {userDate} загружен (XML)";
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
                        catch (Exception xmlEx)
                        {
                            listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
                            listBoxResults.Items.Add($"║           ❌ ОШИБКА ЗАГРУЗКИ АРХИВА                          ║");
                            listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");
                            listBoxResults.Items.Add($"║    {xmlEx.Message,-50} ║");
                            listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
                            labelStatus.Text = "❌ Ошибка загрузки архива";
                            labelStatus.ForeColor = Color.Red;
                        }
                    }
                    else
                    {
                        // Другая ошибка (не 404)
                        listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
                        listBoxResults.Items.Add("║           ❌ ОШИБКА ПОДКЛЮЧЕНИЯ                              ║");
                        listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");
                        listBoxResults.Items.Add($"║    {ex.Message,-50} ║");
                        listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
                        labelStatus.Text = "❌ Ошибка подключения";
                        labelStatus.ForeColor = Color.Red;
                    }
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
        // 5️⃣ ОТОБРАЖЕНИЕ АРХИВА ИЗ JSON
        // ============================================================
        private void DisplayArchiveDataJson(string userDate, JObject valute)
        {
            listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
            listBoxResults.Items.Add($"║           📅 АРХИВНЫЙ КУРС ЗА {userDate}                       ║");
            listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");

            AddCurrencyArchive(valute, "USD", "Доллар США");
            AddCurrencyArchive(valute, "EUR", "Евро");
            AddCurrencyArchive(valute, "CNY", "Китайский юань");
            AddCurrencyArchive(valute, "GBP", "Фунт стерлингов");
            AddCurrencyArchive(valute, "BYN", "Белорусский рубль");
            AddCurrencyArchive(valute, "JPY", "Японская иена");
            AddCurrencyArchive(valute, "KZT", "Казахстанский тенге");

            listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
        }

        // ============================================================
        // 6️⃣ ДОБАВЛЕНИЕ ВАЛЮТЫ (архив JSON)
        // ============================================================
        private void AddCurrencyArchive(JObject valute, string code, string name)
        {
            if (valute[code] != null)
            {
                decimal value = (decimal)valute[code]["Value"];
                listBoxResults.Items.Add($"║ {code,-3} {name,-22} {value,12:F4} руб. ║");
            }
        }

        // ============================================================
        // 7️⃣ ПАРСИНГ XML (официальный API ЦБ РФ)
        // ============================================================
        private void ParseXmlAndDisplay(string userDate, string xml)
        {
            listBoxResults.Items.Add("╔═══════════════════════════════════════════════════════════════╗");
            listBoxResults.Items.Add($"║           📅 АРХИВНЫЙ КУРС ЗА {userDate} (XML)                ║");
            listBoxResults.Items.Add("╠═══════════════════════════════════════════════════════════════╣");

            // Парсим XML вручную
            int startIndex = 0;
            int found = 0;

            while (true)
            {
                int valuteStart = xml.IndexOf("<Valute", startIndex);
                if (valuteStart == -1) break;

                int valuteEnd = xml.IndexOf("</Valute>", valuteStart);
                if (valuteEnd == -1) break;

                string valuteBlock = xml.Substring(valuteStart, valuteEnd - valuteStart + 9);

                // Извлекаем данные
                string charCode = GetTagValue(valuteBlock, "CharCode");
                string value = GetTagValue(valuteBlock, "Value");
                string name = GetTagValue(valuteBlock, "Name");

                // Показываем только нужные валюты
                if (charCode == "USD" || charCode == "EUR" || charCode == "CNY" ||
                    charCode == "GBP" || charCode == "BYN" || charCode == "JPY" ||
                    charCode == "KZT")
                {
                    // Заменяем запятую на точку и парсим
                    value = value.Replace(',', '.');
                    if (decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
                    {
                        listBoxResults.Items.Add($"║ {charCode,-3} {name,-22} {rate,12:F4} руб. ║");
                        found++;
                    }
                }

                startIndex = valuteEnd + 9;
            }

            if (found == 0)
            {
                listBoxResults.Items.Add("║    ❌ ВАЛЮТЫ НЕ НАЙДЕНЫ В ОТВЕТЕ                          ║");
            }

            listBoxResults.Items.Add("╚═══════════════════════════════════════════════════════════════╝");
        }

        // ============================================================
        // 8️⃣ ИЗВЛЕЧЕНИЕ ЗНАЧЕНИЯ ТЕГА ИЗ XML
        // ============================================================
        private string GetTagValue(string xml, string tag)
        {
            string openTag = $"<{tag}>";
            string closeTag = $"</{tag}>";

            int start = xml.IndexOf(openTag);
            if (start == -1) return "";

            start += openTag.Length;
            int end = xml.IndexOf(closeTag, start);
            if (end == -1) return "";

            return xml.Substring(start, end - start).Trim();
        }

        // ============================================================
        // 9️⃣ ПРОВЕРКА ФОРМАТА ДАТЫ
        // ============================================================
        private bool IsValidDateFormat(string date)
        {
            try
            {
                DateTime.ParseExact(date, "yyyy/MM/dd",
                    System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}