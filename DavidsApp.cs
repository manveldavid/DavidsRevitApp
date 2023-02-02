using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DavidsRevitApp
{
    internal class DavidsApp : IExternalApplication
    {
        public static string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        public static string DirOfAssembly = assemblyLocation.Substring(0,assemblyLocation.Length-"DavidsRevitApp.dll".Length)+ "DavidsRevitApp/";
        
        public static string[] IconPaths = new string[]/*Задаем пути к иконкам*/
            {
                DirOfAssembly+"Icons/Брус.ico",
                DirOfAssembly+"Icons/Ось.ico",
                DirOfAssembly + "Icons/Флажок.ico"
            };
        public Result OnStartup(UIControlledApplication application)
        {
            /*Задаем название вкладке*/
            string myTabName = "Плагины";

            /*Задаем названия панелям*/
            string[] myPanelsNames =
            new string[] {
                    "Стена из бруса",
                    "Оформление"
            };

            /*Задаем названия кнопкам*/
            string[] myCommandsNames =
            new string[] {
                    "Забивка брусом",
                    "Перенумерация осей",
                    "АвтоФлажок",
                    "Пометка стен помещением"
            };

            /*Список существующих классов команд*/
            Type[] myCommands =
            new Type[] {
                    typeof(WallSweeps.WallSweepsCommand),
                    typeof(ReNumGrid.RenumberingOfAxelsCommand),
                    typeof(AutoFlag.AutoFlagCommand),
                    typeof(RoomFinishing.RoomFinishingCommand)
            };

            //myCommands[0] = null;   //Так можно отключать ненужные функции
            myCommands[3] = null;

            /*Создаем все вкладки, панели, кнопки*/
            CreateTabs(application, assemblyLocation, myTabName, myPanelsNames, myCommandsNames, myCommands);

            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /*Создаем таблицу, добавляем в нее панели, а в панели кнопки с иконками*/
        public static void CreateTabs(UIControlledApplication application,
            string assemblyLocation, string myTabName, string[] myPanelsNames,
            string[] myComandsNames, Type[] myComands)
        {
            /*Создаем новую вкладку для плагина*/
            application.CreateRibbonTab(myTabName);

            /*Создаем список всех созданных панелей*/
            List<RibbonPanel> ribbons = new List<RibbonPanel>();
            /*Создаем список всех созданных кнопок*/
            List<PushButtonData> buttonData = new List<PushButtonData>();

            Dictionary<string, string[]> dictOfRibbonsMembers = new Dictionary<string, string[]>();
            /*Сопоставляем кнопки панелям*/
            {
                dictOfRibbonsMembers.Add(myPanelsNames[0], new string[]
                    {
                        myComandsNames[0]
                    });

                dictOfRibbonsMembers.Add(myPanelsNames[1], new string[]
                    {
                        myComandsNames[1],
                        myComandsNames[2],
                        myComandsNames[3]
                    });
            }

            Dictionary<string, Icon> dictOfBtnIcon = new Dictionary<string, Icon>();
            /*Сопоставляем иконки кнопкам*/
            for (int i = 0; i < IconPaths.Length; i++)
            {
                dictOfBtnIcon.Add(myComandsNames[i],
                    File.Exists(IconPaths[i]) ? new Icon(IconPaths[i]) : null);
            }

            /*Циклом проходим по каждому классу команды*/
            for (int i = 0; i < myComands.Count(); i++)
                if (myComands[i] != null)
                {
                    RibbonPanel newRibbon = null;
                    /*Создаем все панели из списка*/
                    if (i < myPanelsNames.Length) newRibbon =
                    application.CreateRibbonPanel(myTabName, dictOfRibbonsMembers.First(o => o.Value.Contains(myComandsNames[i])).Key);

                    /*Добавляем панель в список, если ее там нет*/
                    if (!ribbons.Contains(newRibbon) && newRibbon != null)
                        ribbons.Add(newRibbon);

                    /*Добавляем новую кнопку для команды*/
                    buttonData.Add(new PushButtonData(myComands[i].Name, myComandsNames[i],
                            assemblyLocation, myComands[i].ToString()));

                    /*Создаем иконку, если файл существует и присваиваем её кнопке*/
                    Icon tempIcon = dictOfBtnIcon.ContainsKey(myComandsNames[i]) ?
                        dictOfBtnIcon[myComandsNames[i]] : null;

                    if (tempIcon != null)
                        buttonData.Last().LargeImage = Imaging
                            .CreateBitmapSourceFromHIcon(tempIcon.Handle,
                            Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    /*Добавляем кнопку к нужной панели по словарю*/
                    foreach (var el in ribbons)
                        if (dictOfRibbonsMembers[el.Name].Contains(myComandsNames[i]))
                            el.AddItem(buttonData.Last());
                }
        }
    }
}
