using NUnit.Framework;
using ShowWhatProcessLocksFile.LockingProcessesInfo.HandleExe;
using System;
using System.IO;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestHandleOutputParser
    {
        [Test]
        public void Parse_HandleExe_output_of_locked_folder()
        {
            var output = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, @"TestData\HandleExeOutput.txt"));
            var parsedData = HandleOutputParser.Parse(output).ToList();

            Assert.AreEqual(5, parsedData.Count);
            CheckParsedLineIsEqual(new HandleParsedLine(
                "cm d.exe",
                14116,
                HandleType.File,
                @"DESKTOP-NAME\user name",
                "54",
                @"C:\Users\username\Downloads\Han dle\1.txt"),
                parsedData[0]);
            CheckParsedLineIsEqual(new HandleParsedLine(
                "cm d.exe",
                14116,
                HandleType.File,
                @"DESKTOP-NAME\user name",
                "55",
                @"C:\Users\username\Downloads\Han dle\1.txt"),
                parsedData[1]);
            CheckParsedLineIsEqual(new HandleParsedLine(
                "handle.exe",
                14708,
                HandleType.File,
                @"DESKTOP-NAME\username",
                "64",
                @"C:\Users\username\Downloads\Handle\1.txt"),
                parsedData[2]);
            CheckParsedLineIsEqual(new HandleParsedLine(
                "handle.exe",
                14708,
                HandleType.File,
                @"DESKTOP-NAME\username",
                "94",
                @"C:\Users\username\Downloads\Handle"),
                parsedData[3]);
            CheckParsedLineIsEqual(new HandleParsedLine(
                "handle.exe",
                14708,
                HandleType.Section,
                @"DESKTOP-NAME\username",
                "94",
                @"\Users\username\Downloads\Handle"),
                parsedData[4]);
        }

        [Test]
        public void Parse_HandleExe_output_of_non_locked_folder()
        {
            var output = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, @"TestData\HandleExeOutput_nothingFound.txt"));
            var parsedData = HandleOutputParser.Parse(output).ToList();
            Assert.IsEmpty(parsedData);
        }

        private static void CheckParsedLineIsEqual(HandleParsedLine expected, HandleParsedLine actual)
        {
            Assert.AreEqual(expected.ProcessName, actual.ProcessName);
            Assert.AreEqual(expected.Pid, actual.Pid);
            Assert.AreEqual(expected.HandleType, actual.HandleType);
            Assert.AreEqual(expected.UserName, actual.UserName);
            Assert.AreEqual(expected.HandleCode, actual.HandleCode);
            Assert.AreEqual(expected.FileFullName, actual.FileFullName);
        }
    }
}
