using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization; // Localization
using System.IO;            // List files
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Anime_Girl_Generator.Properties;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections;
using Ini;

namespace Anime_Girl_Generator
{
    public partial class Main : Form
    {
        // Localization
        private System.Resources.ResourceManager RM = null;
        private CultureInfo EnglishCulture = new CultureInfo("en-US");
        private CultureInfo FrenchCulture = new CultureInfo("fr-FR");
        private string mainDirectory;
        private bool generatePicture = true;
        private ListViewItem _itemDnD = null;
        Stopwatch sw = new Stopwatch();

        // store generated images
        private Dictionary<string, Bitmap> itemDictionary = new Dictionary<string, Bitmap>();
        private List<Dictionary<string, Bitmap>> accessoriesDictionary = new List<Dictionary<string, Bitmap>>();
        List<string> accessorySources = new List<string>();

        public Main()
        {
            // Localization
            RM = new System.Resources.ResourceManager("Anime_Girl_Generator.Main", System.Reflection.Assembly.GetExecutingAssembly());
            System.Threading.Thread.CurrentThread.CurrentUICulture = Settings.Default.Language;

            InitializeComponent();

            if (Settings.Default.Language.Name == EnglishCulture.Name)
            {
                this.englishToolStripMenuItem.Checked = true;
            }
            else if (Settings.Default.Language.Name == FrenchCulture.Name)
            {
                this.frenchToolStripMenuItem.Checked = true;
            }

            // initialize collection type settings (avoid null issues)
            if (Settings.Default.Accessories == null)
                Settings.Default.Accessories = new StringCollection();

            // initialize list of image types for accessories
            accessorySources.Add("accessory_back");
            accessorySources.Add("accessory_underwear");
            accessorySources.Add("accessory_middle_back");
            accessorySources.Add("accessory_middle_front");
            accessorySources.Add("accessory_front");

            // Load item lists
            string[] filePaths;
            mainDirectory = Directory.GetCurrentDirectory() + @"\default";
            // List of Hairs (front)
            if(Directory.Exists(mainDirectory + @"\hair_front")) {
                filePaths = Directory.GetFiles(mainDirectory + @"\hair_front", "*.png", SearchOption.AllDirectories);
                filePaths = filePaths.Select(f => Path.GetFileNameWithoutExtension(f)).Cast<string>().ToArray();
                this.lstHairFront.Items.AddRange(filePaths);
            }
            // List of Hairs (back)
            if (Directory.Exists(mainDirectory + @"\hair_back"))
            {
                filePaths = Directory.GetFiles(mainDirectory + @"\hair_back", "*.png", SearchOption.AllDirectories);
                filePaths = filePaths.Select(f => Path.GetFileNameWithoutExtension(f)).Cast<string>().ToArray();
                this.lstHairBack.Items.AddRange(filePaths);
            }
            // List of Eyes
            if (Directory.Exists(mainDirectory + @"\eye"))
            {
                filePaths = Directory.GetFiles(mainDirectory + @"\eye", "*.png", SearchOption.AllDirectories);
                filePaths = filePaths.Select(f => Path.GetFileNameWithoutExtension(f)).Cast<string>().ToArray();
                this.lstEyes.Items.AddRange(filePaths);
            }
            // List of Faces (mouth + blush + eyebrows)
            if (Directory.Exists(mainDirectory + @"\face_back"))
            {
                filePaths = Directory.GetFiles(mainDirectory + @"\face_back", "*.png", SearchOption.AllDirectories);
                filePaths = filePaths.Select(f => Path.GetFileNameWithoutExtension(f)).Cast<string>().ToArray();
                this.lstFace.Items.AddRange(filePaths);
            }
            // List of Heads
            if (Directory.Exists(mainDirectory + @"\head"))
            {
                filePaths = Directory.GetFiles(mainDirectory + @"\head", "*.png", SearchOption.AllDirectories);
                filePaths = filePaths.Select(f => Path.GetFileNameWithoutExtension(f)).Cast<string>().ToArray();
                this.lstHead.Items.AddRange(filePaths);
            }
            // List of Bodys & Clothes
            if (Directory.Exists(mainDirectory + @"\body_back"))
            {
                filePaths = Directory.GetFiles(mainDirectory + @"\body_back", "*.png", SearchOption.AllDirectories);
                filePaths = filePaths.Select(f => Path.GetFileNameWithoutExtension(f)).Cast<string>().ToArray();
                this.lstBody.Items.AddRange(filePaths);
            }
            // List of Accessories
            if (Directory.Exists(mainDirectory + @"\accessory_back"))
            {
                filePaths = Directory.GetFiles(mainDirectory + @"\accessory_back", "*.png", SearchOption.AllDirectories);
                filePaths = filePaths.Select(f => Path.GetFileNameWithoutExtension(f)).Cast<string>().ToArray();
                this.lstAccessories.Items.AddRange(filePaths);
            }
            // Load user settings or default settings, then generate the picture
            this.loadSettings();
        }


        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.uncheckLanguages();
            this.englishToolStripMenuItem.Checked = true;
            System.Threading.Thread.CurrentThread.CurrentUICulture = EnglishCulture;
            this.reloadLocalizedStrings();
        }

