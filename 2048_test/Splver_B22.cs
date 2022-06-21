using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


public class B22Solver : SolveAgent
{
    public B22Solver(Random random) : base(random) { }

    public override string solve(List<GameField> history)
    {
        GameField latest = history[history.Count - 1];

        return MonteCarlo(latest);
        
    }

    public string MonteCarlo(GameField latest)
    {
        Dictionary<string, double> score = new Dictionary<string, double>() { { "Up", 0 }, { "Down", 0 }, { "Left", 0 }, { "Right", 0 } };
        Dictionary<string, double> count = new Dictionary<string, double>() { { "Up", 0 }, { "Down", 0 }, { "Left", 0 }, { "Right", 0 } };

        int trials = 1000;
        int depth = 4;

        for (int i = 0; i < trials; i++)
        {
            GameField test = new GameField(latest);
            string firstDirection = "";


            int cnt;
            for (cnt = 0; cnt < depth; cnt++)
            {
                if (test.checkmate())
                {
                    score[firstDirection] -= 100;
                    break;
                }else if (test.composed())
                {
                    score[firstDirection] += 200;
                    break;
                }
                else
                {
                    Challenge(test, ref firstDirection);
                }
            }
            if(cnt == depth)
            {
                score[firstDirection] += Evaluation.eval(test);
                
            }

            count[firstDirection]++;
        }

        foreach (string direction in new string[] { "Up", "Down", "Left", "Right" })
        {
            if (count[direction] != 0)
            {
                score[direction] /= count[direction];
            }
            else
            {
                score[direction] = double.NegativeInfinity;
            }
        }

        Console.WriteLine("---score---");
        foreach (KeyValuePair<string, double> dicItem in score)
        {
            Console.WriteLine("[{0}:{1}]", dicItem.Key, dicItem.Value);
        }
        Console.WriteLine("---count---");
        foreach (KeyValuePair<string, double> dicItem in count)
        {
            Console.WriteLine("[{0}:{1}]", dicItem.Key, dicItem.Value);
        }
        return score.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }

    public void Challenge(GameField test, ref string firstDirection)
    {
        HashSet<string> candidates = new HashSet<string>();
        foreach (var x in new string[] { "Up", "Down", "Left", "Right" }) { if (test.isValidSlide(x)) { candidates.Add(x); } }
        string[] arrayed = candidates.ToArray();
        string direction_random = arrayed[rand.Next(arrayed.Length)];

        test.slide(direction_random);
        Tuple<Tuple<int, int>, int> placement = place(test);
        test.putNewPieceAt(placement.Item1, placement.Item2);

        if (firstDirection == "")
        {
            firstDirection = direction_random;
        }
    }

    public Tuple<Tuple<int, int>, int> place(GameField test)
    {
        Tuple<int, int>[] candidates =
        Enumerable.Range(0, test.size.Item1).SelectMany(row => Enumerable.Range(0, test.size.Item2), (first, second) => new Tuple<int, int>(first, second)).Where(x => test.isValidPiecePlacement(x)).ToArray();

        return new Tuple<Tuple<int, int>, int>(candidates[rand.Next(candidates.Length)], rand.Next(5) == 0 ? 4 : 2);
    }

}



class Evaluation
{
    public static double eval(GameField gf)
    {                           //0.5に変更                          //log()を追加
        return smoothness(gf) * 0.1 + Monotonicity(gf) * 1.0 + Math.Log(emptyCells(gf)) * 2.7 + MaxValue(gf) * 1.0;
    }

