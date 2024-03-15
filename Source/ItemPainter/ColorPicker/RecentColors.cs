using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace ColorPicker
{
    public class RecentColors
    {
        private const int Max = 20;
        private static List<Color> _colors = new List<Color>();

        static RecentColors()
        {
            Read();
        }

        public Color this[int index] => _colors[index];

        public int Count => _colors.Count;

        public void Add(Color color)
        {
            _colors.RemoveAll(c => c == color);
            _colors.Insert(0, color);
            while (_colors.Count > 20)
            {
                _colors.RemoveAt(_colors.Count - 1);
            }
            Write();
        }

        private static void Read()
        {
            var text = Path.Combine(GenFilePaths.ConfigFolderPath, "ColourPicker.xml");
            var flag = !File.Exists(text);
            if (!flag)
                try
                {
                    Scribe.loader.InitLoading(text);
                    ExposeData();
                }
                catch (Exception ex)
                {
                    const string str = "ColourPicker :: Error loading recent colours from file:";
                    var ex2 = ex;
                    Log.Error($"{str}{(ex2 != null ? ex2.ToString() : null)}");
                }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                }
        }

        private static void Write()
        {
            try
            {
                var text = Path.Combine(GenFilePaths.ConfigFolderPath, "ColourPicker.xml");
                Scribe.saver.InitSaving(text, "ColourPicker");
                ExposeData();
            }
            catch (Exception ex)
            {
                const string str = "ColourPicker :: Error saving recent colours to file:";
                Log.Error($"{str}{ex?.ToString()}");
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
        }

        private static void ExposeData()
        {
            Scribe_Collections.Look(ref _colors, "RecentColors", 0, Array.Empty<object>());
        }
    }
}