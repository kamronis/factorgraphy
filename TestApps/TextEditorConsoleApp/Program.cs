using System;
using System.IO;
using System.Globalization;

// Программа для обработки текста. Программа принимает на вход файл на диске.

// Вы можете:
// a.произвести удаление слов длиной менее какого-либо числа символов;
// b. произвести удаление знаков препинания.

// Чтобы запустить приложение, нажмите ctrl+f5
public class TextEditor
{
    public static int Main()
    {
        int maxLenOfWord;
        int keypunct;
        FileStream fileStream = null;

        Console.WriteLine("Введите путь к файлу, который необходимо отредактировать");
        string path_in = Console.ReadLine();
        while (!File.Exists(path_in))
        {
            Console.WriteLine("Не удается найти указанный файл. Повтроите попытку");
            path_in = Console.ReadLine();
        }
        Console.WriteLine("Введите путь к файлу, в который будет помещен результат. Если файла не существует, то создастся новый");
        string path_out = Console.ReadLine();
        if (!File.Exists(path_out))
        {
            fileStream = File.Create(path_out); // создаем файл
        }

        Console.WriteLine("Укажите максимальную длину слова. Слова  с меньшей длиной удалятся");
        maxLenOfWord = Convert.ToInt32(Console.ReadLine()); // слова, имеющие меньшую длину, удаляются
        Console.WriteLine($"Удалятся слова меньшие {maxLenOfWord} символов");

        Console.WriteLine("Введите цифру 0, если необходимо удалить знаки препинания, любую другую - иначе");
        keypunct = Convert.ToInt32(Console.ReadLine());
        if (keypunct == 0)
        {
            Console.WriteLine("Знаки препинания удалятся");
        }
        else
        {
            Console.WriteLine("Знаки препинания не удалятся");
        }

        string[] args = new string[] { path_in, path_out };

        try
        {
            // Попытка открыть входной файл
            using (var writer = new StreamWriter(args[1]))
            {
                using (var reader = new StreamReader(args[0]))
                {
                    // Перенаправление стандартного вывода с консоли в выходной файл.
                    Console.SetOut(writer);
                    // Перенаправление стандартного ввода с консоли во входной файл.
                    Console.SetIn(reader);

                    string line;
                    while ((line = Console.ReadLine()) != null)  // читаем файл построчно
                    {
                        int len = line.Length;
                        int i = 0; //индекс символа
                        int j = 0; //длина слова

                        if (keypunct == 0) // удаляем знаки препинания
                        {
                            while (i < len)
                            {
                                if (line[i] == ',' || line[i] == '.' || line[i] == ';')
                                {

                                    if (j < maxLenOfWord) // удаляем слово и знак препинания
                                    {
                                        line = line.Remove(i - j, j + 1);
                                        len = line.Length;
                                        i = i - j;
                                        j = 0;
                                    }
                                    else // не удаляем слово, удаляем только знак препинания
                                    {
                                        line = line.Remove(i, 1);
                                        len--;
                                        i--;
                                    }
                                }
                                else // не знак препинания
                                    if (line[i] != ' ') // не пробел
                                {
                                    i++; // идем дальше
                                    j++; // по слову
                                }

                                else // пробел
                                    if (j < maxLenOfWord) // удаляем слово и знаки перпинания
                                {
                                    line = line.Remove(i - j, j + 1);
                                    len = line.Length;
                                    i = i - j;
                                    j = 0;
                                }
                                else // не удаляем слово, идем далее
                                {
                                    i++;
                                    j = 0;
                                }
                            }
                        }
                        else // не удаляем знаки препинания
                        {
                            while (i < len)
                            {
                                if (line[i] == ',' || line[i] == '.' || line[i] == ';') // знак препинания
                                {
                                    if (j < maxLenOfWord)
                                    {
                                        if (i == j) // первое слово в файле
                                        {
                                            line = line.Remove(i - j, j);
                                            len = line.Length;
                                            i = i - j + 2;
                                            j = 0;
                                        }
                                        else
                                        {
                                            line = line.Remove(i - j, j);
                                            len = line.Length;
                                            i = i - j + 2;
                                            j = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (i == len - 1) break; // конец
                                        i++;
                                    }
                                }
                                else if (line[i] != ' ')
                                {
                                    i++;
                                    j++;
                                }

                                else // пробел
                                    if (j < maxLenOfWord)
                                {
                                    line = line.Remove(i - j, j + 1);
                                    len = line.Length;
                                    i = i - j;
                                    j = 0;
                                }
                                else
                                {
                                    i++;
                                    j = 0;
                                }
                            }
                        }

                        if (j < maxLenOfWord) // последнее слово в файле
                        {
                            line = line.Remove(i - j, j);
                        }

                        string newLine = line;
                        Console.WriteLine(newLine); // пишем результирующую строку в выодной файл
                    }
                }
            }
        }
        catch (IOException e)
        {
            TextWriter errorWriter = Console.Error;
            errorWriter.WriteLine(e.Message);
            return 1;
        }

        // Восстановление стандартного выходного потока,
        // чтобы можно было отобразить сообщение о завершении.
        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
        standardOutput.AutoFlush = true;
        Console.SetOut(standardOutput);
        Console.WriteLine($"EDITTEXT has completed the processing of {args[0]}.");
        return 0;
    }
}