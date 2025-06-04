using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.FileSystemGlobbing;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Math;
using Tesseract;
using TraderForStalCraft.Data.Serialize;
using TraderForStalCraft.Interfaces;
using TraderForStalCraft.Proprties;
using TraderForStalCraft.Scripts.HelperScripts;
using WindowsInput;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TraderForStalCraft.Scripts.MainScripts
{
	internal class StartingScript 
	{
        public bool IsRunning;

        private decimal _keyboardDelay;
        private decimal _mouseDelay;
        private Logger _logger;
        private ScreenProcessor _screenProcessor;
        private CompletePreparation start;
        private FileManager _fileManager;
        private SearchItems searchItems;
        private List<Rectangls> matches;
        private InputEmulator emulator;

        private int money;

        public StartingScript(decimal mouseDelay, decimal keyboardDelay, Logger logger, ScreenProcessor sp, FileManager fileManager)
        {
            _mouseDelay = mouseDelay;
            _keyboardDelay = keyboardDelay;
            _logger = logger;
            _screenProcessor = sp;
            start = new CompletePreparation(sp, fileManager);
            searchItems = new SearchItems(_screenProcessor);
            emulator = new InputEmulator();
            _fileManager = fileManager;
            money = 0;
        }

        internal void Start(Dictionary<string, int> itemsData, CancellationToken cts, bool skipPages)
        {
            IsRunning = true;
            start.StartSetup();
            matches = new List<Rectangls>(start.Matches);
            searchItems.matches.AddRange(matches);
            searchItems.isRunning = IsRunning;

            while (IsRunning && !cts.IsCancellationRequested)
            {
                foreach (var item in itemsData)
                {
                    emulator.MoveMouseAsync(start.SearchField.Location);
                    Thread.Sleep(200);
                    emulator.InputTextAsync(item.Key);
                    Thread.Sleep(200);
                    emulator.MoveMouseAsync(start.SearchButton.Location);
                    Thread.Sleep(200);


                    searchItems.StartSearch(item.Key, item.Value, start.GetMoney());

                    Thread.Sleep(200);
                    emulator.MoveMouseAsync(start.SearchField.Location);
                    Thread.Sleep(200);
                    emulator.ClearFieldAsync();
                }
            }
        }

        public void Stop()
        {
            searchItems.isRunning = false;
            IsRunning = false;
        }
    }
}