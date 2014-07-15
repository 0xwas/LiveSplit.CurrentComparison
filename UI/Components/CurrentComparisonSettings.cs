﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using Fetze.WinFormsColor;
using LiveSplit.TimeFormatters;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using LiveSplit.Options;

namespace LiveSplit.UI.Components
{
    public partial class CurrentComparisonSettings : UserControl
    {
        public Color TextColor { get; set; }
        public bool OverrideTextColor { get; set; }
        public Color TimeColor { get; set; }
        public bool OverrideTimeColor { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }
        public String GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public Font Font1 { get; set; }
        public String Font1String { get { return String.Format("{0} {1}", Font1.FontFamily.Name, Font1.Style); } }
        public bool OverrideFont1 { get; set; }
        public Font Font2 { get; set; }
        public String Font2String { get { return String.Format("{0} {1}", Font2.FontFamily.Name, Font2.Style); } }
        public bool OverrideFont2 { get; set; }

        public bool Display2Rows { get; set; }

        public LayoutMode Mode { get; set; }

        public LiveSplitState CurrentState { get; set; }

        public CurrentComparisonSettings()
        {
            InitializeComponent();

            TextColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            TimeColor = Color.FromArgb(255, 255, 255);
            OverrideTimeColor = false;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;
            OverrideFont1 = false;
            OverrideFont2 = false;
            Font1 = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            Font2 = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            Display2Rows = false;

            chkOverrideTextColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTextColor.DataBindings.Add("BackColor", this, "TextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideTimeColor.DataBindings.Add("Checked", this, "OverrideTimeColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTimeColor.DataBindings.Add("BackColor", this, "TimeColor", false, DataSourceUpdateMode.OnPropertyChanged);
            lblFont.DataBindings.Add("Text", this, "Font1String", false, DataSourceUpdateMode.OnPropertyChanged);
            lblFont2.DataBindings.Add("Text", this, "Font2String", false, DataSourceUpdateMode.OnPropertyChanged);

            cmbGradientType.SelectedIndexChanged += cmbGradientType_SelectedIndexChanged;
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
            chkFont.DataBindings.Add("Checked", this, "OverrideFont1", false, DataSourceUpdateMode.OnPropertyChanged);
            chkFont2.DataBindings.Add("Checked", this, "OverrideFont2", false, DataSourceUpdateMode.OnPropertyChanged);
            this.Load += TextComponentSettings_Load;

            chkOverrideTextColor.CheckedChanged += chkOverrideTextColor_CheckedChanged;
            chkOverrideTimeColor.CheckedChanged += chkOverrideTimeColor_CheckedChanged;
            chkFont.CheckedChanged += chkFont_CheckedChanged;
            chkFont2.CheckedChanged += chkFont2_CheckedChanged;
        }

        void chkFont2_CheckedChanged(object sender, EventArgs e)
        {
            label7.Enabled = lblFont2.Enabled = btnFont2.Enabled = chkFont2.Checked;
        }

        void chkFont_CheckedChanged(object sender, EventArgs e)
        {
            label5.Enabled = lblFont.Enabled = btnFont.Enabled = chkFont.Checked;
        }

        void chkOverrideTimeColor_CheckedChanged(object sender, EventArgs e)
        {
            label2.Enabled = btnTimeColor.Enabled = chkOverrideTimeColor.Checked;
        }

        void chkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = btnTextColor.Enabled = chkOverrideTextColor.Checked;
        }

