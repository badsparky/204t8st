using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/*public class B22Placer : PlaceAgent
{
    public B22Placer(Random random) : base(random) {}

    public override Tuple<Tuple<int, int>, int> place(List<GameField> history)
    {
        GameField latest = history[history.Count - 1];
        Tuple<int, int>[] candidates = 
        Enumerable.Range(0, latest.size.Item1).SelectMany(row => Enumerable.Range(0, latest.size.Item2), (first, second) => new Tuple<int, int>(first, second)).Where(x => latest.isValidPiecePlacement(x)).ToArray();

        var answer = Montecarlo(candidates, latest);
        
        return new Tuple<Tuple<int, int>, int>(answer.Item1, answer.Item2);
    }

    public Tuple<Tuple<int, int>, int> Montecarlo(Tuple<int, int>[] candidates, GameField latest)
    {
        int depth = 100;
        double[] score_array = new double[candidates.Length*2];
        double min = 999999;
        int ans_number = 0;
        int[] candidates_number = { 2, 4 };
        Tuple<int, int> ans_coordinate = candidates[0];
        for(int i = 0; i <candidates_number.Length; i ++)
        {
            for(int j = 0; j < candidates.Length; j ++)
            {
                GameField test = new GameField(latest).putNewPieceAt(candidates[j], candidates_number[i]);
                for(int k = 0; k < depth; k++)
                {
                    while (!test.checkmate())
                    {
                        Challenge(test);
                        score_array[j]++;
                    }
                    score_array[(i+1)*j] += Math.Log(test.maxpiece, 2);
                }
                if(min > score_array[(i + 1) * j])
                {
                    ans_number = candidates_number[i];
                    ans_coordinate = candidates[j];
                    min = score_array[(i + 1) * j];
                }
            }
        }
        var ans = new Tuple<Tuple<int, int>, int>(ans_coordinate, ans_number);

        return ans;
    }
    public void Challenge(GameField test)
    {
        HashSet<string> candidates = new HashSet<string>();
        foreach (var x in new string[] { "Up", "Down", "Left", "Right" }) { if (test.isValidSlide(x)) candidates.Add(x); }
        string[] arrayed = candidates.ToArray();
        string direction_random = arrayed[rand.Next(arrayed.Length)];
        test.slide(direction_random);

        Tuple<Tuple<int, int>, int> placement = Randomplace(test);

        test.putNewPieceAt(placement.Item1, placement.Item2);

        
    }
    public  Tuple<Tuple<int, int>, int> Randomplace(GameField gf)
    {
        GameField latest = gf;
        Tuple<int, int>[] candidates =
        Enumerable.Range(0, latest.size.Item1).SelectMany(row => Enumerable.Range(0, latest.size.Item2), (first, second) => new Tuple<int, int>(first, second)).Where(x => latest.isValidPiecePlacement(x)).ToArray();

        return new Tuple<Tuple<int, int>, int>(candidates[rand.Next(candidates.Length)], rand.Next(5) == 0 ? 4 : 2);
    }

}*/

public class B22Placer : PlaceAgent
{
    public B22Placer(Random random) : base(random) { }

    public override Tuple<Tuple<int, int>, int> place(List<GameField> history)
    {
        GameField latest = history[history.Count - 1];
        Tuple<int, int>[] candidates =
        Enumerable.Range(0, latest.size.Item1).SelectMany(row => Enumerable.Range(0, latest.size.Item2), (first, second) => new Tuple<int, int>(first, second)).Where(x => latest.isValidPiecePlacement(x)).ToArray();

        var answer = min_eval(candidates, latest);

        return new Tuple<Tuple<int, int>, int>(answer.Item1, answer.Item2);
    }

    Tuple<Tuple<int,int>,int> min_eval(Tuple<int,int>[] candidates, GameField latest)
    {
        int[] hands = { 2, 4 };
        double min = double.PositiveInfinity;
        Tuple<Tuple<int, int>, int> action = new Tuple<Tuple<int, int>, int>(null,0);
        foreach(var c in candidates)
        {
            foreach(var a in hands)
            {
                GameField tmp = new GameField(latest).putNewPieceAt(c, a);
                double score = Evaluation2.eval(tmp);
                if(min > score)
                {
                    min = score;
                    action = new Tuple<Tuple<int, int>, int>(c, a);
                }
            }
        }
        return action;
    }


    public Tuple<Tuple<int, int>, int> Montecarlo(Tuple<int, int>[] candidates, GameField latest)
    {
        int depth = 100;
        double[] score_array = new double[candidates.Length * 2];
        double min = 999999;
        int ans_number = 0;
        int[] candidates_number = { 2, 4 };
        Tuple<int, int> ans_coordinate = candidates[0];
        for (int i = 0; i < candidates_number.Length; i++)
        {
            for (int j = 0; j < candidates.Length; j++)
            {
                GameField test = new GameField(latest).putNewPieceAt(candidates[j], candidates_number[i]);
                for (int k = 0; k < depth; k++)
                {
                    while (!test.checkmate())
                    {
                        Challenge(test);
                        score_array[j]++;
                    }
                    score_array[(i + 1) * j] += Math.Log(test.maxpiece, 2);
                }
                if (min > score_array[(i + 1) * j])
                {
                    ans_number = candidates_number[i];
                    ans_coordinate = candidates[j];
                    min = score_array[(i + 1) * j];
                }
            }
        }
        var ans = new Tuple<Tuple<int, int>, int>(ans_coordinate, ans_number);

        return ans;
    }
    public void Challenge(GameField test)
    {
        HashSet<string> candidates = new HashSet<string>();
        foreach (var x in new string[] { "Up", "Down", "Left", "Right" }) { if (test.isValidSlide(x)) candidates.Add(x); }
        string[] arrayed = candidates.ToArray();
        string direction_random = arrayed[rand.Next(arrayed.Length)];
        test.slide(direction_random);

        Tuple<Tuple<int, int>, int> placement = Randomplace(test);

        test.putNewPieceAt(placement.Item1, placement.Item2);


    }
    public Tuple<Tuple<int, int>, int> Randomplace(GameField gf)
    {
        GameField latest = gf;
        Tuple<int, int>[] candidates =
        Enumerable.Range(0, latest.size.Item1).SelectMany(row => Enumerable.Range(0, latest.size.Item2), (first, second) => new Tuple<int, int>(first, second)).Where(x => latest.isValidPiecePlacement(x)).ToArray();

        return new Tuple<Tuple<int, int>, int>(candidates[rand.Next(candidates.Length)], rand.Next(5) == 0 ? 4 : 2);
    }

}

class Evaluation2
{
    public static double eval(GameField gf)
    {                           //0.5Ç…ïœçX                          //log()Çí«â¡
        return smoothness(gf) * 1.0 + Monotonicity(gf) * 1.0 + Math.Log(emptyCells(gf)) * 2.7 + MaxValue(gf) * 1.0;
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
        if (count == 0)
        {
            return 0.5;
        }
        else
        {
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

        //6/2ïœçX
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