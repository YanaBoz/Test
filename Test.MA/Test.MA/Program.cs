using Test.MA;

Stroka str = new();

var gena = new List<Stroka>();

string[] files = new string[10];

int n = 10;

//var builder = new StringBuilder();

string path = "a";

string line;

string paths = @"V:\\file.txt";

StreamWriter sw1 = new(paths);
sw1.Close();

bool exist = false;

int count = 0;
try
{
    for (int o = 1; o <= n; o++)
    {
        StreamWriter sw = new(@"V:\\filename" + o + ".txt");
        files[o-1] = "V:\\filename" + o + ".txt";
        gena = str.Generate(1);
        for (int i = 0; i < gena.Count; i++)
        {
            sw.WriteLine($"{gena[i].Date:d}||{gena[i].Latin}||{gena[i].Rus}||{gena[i].Num_Int}||{gena[i].Num_Float}");
        }

        sw.Close();
    }
}
catch (Exception e)
{
    Console.WriteLine("Exception: " + e.Message);
}

foreach (var file in files)
{
    try
    {
        StreamReader sr = new(file);
        if (!File.Exists(path))
        {
            exist = true;

        }
        StreamWriter sw2 = new(paths, exist);
        line = sr.ReadLine();
        while (line != null)
        {
            if (line.Contains(path) is false)
            {
                sw2.WriteLine($"{line}");

            }
            else count++;
            line = sr.ReadLine();

        }
        sw2.WriteLine($"{count}");
        sr.Close();
        sw2.Close();
        //builder.Append(File.ReadAllText(file));
        //File.WriteAllText("V:\\fileOutput.txt", builder.ToString());
    }
    catch (Exception e)
    {
        Console.WriteLine("Exception: " + e.Message);
    }
    
}