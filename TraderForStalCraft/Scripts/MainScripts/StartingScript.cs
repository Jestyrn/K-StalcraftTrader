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
using TraderForStalCraft.Scripts.HelperScripts;
using WindowsInput;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TraderForStalCraft.Scripts.MainScripts
{
	public class StartingScript
	{
        public bool IsRunning;

        private decimal _keyboardDelay;
        private decimal _mouseDelay;
        private Logger _logger;
        private ScreenProcessor _screenProcessor;
        private CompletePreparation start;
        private FileManager _fileManager;

        public StartingScript(decimal mouseDelay, decimal keyboardDelay, Logger logger, ScreenProcessor sp, FileManager fileManager)
        {
            _mouseDelay = mouseDelay;
            _keyboardDelay = keyboardDelay;
            _logger = logger;
            _screenProcessor = sp;
            start = new CompletePreparation(sp, fileManager);
            _fileManager = fileManager;
            // получить битмапы шаблонов (передать путь, записать в локальную переменную)
        }

        internal void Start(Dictionary<string, int> itemsData, CancellationToken cts, bool skipPages)
        {
            IsRunning = true;
            start.StartSetup();
            while (IsRunning && !cts.IsCancellationRequested)
            {
                foreach (var item in itemsData)
                {
                    
                }
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}