        private void frenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.uncheckLanguages();
            this.frenchToolStripMenuItem.Checked = true;
            System.Threading.Thread.CurrentThread.CurrentUICulture = FrenchCulture;
            this.reloadLocalizedStrings();
        }

        private void uncheckLanguages()
        {
            this.englishToolStripMenuItem.Checked = false;
            this.frenchToolStripMenuItem.Checked = false;
        }

        private void reloadLocalizedStrings()
        {
            // save language to settings
            Settings.Default.Language = System.Threading.Thread.CurrentThread.CurrentUICulture;

            // menu
            this.fileToolStripMenuItem.Text = RM.GetString("fileToolStripMenuItem.Text");
            this.openModelToolStripMenuItem.Text = RM.GetString("openModelToolStripMenuItem.Text");
            this.saveModelToolStripMenuItem.Text = RM.GetString("saveModelToolStripMenuItem.Text");
            this.exportPictureToolStripMenuItem.Text = RM.GetString("exportPictureToolStripMenuItem.Text");
            this.exitToolStripMenuItem.Text = RM.GetString("exitToolStripMenuItem.Text");
            this.languageToolStripMenuItem.Text = RM.GetString("languageToolStripMenuItem.Text");
            this.englishToolStripMenuItem.Text = RM.GetString("englishToolStripMenuItem.Text");
            this.frenchToolStripMenuItem.Text = RM.GetString("frenchToolStripMenuItem.Text");
            this.aboutToolStripMenuItem.Text = RM.GetString("aboutToolStripMenuItem.Text");
            this.interrogationToolStripMenuItem.Text = RM.GetString("interrogationToolStripMenuItem.Text");

            // tabs
            this.tabHair.Text = RM.GetString("tabHair.Text");
            this.tabFace.Text = RM.GetString("tabFace.Text");
            this.tabBody.Text = RM.GetString("tabBody.Text");
            this.tabAccessories.Text = RM.GetString("tabAccessories.Text");
            this.tabParam.Text = RM.GetString("tabParam.Text");

            //Hairs
            this.lblHairFront.Text = RM.GetString("lblHairFront.Text");
            this.lblHairBack.Text = RM.GetString("lblHairBack.Text");
            this.lblHairColor.Text = RM.GetString("lblHairColor.Text");
            this.lblHairLuminance.Text = RM.GetString("lblHairLuminance.Text");

            //Face
            this.lblEyes.Text = RM.GetString("lblEyes.Text");
            this.lblFace.Text = RM.GetString("lblEyesFace.Text");
            this.lblEyesColor.Text = RM.GetString("lblEyesColor.Text");
            this.lblEyesLuminance.Text = RM.GetString("lblEyesLuminance.Text");

            //Body
            this.lblHead.Text = RM.GetString("lblHead.Text");
            this.lblBody.Text = RM.GetString("lblBody.Text");
            this.lblBodyColor.Text = RM.GetString("lblBodyColor.Text");
            this.lblBodyLuminance.Text = RM.GetString("lblBodyLuminance.Text");
            this.lblClothesColor.Text = RM.GetString("lblClothesColor.Text");
            this.lblClothesLuminance.Text = RM.GetString("lblClothesLuminance.Text");

            //Accessories
            this.lblAccessories.Text = RM.GetString("lblAccessories.Text");
            this.lblAccessoriesSelected.Text = RM.GetString("lblAccessoriesSelected.Text");
            this.lblDragDropHelp.Text = RM.GetString("lblDragDropHelp.Text");
            this.lblAccessoryColor.Text = RM.GetString("lblAccessoryColor.Text");
            this.lblAccessoryLuminance.Text = RM.GetString("lblAccessoryLuminance.Text");
            this.btRemoveAllAccessories.Text = RM.GetString("btRemoveAllAccessories.Text");

            //Param
            this.btBgColor.Text = RM.GetString("btBgColor.Text");
            this.lblBgColor.Text = RM.GetString("lblBgColor.Text");
            this.chkBackTransparent.Text = RM.GetString("chkBackTransparent.Text");
        }

        private void loadSettings()
        {
            // don't regenerate the picture until we ask for it.
            this.generatePicture = false;

            // load color settings
            try
            {
                this.lblBgColor.BackColor = Settings.Default.BackColor;
                this.pictureBox.BackColor = Settings.Default.BackColor;
                if (Settings.Default.BackColor.A == 0)
                {
                    this.chkBackTransparent.Checked = true;
                }
            }
            catch
            {
                this.lblBgColor.BackColor = new Color();
                this.pictureBox.BackColor = new Color();
            }

            this.tbBodyColor.Value = Settings.Default.BodyColor;
            if (this.tbBodyColor.Value == null)
                this.tbBodyColor.Value = 0;

            this.tbBodyLuminance.Value = Settings.Default.BodyLuminance;
            if (this.tbBodyLuminance.Value == null)
                this.tbBodyLuminance.Value = 0;

            this.tbClothesColor.Value = Settings.Default.ClothesColor;
            if (this.tbClothesColor.Value == null)
                this.tbClothesColor.Value = 0;

            this.tbClothesLuminance.Value = Settings.Default.ClothesLuminance;
            if (this.tbClothesLuminance.Value == null)
                this.tbClothesLuminance.Value = 0;

            this.tbEyesColor.Value = Settings.Default.EyesColor;
            if (this.tbEyesColor.Value == null)
                this.tbEyesColor.Value = 0;

            this.tbEyesLuminance.Value = Settings.Default.EyesLuminance;
            if (this.tbEyesLuminance.Value == null)
                this.tbEyesLuminance.Value = 0;

            this.tbHairColor.Value = Settings.Default.HairColor;
            if (this.tbHairColor.Value == null)
                this.tbHairColor.Value = 0;

            this.tbHairLuminance.Value = Settings.Default.HairLuminance;
            if (this.tbHairLuminance.Value == null)
                this.tbHairLuminance.Value = 0;

            // load item settings
            this.lstHairFront.SelectedItem = Settings.Default.HairFront;
            if (this.lstHairFront.SelectedItem == null)
                this.lstHairFront.SelectedIndex = 0;

            this.lstHairBack.SelectedItem = Settings.Default.HairBack;
            if (this.lstHairBack.SelectedItem == null)
                this.lstHairBack.SelectedIndex = 0;

            this.lstEyes.SelectedItem = Settings.Default.Eyes;
            if (this.lstEyes.SelectedItem == null)
                this.lstEyes.SelectedIndex = 0;

            this.lstFace.SelectedItem = Settings.Default.Face;
            if (this.lstFace.SelectedItem == null)
                this.lstFace.SelectedIndex = 0;

            this.lstHead.SelectedItem = Settings.Default.Head;
            if (this.lstHead.SelectedItem == null)
                this.lstHead.SelectedIndex = 0;

            this.lstBody.SelectedItem = Settings.Default.Body;
            if (this.lstBody.SelectedItem == null)
                this.lstBody.SelectedIndex = 0;

            // load accessories
            var items = new ListViewItem[Properties.Settings.Default.Accessories.Count];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new ListViewItem(Properties.Settings.Default.Accessories[i].Split('|'));
            }
            this.lstViewAccessoriesSelected.Items.AddRange(items);
            if (this.lstViewAccessoriesSelected.Items.Count > 0)
            {
                this.lstViewAccessoriesSelected.Items[0].Selected = true;
                this.tbAccessoryColor.Enabled = true;
                this.tbAccessoryLuminance.Enabled = true;
            }
            else
            {
                this.tbAccessoryColor.Enabled = false;
                this.tbAccessoryLuminance.Enabled = false;
            }

            this.generatePicture = true;

            // Overlay order (from back to front):
            //  1) accessory_back
            //  2) hair_back
            //  3) hair_back_accessory
            //  4) body_back
            //  5) accessory_underwear
            //  6) body_front
            //  7) body_front_color
            //  9) accessory_middle_back
            //  8) head
            // 10) accessory_middle_front
            // 11) hair_front
            // 12) hair_front_accessory
            // 13) face_back
            // 14) eye
            // 15) face_front
            // 16) accessory_front

            // fill the accessoriesDictionary
            foreach (ListViewItem item in this.lstViewAccessoriesSelected.Items)
            {
                setAccessory(item.Index, item.SubItems[0].Text, Convert.ToInt32(item.SubItems[1].Text), Convert.ToInt32(item.SubItems[2].Text), true);
            }

            // fill the itemDictionary
            this.setItem("hair_back", this.lstHairBack.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
            this.setItem("hair_back_accessory", this.lstHairBack.SelectedItem.ToString(), 0, 0);
            this.setItem("body_back", this.lstBody.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
            this.setItem("body_front", this.lstBody.SelectedItem.ToString(), 0, 0);
            this.setItem("body_front_color", this.lstBody.SelectedItem.ToString(), this.tbClothesColor.Value, (float)this.tbClothesLuminance.Value);
            this.setItem("head", this.lstHead.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
            this.setItem("hair_front", this.lstHairFront.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
            this.setItem("hair_front_accessory", this.lstHairFront.SelectedItem.ToString(), 0, 0);
            this.setItem("face_back", this.lstFace.SelectedItem.ToString(), 0, 0);
            this.setItem("eye", this.lstEyes.SelectedItem.ToString(), this.tbEyesColor.Value, (float)this.tbEyesLuminance.Value);
            this.setItem("face_front", this.lstFace.SelectedItem.ToString(), 0, 0);

            this.repaintImage();
        }

        private void repaintImage()
        {
            // Generate image from images list
            Bitmap finalImage = new Bitmap(300, 400);
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                // Background color
                g.Clear(Settings.Default.BackColor);

                // Add each image over the previous one
                foreach (Dictionary<string, Bitmap> item in accessoriesDictionary)
                {
                    if (item.ContainsKey("accessory_back"))
                        g.DrawImage(item["accessory_back"], 0, 0);
                }
                if(itemDictionary.ContainsKey("hair_back"))
                    g.DrawImage(itemDictionary["hair_back"], 0, 0);
                if(itemDictionary.ContainsKey("hair_back_accessory"))
                    g.DrawImage(itemDictionary["hair_back_accessory"], 0, 0);
                if(itemDictionary.ContainsKey("body_back"))
                    g.DrawImage(itemDictionary["body_back"], 0, 0);
                foreach (Dictionary<string, Bitmap> item in accessoriesDictionary)
                {
                    if (item.ContainsKey("accessory_underwear"))
                        g.DrawImage(item["accessory_underwear"], 0, 0);
                }
                if(itemDictionary.ContainsKey("body_front"))
                    g.DrawImage(itemDictionary["body_front"], 0, 0);
                if(itemDictionary.ContainsKey("body_front_color"))
                    g.DrawImage(itemDictionary["body_front_color"], 0, 0);
                foreach (Dictionary<string, Bitmap> item in accessoriesDictionary)
                {
                    if (item.ContainsKey("accessory_middle_back"))
                        g.DrawImage(item["accessory_middle_back"], 0, 0);
                }
                if(itemDictionary.ContainsKey("head"))
                    g.DrawImage(itemDictionary["head"], 0, 0);
                foreach (Dictionary<string, Bitmap> item in accessoriesDictionary)
                {
                    if (item.ContainsKey("accessory_middle_front"))
                        g.DrawImage(item["accessory_middle_front"], 0, 0);
                }
                if(itemDictionary.ContainsKey("hair_front"))
                    g.DrawImage(itemDictionary["hair_front"], 0, 0);
                if(itemDictionary.ContainsKey("hair_front_accessory"))
                    g.DrawImage(itemDictionary["hair_front_accessory"], 0, 0);
                if(itemDictionary.ContainsKey("face_back"))
                    g.DrawImage(itemDictionary["face_back"], 0, 0);
                if(itemDictionary.ContainsKey("eye"))
                    g.DrawImage(itemDictionary["eye"], 0, 0);
                if(itemDictionary.ContainsKey("face_front"))
                    g.DrawImage(itemDictionary["face_front"], 0, 0);
                foreach (Dictionary<string, Bitmap> item in accessoriesDictionary)
                {
                    if (item.ContainsKey("accessory_front"))
                        g.DrawImage(item["accessory_front"], 0, 0);
                }
            }

            this.pictureBox.Image = finalImage;

            // execution time
            sw.Stop();
            this.toolStripStatusLabel1.Text = RM.GetString("generateTime.Text") + String.Format("{0:0}",sw.Elapsed.TotalMilliseconds) + "ms";
        }

        private void setItem(string folder, string item, float color, float luminance)
        {
            if(sw.IsRunning == false)
                sw = Stopwatch.StartNew();
            
            string filePath;
            Bitmap image;
            filePath = mainDirectory + @"\" + folder + @"\" + item + @".png";
            if (File.Exists(filePath))
            {
                image = new Bitmap(filePath);
                image.SetResolution(96, 96);

                if (color != 0 || luminance != 0)
                {
                    this.RotateColors(image, color * 20, luminance / 10);
                }

                if (this.itemDictionary.ContainsKey(folder))
                {
                    itemDictionary[folder] = image;
                }
                else
                {
                    itemDictionary.Add(folder, image);
                }
            }
            else
            {
                if (this.itemDictionary.ContainsKey(folder))
                {
                    itemDictionary.Remove(folder);
                }
            }
        }

        private void setAccessory(int index, string item, float color, float luminance, bool insert)
        {
            if (sw.IsRunning == false)
                sw = Stopwatch.StartNew();

            string filePath;
            Bitmap image;
            Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

            foreach (string source in accessorySources)
            {
                filePath = mainDirectory + @"\" + source + @"\" + item + @".png";
                if (File.Exists(filePath))
                {
                    image = new Bitmap(filePath);
                    image.SetResolution(96, 96);

                    if (color != 0 || luminance != 0)
                    {
                        this.RotateColors(image, color * 20, luminance / 10);
                    }
                    images.Add(source,image);
                }
            }

            if (insert)
            {
                // insert the series of Bitmap at the designated index
                this.accessoriesDictionary.Insert(index, images);
            }
            else
            {
                this.accessoriesDictionary[index] = images;
            }
        }

        private void RotateColors(Bitmap image, float degrees, float luminance)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            int width = image.Width;
            int height = image.Height;
            double r = degrees * System.Math.PI / 180; // degrees to radians
            float cosR = (float)Math.Cos(r);
            float sinR = (float)Math.Sin(r);
            float a = (1 + 2 * cosR) / 3;
            float b = ((1 - cosR) - (float)Math.Sqrt(3) * sinR) / 3;
            float c = ((1 - cosR) + (float)Math.Sqrt(3) * sinR) / 3;

            float[][] colorMatrixElements = { 
                new float[] {a, b, c, 0, 0},
                new float[] {c, a, b, 0, 0},
                new float[] {b, c, a, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {luminance, luminance, luminance, 0, 1}};

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            
            imageAttributes.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);

            using (Graphics g = Graphics.FromImage(image))
            {
                g.DrawImage(
                   image,
                   new Rectangle(0, 0, width, height),  // destination rectangle 
                    0, 0,        // upper-left corner of source rectangle 
                    width,       // width of source rectangle
                    height,      // height of source rectangle
                    GraphicsUnit.Pixel,
                   imageAttributes);
            }

        }

        private void lstEyes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("eye", this.lstEyes.SelectedItem.ToString(), this.tbEyesColor.Value, (float)this.tbEyesLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.Eyes = this.lstEyes.SelectedItem.ToString();
        }

        private void lstFace_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("face_back", this.lstFace.SelectedItem.ToString(), 0, 0);
                this.setItem("face_front", this.lstFace.SelectedItem.ToString(), 0, 0);
                this.repaintImage();
            }
            if (this.generatePicture) repaintImage();
            Settings.Default.Face = this.lstFace.SelectedItem.ToString();
        }

        private void lstHairFront_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("hair_front", this.lstHairFront.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
                this.setItem("hair_front_accessory", this.lstHairFront.SelectedItem.ToString(), 0, 0);
                this.repaintImage();
            }
            if (this.generatePicture) repaintImage();
            Settings.Default.HairFront = this.lstHairFront.SelectedItem.ToString();
        }

        private void lstHairBack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("hair_back", this.lstHairBack.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
                this.setItem("hair_back_accessory", this.lstHairBack.SelectedItem.ToString(), 0, 0);
                this.repaintImage();
            }
            if (this.generatePicture) repaintImage();
            Settings.Default.HairBack = this.lstHairBack.SelectedItem.ToString();
        }

        private void lstHead_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("head", this.lstHead.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
                this.repaintImage();
            }
            if (this.generatePicture) repaintImage();
            Settings.Default.Head = this.lstHead.SelectedItem.ToString();
        }

        private void lstBody_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("body_back", this.lstBody.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
                this.setItem("body_front", this.lstBody.SelectedItem.ToString(), 0, 0);
                this.setItem("body_front_color", this.lstBody.SelectedItem.ToString(), this.tbClothesColor.Value, (float)this.tbClothesLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.Body = this.lstBody.SelectedItem.ToString();
        }

        private void btBgColor_Click(object sender, EventArgs e)
        {
            DialogResult result = this.colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.lblBgColor.BackColor = this.colorDialog.Color;
                Settings.Default.BackColor = this.colorDialog.Color;
                this.repaintImage();
            }
        }

        private void chkBackTransparent_CheckedChanged(object sender, EventArgs e)
        {
            Color color;
            if (this.chkBackTransparent.Checked == true)
            {
                this.btBgColor.Enabled = false;
                color = Color.Transparent;
            }
            else
            {
                this.btBgColor.Enabled = true;
                color = Color.White;
            }
            this.lblBgColor.BackColor = color;
            Settings.Default.BackColor = color;
            this.repaintImage();
        }

        private void tbHairColor_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("hair_back", this.lstHairBack.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
                this.setItem("hair_front", this.lstHairFront.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.HairColor = this.tbHairColor.Value;
        }

        private void tbHairLuminance_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("hair_back", this.lstHairBack.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
                this.setItem("hair_front", this.lstHairFront.SelectedItem.ToString(), this.tbHairColor.Value, (float)this.tbHairLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.HairLuminance = this.tbHairLuminance.Value;
        }

        private void tbEyesColor_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("eye", this.lstEyes.SelectedItem.ToString(), this.tbEyesColor.Value, (float)this.tbEyesLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.EyesColor = this.tbEyesColor.Value;
        }

        private void tbEyesLuminance_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("eye", this.lstEyes.SelectedItem.ToString(), this.tbEyesColor.Value, (float)this.tbEyesLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.EyesLuminance = this.tbEyesLuminance.Value;
        }

        private void tbBodyColor_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("body_back", this.lstBody.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
                this.setItem("head", this.lstHead.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.BodyColor = this.tbBodyColor.Value;
        }

        private void tbBodyLuminance_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("body_back", this.lstBody.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
                this.setItem("head", this.lstHead.SelectedItem.ToString(), this.tbBodyColor.Value, (float)this.tbBodyLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.BodyLuminance = this.tbBodyLuminance.Value;
        }

        private void tbClothesColor_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("body_front", this.lstBody.SelectedItem.ToString(), 0, 0);
                this.setItem("body_front_color", this.lstBody.SelectedItem.ToString(), this.tbClothesColor.Value, (float)this.tbClothesLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.ClothesColor = this.tbClothesColor.Value;
        }

        private void tbClothesLuminance_ValueChanged(object sender, EventArgs e)
        {
            if (this.generatePicture)
            {
                this.setItem("body_front", this.lstBody.SelectedItem.ToString(), 0, 0);
                this.setItem("body_front_color", this.lstBody.SelectedItem.ToString(), this.tbClothesColor.Value, (float)this.tbClothesLuminance.Value);
                this.repaintImage();
            }
            Settings.Default.ClothesLuminance = this.tbClothesLuminance.Value;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void exportPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName;
            DialogResult result = this.savePngDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                fileName = this.savePngDialog.FileName;
                Bitmap imgToExport = new Bitmap(this.pictureBox.Image);
                imgToExport.Save(fileName, ImageFormat.Png);
            }
        }

        private void openModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName;
            DialogResult result = this.openAgmDialog.ShowDialog();

            // empty accessories list
            this.accessoriesDictionary.Clear();
            this.lstViewAccessoriesSelected.Items.Clear();
            Settings.Default.Accessories.Clear();

            if (result == DialogResult.OK)
            {
                fileName = this.openAgmDialog.FileName;
                IniFile ini = new IniFile(fileName);
                int i;

                // selected parts
                Settings.Default.HairFront = ini.IniReadValue("Parts", "hair_front");
                Settings.Default.HairBack = ini.IniReadValue("Parts", "hair_back");
                Settings.Default.Head = ini.IniReadValue("Parts", "head");
                Settings.Default.Face = ini.IniReadValue("Parts", "face");
                Settings.Default.Eyes = ini.IniReadValue("Parts", "eye");
                Settings.Default.Body = ini.IniReadValue("Parts", "body");

                // color settings
                Settings.Default.HairColor = Convert.ToInt16(ini.IniReadValue("Color", "hair_color"));
                Settings.Default.HairLuminance = Convert.ToInt16(ini.IniReadValue("Color", "hair_luminance"));
                Settings.Default.EyesColor = Convert.ToInt16(ini.IniReadValue("Color", "eye_color"));
                Settings.Default.EyesLuminance = Convert.ToInt16(ini.IniReadValue("Color", "eye_luminance"));
                Settings.Default.BodyColor = Convert.ToInt16(ini.IniReadValue("Color", "skin_color"));
                Settings.Default.BodyLuminance = Convert.ToInt16(ini.IniReadValue("Color", "skin_luminance"));
                Settings.Default.ClothesColor = Convert.ToInt16(ini.IniReadValue("Color", "cloth_color"));
                Settings.Default.ClothesLuminance = Convert.ToInt16(ini.IniReadValue("Color", "cloth_luminance"));

                // accessories
                string[] accessory = ini.IniReadValue("Parts", "accessory").Split(',');
                string[] accessory_color = ini.IniReadValue("Color", "accessory_color").Split(',');
                string[] accessory_luminance = ini.IniReadValue("Color", "accessory_luminance").Split(',');
                var items = new List<string>();
                for (i = 0; i < accessory.Length; i++)
                {
                    string[] item = new string[] { accessory[i], accessory_color[i], accessory_luminance[i] };
                    //this.lstViewAccessoriesSelected.Items.Add(new ListViewItem(item));

                    
                    var subitems = new List<string>();
                    subitems.Add(accessory[i]);
                    subitems.Add(accessory_color[i]);
                    subitems.Add(accessory_luminance[i]);
                    items.Add(string.Join("|", subitems.ToArray()));
                }
                Properties.Settings.Default.Accessories.AddRange(items.ToArray());

                // background color
                string[] back_rgb = ini.IniReadValue("Color", "bg_rgb").Split(',');
                Settings.Default.BackColor = Color.FromArgb(Convert.ToByte(back_rgb[0]), Convert.ToByte(back_rgb[1]), Convert.ToByte(back_rgb[2]));

                // load the newly selected settings
                this.loadSettings();
            }

        }

        private void saveModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName;
            DialogResult result = this.saveAgmDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                fileName = this.saveAgmDialog.FileName;
                IniFile ini = new IniFile(fileName);

                string accessory, accessory_color, accessory_luminance;

                accessory = "";
                accessory_color = "";
                accessory_luminance = "";
                foreach (ListViewItem item in this.lstViewAccessoriesSelected.Items)
                {
                    if (item.Index < this.lstViewAccessoriesSelected.Items.Count - 1)
                    {
                        accessory += item.SubItems[0].Text + ",";
                        accessory_color += item.SubItems[1].Text + ",";
                        accessory_luminance += item.SubItems[2].Text + ",";
                    }
                    else
                    {
                        accessory += item.SubItems[0].Text;
                        accessory_color += item.SubItems[1].Text;
                        accessory_luminance += item.SubItems[2].Text;
                    }
                }

                ini.IniWriteValue("Character", "name", "default");
                ini.IniWriteValue("Parts", "hair_front", this.lstHairFront.SelectedItem.ToString());
                ini.IniWriteValue("Parts", "hair_back", this.lstHairBack.SelectedItem.ToString());
                ini.IniWriteValue("Parts", "head", this.lstHead.SelectedItem.ToString());
                ini.IniWriteValue("Parts", "face", this.lstFace.SelectedItem.ToString());
                ini.IniWriteValue("Parts", "eye", this.lstEyes.SelectedItem.ToString());
                ini.IniWriteValue("Parts", "body", this.lstBody.SelectedItem.ToString());
                ini.IniWriteValue("Parts", "accessory", accessory);

                ini.IniWriteValue("Color", "hair_color", this.tbHairColor.Value.ToString());
                ini.IniWriteValue("Color", "hair_luminance", this.tbHairLuminance.Value.ToString());
                ini.IniWriteValue("Color", "eye_color", this.tbEyesColor.Value.ToString());
                ini.IniWriteValue("Color", "eye_luminance", this.tbEyesLuminance.Value.ToString());
                ini.IniWriteValue("Color", "skin_color", this.tbBodyColor.Value.ToString());
                ini.IniWriteValue("Color", "skin_luminance", this.tbBodyLuminance.Value.ToString());
                ini.IniWriteValue("Color", "cloth_color", this.tbClothesColor.Value.ToString());
                ini.IniWriteValue("Color", "cloth_luminance", this.tbClothesLuminance.Value.ToString());
                ini.IniWriteValue("Color", "accessory_color", accessory_color);
                ini.IniWriteValue("Color", "accessory_luminance", accessory_luminance);
                ini.IniWriteValue("Color", "bg_rgb",
                    Settings.Default.BackColor.R.ToString() + "," +
                    Settings.Default.BackColor.G.ToString() + "," +
                    Settings.Default.BackColor.B.ToString());
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                RM.GetString("about.Text") + Settings.Default.Version,
                RM.GetString("about.Title"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // Add new accessory
        private void lstAccessories_DoubleClick(object sender, EventArgs e)
        {
            string[] item = new string[] { this.lstAccessories.SelectedItem.ToString(), "0", "0" };
            int index = this.lstViewAccessoriesSelected.Items.Count;
            this.lstViewAccessoriesSelected.Items.Add(new ListViewItem(item));
            this.setAccessory(index, this.lstAccessories.SelectedItem.ToString(), 0, 0, true);
            this.refreshAccessoriesList();
        }

        private void lstViewAccessoriesSelected_ItemDrag(object sender, ItemDragEventArgs e)
        {
            this._itemDnD = (ListViewItem)e.Item;
        }

        private void lstViewAccessoriesSelected_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._itemDnD == null)
                return;

            // Show the user that a drag operation is happening
            Cursor = Cursors.HSplit;
        }

        private void lstViewAccessoriesSelected_MouseUp(object sender, MouseEventArgs e)
        {
            if (this._itemDnD == null)
                return;

            // use 0 instead of e.X so that you don't have
            // to keep inside the columns while dragging
            ListViewItem itemOver = this.lstViewAccessoriesSelected.GetItemAt(0, e.Y);
            if (itemOver == null)
            {
                itemOver = this.lstViewAccessoriesSelected.Items[this.lstViewAccessoriesSelected.Items.Count - 1];
            }

            Rectangle rc = itemOver.GetBounds(ItemBoundsPortion.Entire);

            // find out if we insert before or after the item the mouse is over
            bool insertBefore;
            if (e.Y < rc.Top + (rc.Height / 2))
                insertBefore = true;
            else
                insertBefore = false;

            if (_itemDnD != itemOver)
            // if we dropped the item on itself, nothing is to be done
            {
                if (insertBefore)
                {
                    Dictionary<string, Bitmap> item = this.accessoriesDictionary[_itemDnD.Index];
                    this.accessoriesDictionary.RemoveAt(_itemDnD.Index);
                    this.lstViewAccessoriesSelected.Items.Remove(_itemDnD);
                    this.accessoriesDictionary.Insert(itemOver.Index, item);
                    this.lstViewAccessoriesSelected.Items.Insert(itemOver.Index, _itemDnD);
                }
                else
                {
                    Dictionary<string, Bitmap> item = this.accessoriesDictionary[_itemDnD.Index];
                    this.accessoriesDictionary.RemoveAt(_itemDnD.Index);
                    this.lstViewAccessoriesSelected.Items.Remove(_itemDnD);
                    this.accessoriesDictionary.Insert(itemOver.Index + 1, item);
                    this.lstViewAccessoriesSelected.Items.Insert(itemOver.Index + 1, _itemDnD);
                }
                this.refreshAccessoriesList();
            }

            this._itemDnD = null;
            Cursor = Cursors.Default;
        }

        // after adding/removing/reordering items from the selected accessories list
        private void refreshAccessoriesList()
        {
            if (sw.IsRunning == false)
                sw = Stopwatch.StartNew();

            Settings.Default.Accessories = new StringCollection();
            var items = new List<string>();
            foreach (ListViewItem item in this.lstViewAccessoriesSelected.Items)
            {
                var subitems = new List<string>();
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    subitems.Add(subitem.Text);
                }
                items.Add(string.Join("|", subitems.ToArray()));
            }
            this.repaintImage();
            Properties.Settings.Default.Accessories.AddRange(items.ToArray());
        }

        private void lstViewAccessoriesSelected_DoubleClick(object sender, EventArgs e)
        {
            if (this.lstViewAccessoriesSelected.SelectedIndices.Count != 0)
            {
                this.accessoriesDictionary.RemoveAt(this.lstViewAccessoriesSelected.SelectedIndices[0]);
                this.lstViewAccessoriesSelected.Items.RemoveAt(this.lstViewAccessoriesSelected.SelectedIndices[0]);
                this.refreshAccessoriesList();
            }
        }

        private void btRemoveAllAccessories_Click(object sender, EventArgs e)
        {
            this.accessoriesDictionary.Clear();
            this.lstViewAccessoriesSelected.Items.Clear();
            Settings.Default.Accessories.Clear();
            if (this.generatePicture) repaintImage();
        }

        private void lstViewAccessoriesSelected_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void lstViewAccessoriesSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.generatePicture = false;
            if (this.lstViewAccessoriesSelected.SelectedIndices.Count == 0)
            {
                this.tbAccessoryColor.Enabled = false;
                this.tbAccessoryLuminance.Enabled = false;
            }
            else
            {
                this.tbAccessoryColor.Value = Convert.ToInt32(
                    this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[1].Text);
                this.tbAccessoryLuminance.Value = Convert.ToInt32(
                    this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[2].Text);
                this.tbAccessoryColor.Enabled = true;
                this.tbAccessoryLuminance.Enabled = true;
            }
            this.generatePicture = true;
        }

        private void tbAccessoryColor_ValueChanged(object sender, EventArgs e)
        {
            this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[1].Text = this.tbAccessoryColor.Value.ToString();
            if (this.generatePicture)
            {
                this.setAccessory(
                    this.lstViewAccessoriesSelected.SelectedItems[0].Index,
                    this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[0].Text,
                    Convert.ToInt16(this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[1].Text),
                    Convert.ToInt16(this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[2].Text),
                    false);
                this.refreshAccessoriesList();
            }
        }

        private void tbAccessoryLuminance_ValueChanged(object sender, EventArgs e)
        {
            this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[2].Text = this.tbAccessoryLuminance.Value.ToString();
            if (this.generatePicture)
            {
                this.setAccessory(
                    this.lstViewAccessoriesSelected.SelectedItems[0].Index,
                    this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[0].Text,
                    Convert.ToInt16(this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[1].Text),
                    Convert.ToInt16(this.lstViewAccessoriesSelected.SelectedItems[0].SubItems[2].Text),
                    false);
                this.refreshAccessoriesList();
            }
        }

    }
}
