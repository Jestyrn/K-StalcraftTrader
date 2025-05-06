using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderForStalCraft.Proprties;
using TraderForStalCraft.Scripts.HelperScripts;

namespace TraderForStalCraft.Scripts.MainScripts
{
    internal class CompletePreparation
    {
        public static string pathToFile; 
        private bool IsSerialized;
        private List<Rectangls> matches;
        private Bitmap _screen;
        private ScreenProcessor _sp;
        private FileManager _fileManager;

        // выполнить подготовку
        // 1. проверить сериализацию (на существование)
        // 
        // 2. проеврить целостность сериализации
        // 3. обработать поле поиска
        // 4. проеврить сортировку

        public CompletePreparation(ScreenProcessor sp, FileManager fm)
        {
            _sp = sp;
            _fileManager = fm;
            pathToFile = Path.Combine(pathToFile, "Recsts.json");
        }

        public void StartSetup()
        {
            IsSerialized = DetermineStatus();

            if (IsSerialized)
                WithSerialization();
            else
                WithoutSerialization();

            CheckField();
            CheckSorting();
        }

        private bool DetermineStatus()
        {
            if (_fileManager.Exists(pathToFile))
            {
                List<Rectangls> rects = _fileManager.LoadFromJson<List<Rectangls>>(pathToFile);
                return CheckSerialization(rects);
            }
            else
            {
                return false;
            }
        }

        private bool CheckSerialization(List<Rectangls> ser)
        {
            // сделать логику проверки на корректность
            // Посмотреть наличие нужных точек
            // (Поле поиска, Кнопка сортировки, Состояние сортировки) 
            // +- сошлось - return true иначе false
            return false;
        }

        private void WithSerialization()
        {

        }

        private void WithoutSerialization()
        {
            _screen = _sp.CaptureScreen();
            matches = _sp.GetMatches(_screen);

            _fileManager.SaveToJson<List<Rectangls>>(pathToFile, matches);
        }

        private void CheckField()
        {

        }

        private void CheckSorting()
        {

        }
    }
}
