using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

public static class Program
{
    public static int[] Main_old(string[] args,Type type_solver)
    {
        int rand_seed = args.Length > 0 ? int.Parse(args[0]) : 0;
        string log_path = args.Length > 1 ? args[1] : "";

        Tuple<int, int> fieldsize = new Tuple<int, int>(4, 4);

        GameController cont = new GameController(fieldsize, rand_seed);

        cont.Run(typeof(B22Placer),type_solver, log_path : log_path );

        LinkedList<GameField> history=cont.history;

        int [] result=new int[2];
        result[0] = history.Last.Value.maxpiece;
        result[1] = cont.count;
        return result;
    }

    public static void Main()
    {
        //*********************
        int times =2;//10以上は、諸事情により10の倍数のみ
        string path_result = "ResultFile.txt";
        string path_log = "";//マルチタスクだとバグる
        Type type_solver = typeof(B22Solver);//()内変更
        //************************
        DateTime start =DateTime.Now;
        DateTime end;
        Random rand;
        double sum_count_total = 0;
        int[] log_score = new int[times];
        int[] log_count = new int[times];
        rand = new Random();
        int limit_a = 10;
        int a = times<limit_a?times:limit_a;
        times -= times%a;
        for (int i = 0; i <times/a ; i++)
        {
            List<Task<int[]>> tasks = new List<Task<int[]>>();
            for (int j = 0; j < a; j++)
            {
                tasks.Add(Task.Run(() =>
                {
                    Thread.Sleep(j);
                    string[] str = new string[2];
                    str[0] = rand.Next().ToString();
                    str[1] = path_log;
                    return Main_old(str,type_solver);
                })) ;
            }
            Task.WaitAll(tasks.ToArray());
            for (int j = 0; j < tasks.Count(); j++)
            {
                log_score[i*limit_a+j] = tasks[j].Result[0];
                log_count[i*limit_a+j] = tasks[j].Result[1];
                sum_count_total+=tasks[j].Result[1];
            }
        }
        



        end =DateTime.Now;
        int count_success = log_score.Count(x => x == 2048);
        int sum_count_success=0;
        double accuracy = (double)count_success/times;
        Console.WriteLine("精度 : "+accuracy);

        using (FileStream fs = File.Create("./"+path_result));
        using (StreamWriter w=new StreamWriter("./"+path_result,true, Encoding.GetEncoding("Shift_JIS")))
        {
            w.WriteLine("**************************\n");
            for (int i = 0; i < log_score.Length; i++)
            {
                w.WriteLine($"  score : {log_score[i]} count : {log_count[i]} ");
                if (log_score[i]==2048)sum_count_success += log_count[i];
            }
            string average_count= ""+(double)sum_count_success / count_success;
            w.WriteLine("\n**************************\n");
            w.WriteLine("Solver : "+type_solver.Name);
            w.WriteLine("試行回数 : " + times);
            w.WriteLine("精度 : " + accuracy);
            w.WriteLine("平均手数 （成功）: " + (double.TryParse( average_count,out double result)?average_count:"no data"));
            w.WriteLine("平均手数 （全部）: " + sum_count_total/times);
            w.WriteLine("平均所要時間（全部） : " +(end-start).TotalMinutes/times+"分");
            w.Close();

            openFile(path_result);
        }

        void openFile(string path)
        {
            Process ps = new Process();
            ps.StartInfo.FileName = path;
            ps.Start();
        }
    }
}