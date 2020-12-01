using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace ExamineDictWords
{
    internal partial class SettingsForm_ExamineDictWords : Form
    {


        #region Get and Set Options

        public DictionaryMetaObject DictDataToReturn;
        public bool RawFreqs;
        public string SelectedDictionaryFileLocation;
        public bool IncludeStDevSetting;
        public int RoundLength;

       #endregion



        public SettingsForm_ExamineDictWords(DictionaryMetaObject Dicts, bool RawFreqs, string DictionaryFileLocation, bool IncludeStDevOption, int RoundLenParameter)
        {
            InitializeComponent();

            DictDataToReturn = Dicts;
            RawCountsCheckbox.Checked = RawFreqs;
            SelectedFileTextbox.Text = DictionaryFileLocation;
            IncludeStDevCheckbox.Checked = IncludeStDevOption;
            RoundingToNParameter.Value = RoundLenParameter;
            
        }










        private void OKButton_Click(object sender, System.EventArgs e)
        {
            RawFreqs = RawCountsCheckbox.Checked;
            SelectedDictionaryFileLocation = SelectedFileTextbox.Text;
            this.DialogResult = DialogResult.OK;
            IncludeStDevSetting = IncludeStDevCheckbox.Checked;
            RoundLength = (int)RoundingToNParameter.Value;

        }



        private void LoadDictionaryButton_Click(object sender, System.EventArgs e)
        {


            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.ValidateNames = true;
                dialog.Title = "Please choose the Dictionary file that you would like to read";
                dialog.FileName = "Dictionary.dic";
                dialog.Filter = "LIWC Dictionary File|*.dic;*.txt";
                if (dialog.ShowDialog() == DialogResult.OK)
                {


                    try
                    {
                        string DicText = File.ReadAllText(dialog.FileName, Encoding.UTF8).Trim();
                        DictionaryMetaObject InputDictData = new DictionaryMetaObject(Path.GetFileName(dialog.FileName), "User-loaded dictionary", "", DicText);
                        DictParser DP = new DictParser();
                        InputDictData.DictData = DP.ParseDict(InputDictData);
                        DictDataToReturn = InputDictData;
                        SelectedFileTextbox.Text = dialog.FileName;
                    }

                    catch
                    {
                        MessageBox.Show("There was an error while trying to read/load your dictionary file. Is your dictionary correctly formatted? Have you already added this dictionary to this plugin?", "Error reading dictionary", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DictDataToReturn = null;
                        SelectedFileTextbox.Text = "";
                        return;
                    }


                }
                else
                {
                    DictDataToReturn = null;
                    SelectedFileTextbox.Text = "";
                }



            }



        }









        //private DictionaryData ParseDict(DictionaryMetaObject DictionaryToParse)
        //{



        //    DictionaryData DictData = DictionaryToParse.DictData;

        //    //  ____                   _       _         ____  _      _   ____        _           ___  _     _           _   
        //    // |  _ \ ___  _ __  _   _| | __ _| |_ ___  |  _ \(_) ___| |_|  _ \  __ _| |_ __ _   / _ \| |__ (_) ___  ___| |_ 
        //    // | |_) / _ \| '_ \| | | | |/ _` | __/ _ \ | | | | |/ __| __| | | |/ _` | __/ _` | | | | | '_ \| |/ _ \/ __| __|
        //    // |  __/ (_) | |_) | |_| | | (_| | ||  __/ | |_| | | (__| |_| |_| | (_| | || (_| | | |_| | |_) | |  __/ (__| |_ 
        //    // |_|   \___/| .__/ \__,_|_|\__,_|\__\___| |____/|_|\___|\__|____/ \__,_|\__\__,_|  \___/|_.__// |\___|\___|\__|
        //    //            |_|                                                                             |__/               




        //    //parse out the the dictionary file
        //    DictData.MaxWords = 0;

        //    //yeah, there's levels to this thing
        //    DictData.FullDictionary = new Dictionary<string, Dictionary<int, Dictionary<string, string[]>>>();

        //    DictData.FullDictionary.Add("Wildcards", new Dictionary<int, Dictionary<string, string[]>>());
        //    DictData.FullDictionary.Add("Standards", new Dictionary<int, Dictionary<string, string[]>>());

        //    DictData.WildCardArrays = new Dictionary<int, string[]>();
        //    DictData.PrecompiledWildcards = new Dictionary<string, Regex>();

        //    List<string> AllEntriesInFile = new List<string>();


        //    Dictionary<int, List<string>> WildCardLists = new Dictionary<int, List<string>>();

          
        //        string[] DicSplit = DictionaryToParse.DictionaryRawText.Split(new char[] { '%' }, 3, StringSplitOptions.None);

        //        string[] HeaderLines = DicSplit[1].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        //        string[] EntryLines = DicSplit[2].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        //        DictData.NumCats = HeaderLines.Length;

        //        //now that we know the number of categories, we can fill out the arrays
        //        DictData.CatNames = new string[DictData.NumCats];
        //        DictData.CatValues = new string[DictData.NumCats];


        //        //Map Out the Categories
        //        for (int i = 0; i < DictData.NumCats; i++)
        //        {
        //            string[] HeaderRow = HeaderLines[i].Trim().Split(new char[] { '\t' }, 2);

        //            DictData.CatValues[i] = HeaderRow[0];
        //            DictData.CatNames[i] = HeaderRow[1];
        //        }


        //        //Map out the dictionary entries
        //        for (int i = 0; i < EntryLines.Length; i++)
        //        {

        //            string EntryLine = EntryLines[i].Trim();
        //            while (EntryLine.Contains("  ")) EntryLine.Replace("  ", " ");

        //            string[] EntryRow = EntryLine.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

        //            if (EntryRow.Length > 1 && !String.IsNullOrWhiteSpace(EntryRow[0]))
        //            {

        //                int Words_In_Entry = EntryRow[0].Split(' ').Length;
        //                if (Words_In_Entry > DictData.MaxWords) DictData.MaxWords = Words_In_Entry;

        //                //this is something special added to this version of the "parse dictionary" method
        //                //allows us to keep track of each entry within the dictionary file
        //                AllEntriesInFile.Add(EntryRow[0].ToLower());


        //                if (EntryRow[0].Contains("*"))
        //                {

        //                    if (DictData.FullDictionary["Wildcards"].ContainsKey(Words_In_Entry))
        //                    {
        //                        if (!DictData.FullDictionary["Wildcards"][Words_In_Entry].ContainsKey(EntryRow[0].ToLower()))
        //                        {
        //                            DictData.FullDictionary["Wildcards"][Words_In_Entry].Add(EntryRow[0].ToLower(), EntryRow.Skip(1).ToArray());
        //                            WildCardLists[Words_In_Entry].Add(EntryRow[0].ToLower());
        //                            DictData.PrecompiledWildcards.Add(EntryRow[0].ToLower(), new Regex("^" + Regex.Escape(EntryRow[0].ToLower()).Replace("\\*", ".*"), RegexOptions.Compiled));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        DictData.FullDictionary["Wildcards"].Add(Words_In_Entry, new Dictionary<string, string[]> { { EntryRow[0].ToLower(), EntryRow.Skip(1).ToArray() } });
        //                        WildCardLists.Add(Words_In_Entry, new List<string>(new string[] { EntryRow[0].ToLower() }));
        //                        DictData.PrecompiledWildcards.Add(EntryRow[0].ToLower(), new Regex("^" + Regex.Escape(EntryRow[0].ToLower()).Replace("\\*", ".*"), RegexOptions.Compiled));
        //                    }


        //                }
        //                else
        //                {
        //                    if (DictData.FullDictionary["Standards"].ContainsKey(Words_In_Entry))
        //                    {
        //                        if (!DictData.FullDictionary["Standards"][Words_In_Entry].ContainsKey(EntryRow[0].ToLower()))
        //                            DictData.FullDictionary["Standards"][Words_In_Entry].Add(EntryRow[0].ToLower(), EntryRow.Skip(1).ToArray());
        //                    }
        //                    else
        //                    {
        //                        DictData.FullDictionary["Standards"].Add(Words_In_Entry, new Dictionary<string, string[]> { { EntryRow[0].ToLower(), EntryRow.Skip(1).ToArray() } });
        //                    }
        //                }
        //            }
        //        }


        //        for (int i = DictData.MaxWords; i > 0; i--)
        //        {
        //            if (WildCardLists.ContainsKey(i)) DictData.WildCardArrays.Add(i, WildCardLists[i].ToArray());
        //        }
        //        WildCardLists.Clear();
        //        DictData.DictionaryLoaded = true;
        //        DictData.AllEntries = AllEntriesInFile;

        //        MessageBox.Show("Your dictionary has been successfully loaded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        //        return DictData;

            




        //}






























    }
}
