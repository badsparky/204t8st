using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class B22Placer : PlaceAgent
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

}
