using System.Threading;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using PluginContracts;
using OutputHelperLib;
using System.Collections.Concurrent;


namespace ExamineDictWords
{
    public partial class ExamineDictWords : Plugin
    {


        public string[] InputType { get; } = { "Tokens" };
        public string OutputType { get; } = "OutputArray";

        public Dictionary<int, string> OutputHeaderData { get; set; } = new Dictionary<int, string>() { { 0, "TokenCount" } };
        public bool InheritHeader { get; } = false;

        #region Plugin Details and Info

        public string PluginName { get; } = "Evaluate Dictionary Words";
        public string PluginType { get; } = "Corpus Tools";
        public string PluginVersion { get; } = "1.1.0";
        public string PluginAuthor { get; } = "Ryan L. Boyd (ryan@ryanboyd.io)";
        public string PluginDescription { get; } = "This plugin will take a LIWC-formatted dictionary file and scan your texts, providing word-level output across your dataset. This plugin is particularly helpful for when you are interested in understanding which words in your dictionary are making the largest contribution to each of the dictionary's categories." + Environment.NewLine + Environment.NewLine +
                                                   "Output can be generated as raw frequencies or relative frequencies (i.e., LIWC style). Note that output scores are averaged across all documents. It is recommended that you use this plugin in conjunction with the \"Omit Observations\" plugin to help avoid skew, particularly if you are opting for the \"relative frequency\" output, which is the default -- this will help avoid skewing issues introduced by texts with very few words." + Environment.NewLine + Environment.NewLine +
                                                   "This plugin will also generate Cronbach's alpha values for each category (based on relative frequency data) as well as the the Kuder–Richardson Formula 20 (KR-20; based on \"one hot\" encoded output) as approximate measures of each category's internal consistency. " + 
                                                   "Note that variances, which are used for calculating internal consistency metrics and Standard Deviations, are calculated using a one-pass method: the Welford method. As such, your internal consistency metrics may be unstable, even within the same dataset, if you are working with a particularly small sample size.";
        public bool TopLevel { get; } = false;
        public string PluginTutorial { get; } = "https://youtu.be/vCoGZvDMFDw";

        DictionaryMetaObject UserLoadedDictionary { get; set; }
        private bool RawFreqs { get; set; } = false;
        private string DictionaryLocation = "";
        private bool IncludeStDevs { get; set; } = true;
        private int RoundValuesToNDecimals { get; set; } = 5;

        //we use the "ulong" version to track whole numbers
        //and the "double" version to track relative frequencies
        ConcurrentDictionary<string, ulong[]> EntryFreqTracker_Long { get; set; }
        ConcurrentDictionary<string, double[]> EntryFreqTracker_Double { get; set; }
        ConcurrentDictionary<string, Dictionary<string, double>> TermVariancesRaw { get; set; }
        ConcurrentDictionary<string, Dictionary<string, double>> TermVariancesOneHot { get; set; }
        ConcurrentDictionary<int, Dictionary<string, double>> CategoryVariancesRaw { get; set; }
        ConcurrentDictionary<int, Dictionary<string, double>> CategoryVariancesOneHot { get; set; }



        //here, we're setting up a map that tells us where each output category is located in the output array for each word
        //note that we're using the category *numbers* (or the category identifies, whatever they are) and not the category *names*
        //this means that we can just do it right from the dictionary mappings themselves, rather than having to route around a bunch
        private Dictionary<string, int> OutputDataMap { get; set; }
        private int TotalCatCount { get; set; } = 0;

        private ConcurrentDictionary<string, ulong> TotalNumberOfDocs;



        public Icon GetPluginIcon
        {
            get
            {
                return Properties.Resources.icon;
            }
        }

        #endregion



        public void ChangeSettings()
        {

            using (var form = new SettingsForm_ExamineDictWords(UserLoadedDictionary, RawFreqs, DictionaryLocation, IncludeStDevs, RoundValuesToNDecimals))
            {

                form.Icon = Properties.Resources.icon;
                form.Text = PluginName;


                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    RawFreqs = form.RawFreqs;
                    UserLoadedDictionary = form.DictDataToReturn;
                    DictionaryLocation = form.SelectedDictionaryFileLocation;
                    RoundValuesToNDecimals = form.RoundLength;
                    IncludeStDevs = form.IncludeStDevSetting;
                }
            }

        }


