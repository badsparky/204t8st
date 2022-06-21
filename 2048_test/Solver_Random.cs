using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class RandomSolver : SolveAgent
{
    public RandomSolver(Random random) : base(random) {}

    public override string solve(List<GameField> history)
    {
        GameField latest = history[history.Count - 1];
        HashSet<string> candidates = new HashSet<string>();
        foreach(var x in new string[]{"Up", "Down", "Left", "Right"}){ if(latest.isValidSlide(x)){ candidates.Add(x); } }
        string[] arrayed = candidates.ToArray();
        return arrayed[rand.Next(arrayed.Length)];
    }
}