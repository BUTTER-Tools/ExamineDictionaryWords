using PluginContracts;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExamineDictWords
{


    public partial class ExamineDictWords : Plugin
    {



        private Dictionary<string, ulong[]> AnalyzeText(DictionaryData DictData, string[] Words)
        {

            //this matches the EntryFreqTracker dictionary from the main part of the plugin
            //we'll build this up here, then aggregate it back into the main version once we return
            //the results from this method
            Dictionary<string, ulong[]> WordsCaptured_Raw = new Dictionary<string, ulong[]>();
            


            int TotalStringLength = Words.Length;

            Dictionary<string, int> DictionaryResults = new Dictionary<string, int>();
            for (int i = 0; i < DictData.NumCats; i++) DictionaryResults.Add(DictData.CatValues[i], 0);

            for (int i = 0; i < TotalStringLength; i++)
            {



                //iterate over n-grams, starting with the largest possible n-gram (derived from the user's dictionary file)
                for (int NumberOfWords = DictData.MaxWords; NumberOfWords > 0; NumberOfWords--)
                {

                    //make sure that we don't overextend past the array
                    if (i + NumberOfWords - 1 >= TotalStringLength) continue;

                    //make the target string

                    string TargetString;

                    if (NumberOfWords > 1)
                    {
                        TargetString = String.Join(" ", Words.Skip(i).Take(NumberOfWords).ToArray());
                    }
                    else
                    {
                        TargetString = Words[i];
                    }


                    //look for an exact match

                    if (DictData.FullDictionary["Standards"].ContainsKey(NumberOfWords))
                    {

                        //this is what we do when a word is captured
                        if (DictData.FullDictionary["Standards"][NumberOfWords].ContainsKey(TargetString))
                        {

                            //make sure that the word is contained in our tracking dictionary
                            if (!WordsCaptured_Raw.ContainsKey(TargetString))
                            {
                                WordsCaptured_Raw.Add(TargetString, new ulong[UserLoadedDictionary.DictData.NumCats]);
                            }

                            //we iterate over each category that the word belongs to, and we increment it accordingly
                            for (int j = 0; j < DictData.FullDictionary["Standards"][NumberOfWords][TargetString].Length; j++)
                            {
                                int CategoryOutputPosition = OutputDataMap[DictData.FullDictionary["Standards"][NumberOfWords][TargetString][j]];
                                //right now I'm just adding 1, but later
                                //i might revisit and add "NumberOfWords" instead
                                //there's no objectively right answer
                                WordsCaptured_Raw[TargetString][CategoryOutputPosition] += 1;
                            }

                            //manually increment the for loop so that we're not testing on words that have already been picked up
                            i += NumberOfWords - 1;
                            //break out of the lower level for loop back to moving on to new words altogether
                            break;
                        }
                    }



                    //if there isn't an exact match, we have to go through the wildcards
                    if (DictData.WildCardArrays.ContainsKey(NumberOfWords))
                    {
                        for (int j = 0; j < DictData.WildCardArrays[NumberOfWords].Length; j++)
                        {
                            if (DictData.PrecompiledWildcards[DictData.WildCardArrays[NumberOfWords][j]].Matches(TargetString).Count > 0)
                            {

                                //make sure that the word is contained in our tracking dictionary
                                if (!WordsCaptured_Raw.ContainsKey(DictData.WildCardArrays[NumberOfWords][j]))
                                {
                                    WordsCaptured_Raw.Add(DictData.WildCardArrays[NumberOfWords][j], new ulong[UserLoadedDictionary.DictData.NumCats]);
                                }

                                for (int k = 0; k < DictData.FullDictionary["Wildcards"][NumberOfWords][DictData.WildCardArrays[NumberOfWords][j]].Length; k++)
                                {

                                    //if (DictionaryResults.ContainsKey(DictData.FullDictionary["Wildcards"][NumberOfWords][DictData.WildCardArrays[NumberOfWords][j]][k])) DictionaryResults[DictData.FullDictionary["Wildcards"][NumberOfWords][DictData.WildCardArrays[NumberOfWords][j]][k]] += NumberOfWords;
                                    //we iterate over each category that the word belongs to, and we increment it accordingly

                                        int CategoryOutputPosition = OutputDataMap[DictData.FullDictionary["Wildcards"][NumberOfWords][DictData.WildCardArrays[NumberOfWords][j]][k]];
                                        //right now I'm just adding 1, but later
                                        //i might revisit and add "NumberOfWords" instead
                                        //there's no objectively right answer
                                        WordsCaptured_Raw[DictData.WildCardArrays[NumberOfWords][j]][CategoryOutputPosition] += 1;
                                }
                                //manually increment the for loop so that we're not testing on words that have already been picked up
                                i += NumberOfWords - 1;
                                //break out of the lower level for loop back to moving on to new words altogether
                                break;

                            }
                        }
                    }


                }



            }


            return WordsCaptured_Raw;


        }


    }

}