        public Payload RunPlugin(Payload Input)
        {



            Payload pData = new Payload();
            pData.FileID = Input.FileID;
            pData.SegmentID = Input.SegmentID;

            for (int i = 0; i < Input.StringArrayList.Count; i++)
            {


                TotalNumberOfDocs.AddOrUpdate("Docs", 1, (id, count) => count + 1);

                //keep a clean local copy of which document we're on.
                //we need this to calculate variances
                ulong k = 1;
                TotalNumberOfDocs.TryGetValue("Docs", out k);


                //if we have more than 0 words
                if (Input.StringArrayList[i].Length > 0) { 

                    
                    Dictionary<string, ulong[]> Results = AnalyzeText(UserLoadedDictionary.DictData, Input.StringArrayList[i]);

                    


                    //now that we've got our results, we want to add them into our concurrent dictionaries
                    foreach (string key in Results.Keys)
                    {
                        for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++) EntryFreqTracker_Long[key][j] += Results[key][j];
                        for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++) EntryFreqTracker_Double[key][j] += ((double)Results[key][j] / Input.StringArrayList[i].Length);
                    }



                    #region Welford's Method for online variance calculation

                    //set up  dictionaries to aggregate category values, for variance tracking
                    Dictionary<int, double> TempCatSumTrackerRaw = new Dictionary<int, double>();
                    Dictionary<int, double> TempCatSumTrackerOneHot = new Dictionary<int, double>();
                    for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++)
                    {
                        TempCatSumTrackerRaw.Add(j, 0);
                        TempCatSumTrackerOneHot.Add(j, 0);
                    }

                    //we also want to track the variance here for each word and category
                    //first, we have to figure out how many times the word occurred based on the results
                    for (int entryCounter = 0; entryCounter < UserLoadedDictionary.DictData.AllEntriesArray.Length; entryCounter++)
                    {

                        string term = UserLoadedDictionary.DictData.AllEntriesArray[entryCounter];

                        double x = 0;
                        double x_OneHot = 0;

                        if (Results.ContainsKey(term)) {

                            //if the term appears in the results at all, we know that it has some value
                            //so we can just set the "onehot" value to 1
                            x_OneHot = 1;

                            for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++)
                            {
                                if (Results[term][j] > 0)
                                {
                                    if (x == 0) x = (((double)Results[term][j] / (double)Input.StringArrayList[i].Length)) * 100;

                                    TempCatSumTrackerOneHot[j] += x_OneHot;
                                    TempCatSumTrackerRaw[j] += x;

                                }
                            }
                        }

                        //now that we know the number of times the word occurred, we can move forward
                        Dictionary<string, double> termTemp = new Dictionary<string, double>();
                        TermVariancesRaw.TryGetValue(term, out termTemp);

                        double oldM_Raw = termTemp["M"];
                        double oldS_Raw = termTemp["S"];
                        double newMean_Raw = oldM_Raw + ((x - oldM_Raw) / k);
                        double newS_Raw = oldS_Raw + ((x - newMean_Raw) * (x - oldM_Raw));
                        if (!Double.IsNaN(newMean_Raw - oldM_Raw)) TermVariancesRaw[term]["M"] += newMean_Raw - oldM_Raw;
                        if (!Double.IsNaN(newS_Raw - oldS_Raw)) TermVariancesRaw[term]["S"] += newS_Raw - oldS_Raw;