        void TextComponentSettings_Load(object sender, EventArgs e)
        {
            chkOverrideTextColor_CheckedChanged(null, null);
            chkOverrideTimeColor_CheckedChanged(null, null);
            chkFont_CheckedChanged(null, null);
            chkFont2_CheckedChanged(null, null);

            if (Mode == LayoutMode.Horizontal)
            {
                chkTwoRows.Enabled = false;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.Checked = true;
            }
            else
            {
                chkTwoRows.Enabled = true;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.DataBindings.Add("Checked", this, "Display2Rows", false, DataSourceUpdateMode.OnPropertyChanged);
            }
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        private T ParseEnum<T>(XmlElement element)
        {
            return (T)Enum.Parse(typeof(T), element.InnerText);
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Version version;
            if (element["Version"] != null)
                version = Version.Parse(element["Version"].InnerText);
            else
                version = new Version(1, 0, 0, 0);
            TextColor = ParseColor(element["TextColor"]);
            OverrideTextColor = Boolean.Parse(element["OverrideTextColor"].InnerText);
            TimeColor = ParseColor(element["TimeColor"]);
            OverrideTimeColor = Boolean.Parse(element["OverrideTimeColor"].InnerText);
            BackgroundColor = ParseColor(element["BackgroundColor"]);
            BackgroundColor2 = ParseColor(element["BackgroundColor2"]);
            GradientString = element["BackgroundGradient"].InnerText;
            Font1 = GetFontFromElement(element["Font1"]);
            Font2 = GetFontFromElement(element["Font2"]);
            OverrideFont1 = Boolean.Parse(element["OverrideFont1"].InnerText);
            OverrideFont2 = Boolean.Parse(element["OverrideFont2"].InnerText);
            if (version >= new Version(1, 4))
                Display2Rows = Boolean.Parse(element["Display2Rows"].InnerText);
            else
                Display2Rows = false;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(ToElement(document, "Version", "1.4"));
            parent.AppendChild(ToElement(document, TextColor, "TextColor"));
            parent.AppendChild(ToElement(document, "OverrideTextColor", OverrideTextColor));
            parent.AppendChild(ToElement(document, TimeColor, "TimeColor"));
            parent.AppendChild(ToElement(document, "OverrideTimeColor", OverrideTimeColor));
            parent.AppendChild(ToElement(document, BackgroundColor, "BackgroundColor"));
            parent.AppendChild(ToElement(document, BackgroundColor2, "BackgroundColor2"));
            parent.AppendChild(ToElement(document, "BackgroundGradient", BackgroundGradient));
            parent.AppendChild(CreateFontElement(document, "Font1", Font1));
            parent.AppendChild(CreateFontElement(document, "Font2", Font2));
            parent.AppendChild(ToElement(document, "OverrideFont1", OverrideFont1));
            parent.AppendChild(ToElement(document, "OverrideFont2", OverrideFont2));
            parent.AppendChild(ToElement(document, "Display2Rows", Display2Rows));
            return parent;
        }

        private Color ParseColor(XmlElement colorElement)
        {
            return Color.FromArgb(Int32.Parse(colorElement.InnerText, NumberStyles.HexNumber));
        }

        private XmlElement ToElement(XmlDocument document, Color color, string name)
        {
            var element = document.CreateElement(name);
            element.InnerText = color.ToArgb().ToString("X8");
            return element;
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var picker = new ColorPickerDialog();
            picker.SelectedColorChanged += (s, x) => button.BackColor = picker.SelectedColor;
            picker.SelectedColor = picker.OldColor = button.BackColor;
            picker.ShowDialog(this);
            button.BackColor = picker.SelectedColor;
        }

        private XmlElement ToElement<T>(XmlDocument document, String name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            return element;
        }

        private Font GetFontFromElement(XmlElement element)
        {
            if (!element.IsEmpty)
            {
                var bf = new BinaryFormatter();

                var base64String = element.InnerText;
                var data = Convert.FromBase64String(base64String);
                var ms = new MemoryStream(data);
                return (Font)bf.Deserialize(ms);
            }
            return null;
        }

        private Font ChooseFont(Font previousFont, int minSize, int maxSize)
        {
            var dialog = new FontDialog();
            dialog.Font = previousFont;
            /*dialog.MaxSize = (int)previousFont.SizeInPoints;
            dialog.MinSize = (int)previousFont.SizeInPoints;*/
            dialog.MinSize = minSize;
            dialog.MaxSize = maxSize;
            try
            {
                var result = dialog.ShowDialog(this);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.Font;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                MessageBox.Show("This font is not supported.", "Font Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return previousFont;
        }

        private void btnFont1_Click(object sender, EventArgs e)
        {
            Font1 = ChooseFont(Font1, 7, 20);
            lblFont.Text = Font1String;
        }

        private void btnFont2_Click(object sender, EventArgs e)
        {
            Font2 = ChooseFont(Font2, 7, 20);
            lblFont.Text = Font2String;
        }

        private XmlElement CreateFontElement(XmlDocument document, String elementName, Font font)
        {
            var element = document.CreateElement(elementName);

            if (font != null)
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();

                    bf.Serialize(ms, font);
                    var data = ms.ToArray();
                    var cdata = document.CreateCDataSection(Convert.ToBase64String(data));
                    element.InnerXml = cdata.OuterXml;
                }
            }

            return element;
        }
    }
}