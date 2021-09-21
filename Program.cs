using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Частотный_словарь
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Подсчёт символьных элементов, их удаление и преобразование текста в список лемм.
                TextCleaning();
                Console.ReadKey(true);
                // Удаление исходного файла с нелемматизированными словами и его замена на выходной отредактированный.
                FileCastling();
                // Удаление альтернативных (|) и возможных (?) вариантов
                LemmaCleaning();
                Console.ReadKey(true);
                // Общий частотный словарь в порядке возрастания.
                MyStem("-cigd output.txt outputMorph.txt");
                string[] partsOfSpeech = {"1. Все части речи", "2. Служебные части речи, удалено", "3. Имена прилагательные", "4. Глаголы", "5. Имена существительные"};
                foreach (string partOfSpeech in partsOfSpeech)
                {
                    PartOfSpeechLists(LemmaCleaning(), partOfSpeech);
                    FreqDict(LemmaCleaning(), "Слова по частям речи\\" + partOfSpeech + ".txt", partOfSpeech + ", частотный словарь.txt");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
        // Преобразование текста в список слов, записывающихся построчно.
        // Учёт и последующее удаление всех знаков препинания.
        // Замена всех пробельных элементов на абзацы.
        public static void TextCleaning()
        {
            // Открывает текстовый файл.
            using (StreamReader text = new StreamReader("text.txt", Encoding.UTF8))
            {
                // Прочитывает файл и выводит текст на экран.
                string line = text.ReadToEnd();
                Console.WriteLine(line);
                Console.ReadKey(true);
                // Создание массива знаков препинания и определение их количества в тексте. После подсчёта знака он удаляется из текста, если это - не дефис.
                line = SymbolsCounter(line);
                // Замена всех пробельных символов - пробелов, абзацев, табуляций - только на абзацы.
                line = Regex.Replace(line, @"\s+", "\n");
                // Запись текста без символов в текстовый файл.
                StreamWriter lineCleared = new StreamWriter("input.txt");
                lineCleared.Write(line);
                lineCleared.Close();
                // Вызов лемматизатора Mystem
                MyStem("-nld input.txt output.txt");
            }
        }
        // Подсчёт количества символьных элементов, в т.ч. знаков препинания.
        public static string SymbolsCounter(string line)
        {
            char[] listOfSymbols = { '.', '?', '!', ',', ';', ':', '(', ')', '-', '–', '—', '«', '»', '\"', '\'', '+', '*', '/', '=', '<', '>', '[', ']', '{', '}', '#', '№', '%', '$', '&', '^', '_' };
            for (int element = 0; element < listOfSymbols.Length; element++)
            {
                int symbolSum = 0;
                for (int symbolInText = 0; symbolInText < line.Length; symbolInText++)
                {
                    if (line[symbolInText].Equals(listOfSymbols[element])) symbolSum++;
                }
                Console.WriteLine("\nКоличество символов \"" + listOfSymbols[element] + "\" = " + symbolSum);
                if (element != 8) line = line.Replace(listOfSymbols[element].ToString(), "");
                else continue;
            }
            return line;
        }
        // Морфологический анализатор Mystem. Условия для отображения результата передаются в строку "tags".
        // Первый вызов метода преобразует слова текстового списка в их леммы - начальные формы.
        // Второй вызова метода выдаёт морфологические сведения о каждом слове, в т.ч. принадлежность к той или иной части речи.
        public static void MyStem(string tags)
        {
            Process mystem = new Process();
            mystem.StartInfo.FileName = "mystem.exe";
            mystem.StartInfo.Arguments = tags;
            mystem.StartInfo.UseShellExecute = false;
            mystem.StartInfo.RedirectStandardInput = true;
            mystem.StartInfo.RedirectStandardOutput = true;
            String outputText = " ";
            mystem.Start();
            StreamWriter mystemStreamWriter = mystem.StandardInput;
            StreamReader mystemStreamReader = mystem.StandardOutput;
            string bs = mystemStreamReader.ReadToEnd();
            mystemStreamWriter.Write(bs);
            mystemStreamWriter.Close();
            outputText += mystemStreamReader.ReadToEnd() + " ";
            mystem.WaitForExit();
            mystem.Close();
        }
        // Очистка лемм от альтернативных и возможных вариантов.
        public static int LemmaCleaning()
        {
            using (StreamReader textLem = new StreamReader("input.txt"))
            {
                string[] lineLem = textLem.ReadToEnd().Split('\n');
                for (int word = 0; word < lineLem.Length; word++)
                {
                    if (lineLem[word].Contains('?')) lineLem[word] = lineLem[word].Substring(0, lineLem[word].IndexOf('?'));
                    else if (lineLem[word].Contains('|')) lineLem[word] = lineLem[word].Substring(0, lineLem[word].IndexOf('|'));
                }
                File.WriteAllText("output.txt", string.Join("\n", lineLem));
                return lineLem.Length;
            }
        }
        // Рокировка текстовых файлов.
        public static void FileCastling()
        {
            File.Delete("input.txt");
            File.Move("output.txt", "input.txt");
        }
        // Частотный словарь. Принимает текстовые файлы со списками слов той или иной части речи.
        public static void FreqDict(int wordsInFullText, string adress, string newAdress)
        {
            Dictionary<string, int> vocabulary = new Dictionary<string, int>();
            string[] wordList = File.ReadAllLines(adress);
            foreach (string str in wordList)
            {
                foreach (string word in str.Split('\n'))
                {
                    if (vocabulary.ContainsKey(word)) vocabulary[word]++;
                    else vocabulary.Add(word, 1);
                }
            }
            StreamWriter FreqDict = new StreamWriter("Частотные словари\\" + newAdress);
            foreach (KeyValuePair<string, int> pair in vocabulary.OrderBy(pair => pair.Value)) Console.WriteLine("{0} = {1} => " + Math.Round(((double)pair.Value / (double)wordsInFullText * 100), 2) + "%", pair.Key, pair.Value);
            foreach (KeyValuePair<string, int> pair in vocabulary.OrderByDescending(pair => pair.Value)) FreqDict.Write("{0} = {1} => " + Math.Round(((double)pair.Value / (double)wordsInFullText * 100), 2) + "%\n", pair.Key, pair.Value);
            FreqDict.Close();
            Console.ReadKey(true);
        }
        // Формирование документов по частям речи.
        public static void PartOfSpeechLists(int wordsInFullText, string partOfSpeech)
        {
            using (StreamReader srService = new StreamReader("outputMorph.txt"))
            {
                string[] textMassive = srService.ReadToEnd().Split('\n');
                switch(partOfSpeech)
                {
                    case "1. Все части речи":
                        break;
                    case "2. Служебные части речи, удалено":
                        NoServicesTerms(textMassive, wordsInFullText);
                        break;
                    case "3. Имена прилагательные":
                        AdjectivesTerms(textMassive, wordsInFullText);
                        break;
                    case "4. Глаголы":
                        VerbsTerms(textMassive, wordsInFullText);
                        break;
                    case "5. Имена существительные":
                        NounsTerms(textMassive, wordsInFullText);
                        break;
                }
                TrashDelete(textMassive, partOfSpeech + ".txt");
            }
            Console.ReadKey(true);
        }
        // Условия выборки служебных частей речи.
        public static void NoServicesTerms(string[] textMassive, int wordsInFullText)
        {
            // Количество предлогов, союз и частиц соответственно.
            int pretextNum = 0, unionNum = 0, particleNum = 0;
            for (int textElement = 0; textElement < textMassive.Length; textElement++)
            {
                // Поиск частей речи по соответствующим тегам, которые прописаны в MyStem.
                // Большое количество условий необходимо для того, чтобы оставить сочетания только с "PR".
                if (textMassive[textElement].Contains("PR")
                && (textMassive[textElement].Contains("ADVPRO")) == false
                && (textMassive[textElement].Contains("APRO")) == false
                && (textMassive[textElement].Contains("SPRO")) == false)
                {
                    pretextNum++;
                    textMassive[textElement] = textMassive[textElement].Remove(0, textMassive[textElement].Length);
                }
                else if (textMassive[textElement].Contains("CONJ"))
                {
                    unionNum++;
                    textMassive[textElement] = textMassive[textElement].Remove(0, textMassive[textElement].Length);
                }
                else if (textMassive[textElement].Contains("PART"))
                {
                    particleNum++;
                    textMassive[textElement] = textMassive[textElement].Remove(0, textMassive[textElement].Length);
                }
            }
            Console.WriteLine("\nКоличество предлогов равно " + pretextNum + ".\n" +
                              "Количество союзов равно " + unionNum + ".\n" +
                              "Количество частиц равно " + particleNum + ".\n" +
                              "Общее количество служебных частей речи равно " + (pretextNum + unionNum + particleNum) + ".\n" +
                              "Доля служебных частей речи в тексте составляет " + Math.Round(((double)(pretextNum + unionNum + particleNum) / (double)wordsInFullText * 100), 2) + "%.\n");
        }
        // Очистка от пробельных элементов и морфологических показателей.
        public static void TrashDelete(string[] textMassive, string adress)
        {
            string textList = String.Join("\n", textMassive);
            textList = Regex.Replace(textList, @"\s+", "\n").Trim();
            textList = Regex.Replace(textList, @"{.*?}", "");
            File.WriteAllText("Слова по частям речи\\" + adress, textList);
        }
        // Условия выборки имён прилагательных.
        public static void AdjectivesTerms(string[] textMassive, int wordsInFullText)
        {
            int adjectiveNum = 0;
            for (int textElement = 0; textElement < textMassive.Length; textElement++)
            {
                // Поиск прилагательных по соответствующему тегу, который прописан в MyStem.
                // Большое количество условий необходимо для того, чтобы оставить сочетания только с "A".
                if (textMassive[textElement].Contains("A")
                && (textMassive[textElement].Contains("ADV")) == false
                && (textMassive[textElement].Contains("ADVPRO")) == false
                && (textMassive[textElement].Contains("ANUM")) == false
                && (textMassive[textElement].Contains("APRO")) == false
                && (textMassive[textElement].Contains("PART")) == false) adjectiveNum++;
                else textMassive[textElement] = textMassive[textElement].Remove(0, textMassive[textElement].Length);
                }
            Console.WriteLine("\nКоличество прилагательных равно " + adjectiveNum + ".\n" +
                              "Доля прилагательных в тексте составляет " + Math.Round(((double)adjectiveNum / (double)wordsInFullText * 100), 2) + "%.\n");
        }
        // Условия выборки глаголов.
        public static void VerbsTerms(string[] textMassive, int wordsInFullText)
        {
            int verbNum = 0;
            for (int textElement = 0; textElement < textMassive.Length; textElement++)
            {
                // Поиск глаголов по соответствующему тегу, который прописан в MyStem.
                // Большое количество условий необходимо для того, чтобы оставить сочетания только с "V".
                if (textMassive[textElement].Contains("V")
                && (textMassive[textElement].Contains("ADV")) == false
                && (textMassive[textElement].Contains("ADVPRO")) == false) verbNum++;
                else textMassive[textElement] = textMassive[textElement].Remove(0, textMassive[textElement].Length);
            }
            Console.WriteLine("\nКоличество глаголов равно " + verbNum + ".\n" +
                              "Доля глаголов в тексте составляет " + Math.Round(((double)verbNum / (double)wordsInFullText * 100), 2) + "%.\n");
        }
        // Условия выборки имён существительных.
        public static void NounsTerms(string[] textMassive, int wordsInFullText)
        {
            int nounNum = 0;
            for (int textElement = 0; textElement < textMassive.Length; textElement++)
            {
                // Поиск глаголов по соответствующему тегу, который прописан в MyStem.
                // Большое количество условий необходимо для того, чтобы оставить сочетания только с "V".
                if (textMassive[textElement].Contains("S")
                && (textMassive[textElement].Contains("SPRO")) == false) nounNum++;
                else textMassive[textElement] = textMassive[textElement].Remove(0, textMassive[textElement].Length);
            }
            Console.WriteLine("\nКоличество существительных равно " + nounNum + ".\n" +
                              "Доля существительных в тексте составляет " + Math.Round(((double)nounNum / (double)wordsInFullText * 100), 2) + "%.\n");
        }
    }
}