                        termTemp = new Dictionary<string, double>();
                        TermVariancesOneHot.TryGetValue(term, out termTemp);
                        double oldM_OneHot = termTemp["M"];
                        double oldS_OneHot = termTemp["S"];
                        double newMean_OneHot = oldM_OneHot + ((x_OneHot - oldM_OneHot) / k);
                        double newS_OneHot = oldS_OneHot + ((x_OneHot - newMean_OneHot) * (x_OneHot - oldM_OneHot));
                        if (!Double.IsNaN(newMean_OneHot - oldM_OneHot)) TermVariancesOneHot[term]["M"] += newMean_OneHot - oldM_OneHot;
                        if (!Double.IsNaN(newS_OneHot - oldS_OneHot)) TermVariancesOneHot[term]["S"] += newS_OneHot - oldS_OneHot;

                    }

                    //now we update our variance trackers at the "category sums" level
                    for (int catCounter = 0; catCounter < UserLoadedDictionary.DictData.NumCats; catCounter++)
                    {

                        Dictionary<string, double> catTemp = new Dictionary<string, double>();
                        CategoryVariancesRaw.TryGetValue(catCounter, out catTemp);

                        double x = TempCatSumTrackerRaw[catCounter];
                        double x_OneHot = TempCatSumTrackerOneHot[catCounter];

                        double oldM_Raw = catTemp["M"];
                        double oldS_Raw = catTemp["S"];
                        double newMean_Raw = oldM_Raw + ((x - oldM_Raw) / k);
                        double newS_Raw = oldS_Raw + ((x - newMean_Raw) * (x - oldM_Raw));
                        if (!Double.IsNaN(newMean_Raw - oldM_Raw)) CategoryVariancesRaw[catCounter]["M"] += newMean_Raw - oldM_Raw;
                        if (!Double.IsNaN(newS_Raw - oldS_Raw)) CategoryVariancesRaw[catCounter]["S"] += newS_Raw - oldS_Raw;


                        catTemp = new Dictionary<string, double>();
                        CategoryVariancesOneHot.TryGetValue(catCounter, out catTemp);

                        double oldM_OneHot = catTemp["M"];
                        double oldS_OneHot = catTemp["S"];
                        double newMean_OneHot = oldM_OneHot + ((x_OneHot - oldM_OneHot) / k);
                        double newS_OneHot = oldS_OneHot + ((x_OneHot - newMean_OneHot) * (x_OneHot - oldM_OneHot));
                        if (!Double.IsNaN(newMean_OneHot - oldM_OneHot)) CategoryVariancesOneHot[catCounter]["M"] += newMean_OneHot - oldM_OneHot;
                        if (!Double.IsNaN(newS_OneHot - oldS_OneHot)) CategoryVariancesOneHot[catCounter]["S"] += newS_OneHot - oldS_OneHot;

                    }
                    #endregion


                }




            }

            return (new Payload());
        }





        public void Initialize()
        {

            //TotalCatCount = 1;
            //Dictionary<string, int> TempOutputDataMap = new Dictionary<string, int>();
            //Dictionary<int, string> TempHeaderData = new Dictionary<int, string>();
            //TempHeaderData.Add(0, "TokenCount");

            #region This section may be uncommented later and modified to load a dictionary file that is imported from a pipeline file
            //UserLoadedDictionary.DictData = ParseDict(UserLoadedDictionary);

            ////add all of the categories to the header
            //for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++)
            //{



            //    int increment = 1;
            //    //makes sure that we don't have duplicate category names
            //    string CatNameRoot = UserLoadedDictionary.DictData.CatNames[j];
            //    while (TempOutputDataMap.ContainsKey(UserLoadedDictionary.DictionaryCategoryPrefix + UserLoadedDictionary.DictData.CatNames[j]))
            //    {
            //        UserLoadedDictionary.DictData.CatNames[j] = CatNameRoot + "_" + increment.ToString();
            //        increment++;
            //    }

            //    TempHeaderData.Add(TotalCatCount,
            //       UserLoadedDictionary.DictionaryCategoryPrefix + UserLoadedDictionary.DictData.CatNames[j]);

            //    TempOutputDataMap.Add(UserLoadedDictionary.DictionaryCategoryPrefix + UserLoadedDictionary.DictData.CatNames[j],
            //        TotalCatCount);
            //    TotalCatCount++;
            //}

            //OutputHeaderData = TempHeaderData;
            //OutputDataMap = TempOutputDataMap;
            #endregion

            TotalNumberOfDocs = new ConcurrentDictionary<string, ulong>();
            TotalNumberOfDocs.TryAdd("Docs", 0);
            OutputHeaderData = new Dictionary<int, string>();
            OutputDataMap = new Dictionary<string, int>();

            //here, we're setting up a map that tells us where each output category is located in the output array for each word
            //note that we're using the category *numbers* (or the category identifies, whatever they are) and not the category *names*
            //this means that we can just do it right from the dictionary mappings themselves, rather than having to route around a bunch
            for (int i = 0; i < UserLoadedDictionary.DictData.NumCats; i++) OutputDataMap.Add(UserLoadedDictionary.DictData.CatValues[i], i);
            for (int i = 0; i < UserLoadedDictionary.DictData.CatNames.Length; i++) OutputHeaderData.Add(i, UserLoadedDictionary.DictData.CatNames[i]);
            
            EntryFreqTracker_Long = new ConcurrentDictionary<string, ulong[]>();
            EntryFreqTracker_Double = new ConcurrentDictionary<string, double[]>();
            TermVariancesRaw = new ConcurrentDictionary<string, Dictionary<string, double>>();
            TermVariancesOneHot = new ConcurrentDictionary<string, Dictionary<string, double>>();
            CategoryVariancesRaw = new ConcurrentDictionary<int, Dictionary<string, double>>();
            CategoryVariancesOneHot = new ConcurrentDictionary<int, Dictionary<string, double>>();

            //set up our variance trackers for each category
            for (int i = 0; i < UserLoadedDictionary.DictData.NumCats; i++) CategoryVariancesRaw.TryAdd(i, new Dictionary<string, double> { { "M", 0 }, { "S", 0 } });
            for (int i = 0; i < UserLoadedDictionary.DictData.NumCats; i++) CategoryVariancesOneHot.TryAdd(i, new Dictionary<string, double> { { "M", 0 }, { "S", 0 } });


            for (int i = 0; i < UserLoadedDictionary.DictData.AllEntries.Count; i++)
            {
                //set up the arrays that we use to track each of the words' frequencies
                EntryFreqTracker_Long.TryAdd(UserLoadedDictionary.DictData.AllEntries[i], new ulong[UserLoadedDictionary.DictData.NumCats]);
                EntryFreqTracker_Double.TryAdd(UserLoadedDictionary.DictData.AllEntries[i], new double[UserLoadedDictionary.DictData.NumCats]);

                //set up the dictionary that we use to track each of the words' variances
                TermVariancesRaw.TryAdd(UserLoadedDictionary.DictData.AllEntries[i], new Dictionary<string, double> { { "M", 0 }, { "S", 0 } });
                TermVariancesOneHot.TryAdd(UserLoadedDictionary.DictData.AllEntries[i], new Dictionary<string, double> { { "M", 0 }, { "S", 0 } });

                for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++) 
                {
                    EntryFreqTracker_Long[UserLoadedDictionary.DictData.AllEntries[i]][j] = 0;
                    EntryFreqTracker_Double[UserLoadedDictionary.DictData.AllEntries[i]][j] = 0;
                }
                
            }
            


        }

        public bool InspectSettings()
        {
            if (String.IsNullOrEmpty(DictionaryLocation))
            {
                MessageBox.Show("The \"" + this.PluginName + "\" plugin does not appear to have a user dictionary loaded.", "Error Initializing Plugin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        



        //one of the few plugins thus far where I'm actually using a constructor
        //might not be the most efficient way to handle this (especially at runtime)
        //but I don't suspect that it'll be too bad.
        //public ExamineDictWords()
        //{
        //    DictionaryList = new List<DictionaryMetaObject>();
        //    ListOfBuiltInDictionaries = new HashSet<string>();

            
        //    foreach(DictionaryMetaObject dict in DictionaryList)
        //    {
        //        ListOfBuiltInDictionaries.Add(dict.DictionaryName);
        //    }

        //}




        public Payload FinishUp(Payload Input)
        {

            Payload OutputData = new Payload();
            OutputData.FileID = "";
            string StringOutputFormatParameter = "N" + RoundValuesToNDecimals.ToString();


            #region Cronbach's Alphas
            //first thing's first: we have to figure out which words constitute each category so that we can calculate Σ[s2i]
            Dictionary<int, List<string>> CatWordMap = new Dictionary<int, List<String>>();
            for (int i = 0; i < UserLoadedDictionary.DictData.NumCats; i++) CatWordMap.Add(i, new List<string>());

            #region Figure out which words belong to which category
            //iterate over n-grams, starting with the largest possible n-gram (derived from the user's dictionary file)
            for (int NumberOfWords = UserLoadedDictionary.DictData.MaxWords; NumberOfWords > 0; NumberOfWords--)
            {

                if (UserLoadedDictionary.DictData.FullDictionary["Standards"].ContainsKey(NumberOfWords))
                {
                    foreach(string term in UserLoadedDictionary.DictData.FullDictionary["Standards"][NumberOfWords].Keys)
                    {
                        for (int wordCatCount = 0; wordCatCount < UserLoadedDictionary.DictData.FullDictionary["Standards"][NumberOfWords][term].Length; wordCatCount++)
                        {
                            int outputCatMap = OutputDataMap[UserLoadedDictionary.DictData.FullDictionary["Standards"][NumberOfWords][term][wordCatCount]];
                            CatWordMap[outputCatMap].Add(term);
                        }
                    }
                }

                if (UserLoadedDictionary.DictData.FullDictionary["Wildcards"].ContainsKey(NumberOfWords))
                {
                    foreach (string term in UserLoadedDictionary.DictData.FullDictionary["Wildcards"][NumberOfWords].Keys)
                    {
                        for (int wordCatCount = 0; wordCatCount < UserLoadedDictionary.DictData.FullDictionary["Wildcards"][NumberOfWords][term].Length; wordCatCount++)
                        {
                            int outputCatMap = OutputDataMap[UserLoadedDictionary.DictData.FullDictionary["Wildcards"][NumberOfWords][term][wordCatCount]];
                            CatWordMap[outputCatMap].Add(term);
                        }
                    }
                }
            }
            #endregion


            #region Raw Cronbach
            OutputData.SegmentNumber.Add(0); //used as the entry number
            OutputData.SegmentID.Add("Cronbach's Alpha (Raw)"); //the dictionary entry
            string[] OutputArray_Cronbach = new string[UserLoadedDictionary.DictData.NumCats];

            //now, we go through each category and calculate the *raw* cronbach's alpha
            for (int i = 0; i < UserLoadedDictionary.DictData.NumCats; i++)
            {
                //this gets us to sum of variances for the category's constituent items
                double itemVarianceSum = 0;
                string[] catWordList = CatWordMap[i].ToArray();
                double k = (double)catWordList.Length;

                for (int j = 0; j < catWordList.Length; j++)
                {
                    double itemVariance = TermVariancesRaw[catWordList[j]]["S"] / (TotalNumberOfDocs["Docs"] - 1);
                    if (itemVariance > 0)
                    {
                        itemVarianceSum += itemVariance;
                    }
                    else
                    {
                        k -= 1;
                    }
                    
                }

                double totalVariance = CategoryVariancesRaw[i]["S"] / (TotalNumberOfDocs["Docs"] - 1);

                //https://data.library.virginia.edu/using-and-interpreting-cronbachs-alpha/
                //double CronbachRaw = (k / (k - 1)) * ((totalVariance - itemVarianceSum) / totalVariance);
                double CronbachRaw = ((double)k / (k - 1)) * (1 - (itemVarianceSum / totalVariance));
                if (!Double.IsNaN(CronbachRaw) && !Double.IsInfinity(CronbachRaw))
                {
                    OutputArray_Cronbach[i] = Math.Round(CronbachRaw, RoundValuesToNDecimals, MidpointRounding.AwayFromZero).ToString(StringOutputFormatParameter);
                }
                else
                {
                    OutputArray_Cronbach[i] = "N/A";
                }
            }
            OutputData.StringArrayList.Add(OutputArray_Cronbach);
            #endregion


            #region OneHot Cronbach
            OutputData.SegmentNumber.Add(0); //used as the entry number
            OutputData.SegmentID.Add("Kuder–Richardson Formula 20 (KR-20)"); //the dictionary entry
            OutputArray_Cronbach = new string[UserLoadedDictionary.DictData.NumCats];

            //now, we go through each category and calculate the *raw* cronbach's alpha
            for (int i = 0; i < UserLoadedDictionary.DictData.NumCats; i++)
            {
                //this gets us to sum of variances for the category's constituent items
                double itemVarianceSum = 0;
                string[] catWordList = CatWordMap[i].ToArray();
                double k = (double)catWordList.Length;

                for (int j = 0; j < catWordList.Length; j++)
                {
                    double itemVariance = TermVariancesOneHot[catWordList[j]]["S"] / (TotalNumberOfDocs["Docs"] - 1);
                    if (itemVariance > 0)
                    {
                        itemVarianceSum += itemVariance;
                    }
                    else
                    {
                        k -= 1;
                    }

                }

                double totalVariance = CategoryVariancesOneHot[i]["S"] / (TotalNumberOfDocs["Docs"] - 1);

                //double CronbachOneHot = (k / (k - 1)) * ((totalVariance - itemVarianceSum) / totalVariance);
                double CronbachOneHot = (k / (k - 1)) * (1 - (itemVarianceSum / totalVariance));
                if (!Double.IsNaN(CronbachOneHot) && !Double.IsInfinity(CronbachOneHot))
                {
                    OutputArray_Cronbach[i] = Math.Round(CronbachOneHot, RoundValuesToNDecimals, MidpointRounding.AwayFromZero).ToString(StringOutputFormatParameter);
                }
                else
                {
                    OutputArray_Cronbach[i] = "N/A";
                }

            }
            OutputData.StringArrayList.Add(OutputArray_Cronbach);
            #endregion



            #endregion


            #region this is where we calculate the avg percentages for each word
            for (int i = 0; i < UserLoadedDictionary.DictData.AllEntries.Count; i++)
            {
                OutputData.SegmentNumber.Add((ulong)(i + 1)); //used as the entry number
                OutputData.SegmentID.Add(UserLoadedDictionary.DictData.AllEntries[i]); //the dictionary entry

                string[] OutputArray = new string[UserLoadedDictionary.DictData.NumCats];
                for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++) OutputArray[j] = "";

                for (int j = 0; j < UserLoadedDictionary.DictData.NumCats; j++)
                {

                    //if we know that the mean is zero, then we just skip on to the next one
                    if (EntryFreqTracker_Long[UserLoadedDictionary.DictData.AllEntries[i]][j] == 0) continue;
                    
                    if (RawFreqs)
                    {
                        OutputArray[j] = Math.Round((EntryFreqTracker_Long[UserLoadedDictionary.DictData.AllEntries[i]][j] / (double)TotalNumberOfDocs["Docs"]), RoundValuesToNDecimals, MidpointRounding.AwayFromZero).ToString(StringOutputFormatParameter);
                    }
                    else
                    {
                        OutputArray[j] = Math.Round(((EntryFreqTracker_Double[UserLoadedDictionary.DictData.AllEntries[i]][j] / TotalNumberOfDocs["Docs"])) * 100, RoundValuesToNDecimals, MidpointRounding.AwayFromZero).ToString(StringOutputFormatParameter);
                    }

                    //calculate the standard deviation
                    if (IncludeStDevs) 
                    { 
                        OutputArray[j] += " (" + Math.Round(Math.Sqrt(TermVariancesRaw[UserLoadedDictionary.DictData.AllEntries[i]]["S"] / (TotalNumberOfDocs["Docs"] - 1)), RoundValuesToNDecimals, MidpointRounding.AwayFromZero).ToString(StringOutputFormatParameter) + ")";
                    }

                }

                
                OutputData.StringArrayList.Add(OutputArray);

            }
            #endregion



            EntryFreqTracker_Long = new ConcurrentDictionary<string, ulong[]>();
            EntryFreqTracker_Double = new ConcurrentDictionary<string, double[]>();

            return (OutputData);
        }





        #region Import/Export Settings
        public void ImportSettings(Dictionary<string, string> SettingsDict)
        {
            RawFreqs = Boolean.Parse(SettingsDict["RawFreqs"]);
            IncludeStDevs = Boolean.Parse(SettingsDict["IncludeStDev"]);
            RoundValuesToNDecimals = Int32.Parse(SettingsDict["RoundLength"]);

            if (SettingsDict.ContainsKey("DictionaryLocation"))
            {
                DictionaryLocation = SettingsDict["DictionaryLocation"];
                UserLoadedDictionary = new DictionaryMetaObject(DictionaryLocation, "User Loaded Dictionary", "", SettingsDict["DictionaryContents"]);

                try
                {
                    DictParser DP = new DictParser();
                    UserLoadedDictionary.DictData = DP.ParseDict(UserLoadedDictionary);
                }
                catch
                {
                    DictionaryLocation = "";
                    UserLoadedDictionary = null;
                }

            }
            

            

        }


        public Dictionary<string, string> ExportSettings(bool suppressWarnings)
        {
            Dictionary<string, string> SettingsDict = new Dictionary<string, string>();
            SettingsDict.Add("RawFreqs", RawFreqs.ToString());

            //if we have a dictionary loaded, then we'll save it
            if (!String.IsNullOrEmpty(DictionaryLocation))
            {
                SettingsDict.Add("DictionaryLocation", DictionaryLocation.ToString());
                SettingsDict.Add("DictionaryContents", UserLoadedDictionary.DictionaryRawText);
                SettingsDict.Add("IncludeStDev", IncludeStDevs.ToString());
                SettingsDict.Add("RoundLength", RoundValuesToNDecimals.ToString());
            }

            return (SettingsDict);

            
        }
        #endregion






        //a nice, thread-safe way to track the number of documents that we're including
        //private static void IncrementDocCount()
        //{
        //    Interlocked.Increment(ref TotalNumberOfDocs);
        //}



    }
}