    static double emptyCells(GameField gf)
    {
        double count = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Tuple<int, int> P = new Tuple<int, int>(i, j);
                if (gf[P] == 0)
                {
                    count++;
                }
            }
        }
        if(count == 0) {
            return 0.5;
        }
        else {
            return count;
        }
    }

    static double MaxValue(GameField gf)
    {
        return Math.Log(gf.maxpiece, 2);
    }

    static double smoothness(GameField gf)
    {
        double smoothness = 0;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Tuple<int, int> P = new Tuple<int, int>(x, y);
                if (gf[P] != 0)
                {
                    double value = Math.Log(gf[P]) / Math.Log(2);

                    foreach (var vector in new string[] { "Right", "Down" })
                    {
                        Tuple<int, int> targetCell = findFathestPosition(gf, P, vector);

                        if (gf[targetCell] != 0)
                        {
                            int target = gf[targetCell];
                            double targetValue = Math.Log(target) / Math.Log(2);
                            smoothness -= Math.Abs(value - targetValue);
                        }
                    }
                }
            }
        }
        return smoothness;
    }

    static Tuple<int, int> findFathestPosition(GameField gf, Tuple<int, int> P, string vector)
    {
        Tuple<int, int> cell;
        int count = 1;
        if (vector == "Right")
        {
            while (gf.isValidPiecePlacement(new Tuple<int, int>(P.Item1 + count, P.Item2)) && P.Item1 + count < 3) count++;
            cell = P.Item1 + count < 4 ? new Tuple<int, int>(P.Item1 + count, P.Item2) : P;
        }
        else
        {
            while (gf.isValidPiecePlacement(new Tuple<int, int>(P.Item1, P.Item2 + count)) && P.Item2 + count < 3) count++;
            cell = P.Item2 + count < 4 ? new Tuple<int, int>(P.Item1, P.Item2 + count) : P;
        }
        return cell;
    }

    static double Monotonicity(GameField target)
    {
        double[][] tmp_grid = ToJaggedArray(GetArary2d(target));
        double[][] grid = new double[4][];
        double[] monotonicities = new double[4];
        for (int i = 0; i < 4; i++) grid[i] = (tmp_grid[i].Select(x => x == 0 ? 0 : Math.Log(x, 2)).ToArray());
        
        //6/2変更
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var tmp = grid[i][j] - grid[i + 1][j];
                if (tmp > 0) monotonicities[2] += -1 * tmp;
                else monotonicities[3] += tmp;
            }
        }
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var tmp = grid[j][i] - grid[j][i + 1];
                if (tmp > 0) monotonicities[0] += -1 * tmp;
                else monotonicities[1] += tmp;
            }
        }
        return Math.Max(monotonicities[0], monotonicities[1]) + Math.Max(monotonicities[2], monotonicities[3]);

    }
    static int[,] GetArary2d(GameField target)
    {
        Type type = target.GetType();
        FieldInfo field = type.GetField("arary2d", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        return (int[,])field.GetValue(target);
    }

    static double[][] ToJaggedArray(int[,] array)
    {
        var jagged = new double[array.GetLength(0)][];
        for (int i = 0; i < array.GetLength(0); i++)
        {
            jagged[i] = new double[array.GetLength(1)];
            for (int j = 0; j < array.GetLength(1); j++)
            {
                jagged[i][j] = (double)array[i, j];
            }
        }
        return jagged;
    }
}


//メモ
/*
     public static Tuple<int, int> findFathestPosition(GameField gf, Tuple<int, int> P, string vector)

    {

        Tuple<int, int> cell = new Tuple<int, int>(P.Item1, P.Item2);

        if(vector == "Right") {

            do {

                cell = new Tuple<int, int>(cell.Item1, cell.Item2 + 1);

                if(cell.Item2 >= 4) {

                    cell = new Tuple<int, int>(cell.Item1, cell.Item2 - 1);

                    break;

                }

            } while (gf.isValidPosition(cell) && gf[cell] == 0);

        }

        else {

            do {

                cell = new Tuple<int, int>(cell.Item1 + 1, cell.Item2);

                if(cell.Item1 >= 4) {

                    cell = new Tuple<int, int>(cell.Item1 - 1, cell.Item2);

                    break;

                }

            } while (gf.isValidPosition(cell) && gf[cell] == 0);

        }

        

        return cell;

    }
 */


//AIクラスの中身（一応残してある）
/*public string ChooseBestVector(GameField gf_now)
    {
        string direction = "";






        return direction;
    }

    static double Value_Computer(GameField gf_now, Tuple<int, int> piece_code, int piece_value)//場所が有効化は判定せず
    {
        GameField gf_next = gf_now.putNewPieceAt(piece_code, piece_value);
        return Evaluation.eval(gf_next);
    }

    static double Value_Player(GameField gf_now, int vector_number)
    {
        string vector = GetVector(vector_number);
        GameField gf_next = gf_now.slide(vector);
        return gf_next != gf_now ? Evaluation.eval(gf_next) : 0;//なんも変化なかった時の評価 ===> 0
    }

    static string GetVector(int vector_numver)
    {
        string[] vector = new string[4] { "Up", "Down", "Left", "Right" };
        return vector[vector_numver];
    }*/